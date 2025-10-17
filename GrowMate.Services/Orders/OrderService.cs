using GrowMate.Contracts.Utils;
using GrowMate.Models;
using GrowMate.Repositories.Interfaces;
using GrowMate.Repositories.Models.Statuses;

namespace GrowMate.Services.Orders
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Order?> CreateOrderFromCartAsync(int cartId,
            string? shippingAddress = null, string? notes = null)
        {
            // We'll use ExecuteInTransactionAsync to ensure all operations succeed or fail together
            Order? createdOrder = null;

            await _unitOfWork.ExecuteInTransactionAsync(async (ct) =>
            {
                // Step 1: Get the cart with its items and product details
                var cart = await _unitOfWork.Carts.GetByIdAsync(cartId);
                if (cart == null || cart.Status != "Active" || cart.CartItems == null || !cart.CartItems.Any())
                {
                    throw new InvalidOperationException("Cart is empty, not found, or not active.");
                }

                // Step 2: Calculate order amounts
                decimal subtotal = 0;

                // Calculate subtotal from cart items (products + trees)
                foreach (var item in cart.CartItems)
                {
                    if (item.ProductId.HasValue)
                    {
                        subtotal += item.Quantity * item.UnitPrice;
                    }
                    else if (item.ListingId.HasValue)
                    {
                        subtotal += (item.TreeQuantity ?? 0) * (item.TreeUnitPrice ?? 0);
                    }
                }

                // Calculate shipping fee (can be based on distance, weight, etc.)
                // For now, use a simple fixed fee
                decimal shippingFee = 30000; // 30,000 VND

                // Calculate total amount (subtotal + shipping fee)
                decimal totalAmount = subtotal + shippingFee;

                // Determine seller and order type
                var firstCartItem = cart.CartItems.FirstOrDefault();
                int sellerId;
                string orderType;
                if (firstCartItem?.ProductId != null && firstCartItem?.Product != null)
                {
                    sellerId = firstCartItem.Product.FarmerId;
                    orderType = cart.CartItems.Any(ci => ci.ListingId.HasValue) ? "mixed" : "products";
                }
                else if (firstCartItem?.ListingId != null)
                {
                    // Load listing to get farmer
                    var listing = await _unitOfWork.TreeListings.GetByIdAsync(firstCartItem.ListingId.Value, includeTrees: false, ct);
                    if (listing == null)
                    {
                        throw new InvalidOperationException("Invalid tree listing in cart.");
                    }
                    sellerId = listing.FarmerId;
                    orderType = cart.CartItems.Any(ci => ci.ProductId != 0) ? "mixed" : "adoption";
                }
                else
                {
                    throw new InvalidOperationException("Cart items are invalid.");
                }

                // Step 3: Create a new order
                var order = new Order
                {
                    CustomerId = cart.CustomerId,
                    // Get the first seller ID from cart items
                    SellerId = sellerId,
                    OrderType = orderType,
                    Status = OrderStatuses.Pending, // Initial order status
                    PaymentStatus = PaymentStatuses.Pending, // Initial payment status
                    Currency = "VND", // Vietnamese Dong
                    Subtotal = CurrencyUtils.RoundToNearestThousand(subtotal), // Round to nearest 1000 VND
                    ShippingFee = shippingFee,
                    TotalAmount = CurrencyUtils.RoundToNearestThousand(totalAmount), // Round to nearest 1000 VND
                    // Shipping information - now optional
                    ShipAddress = shippingAddress, // Use provided address or null
                    Note = notes, // Use the core property directly
                    OrderItems = new List<OrderItem>(),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                // Step 4: Create order items from cart items
                foreach (var cartItem in cart.CartItems)
                {
                    if (cartItem.ProductId.HasValue)
                    {
                        var product = await _unitOfWork.Products.GetByIdAsync(cartItem.ProductId.Value);
                        if (product == null)
                        {
                            throw new InvalidOperationException($"Product with ID {cartItem.ProductId} not found.");
                        }
                        if (product.Stock < cartItem.Quantity)
                        {
                            throw new InvalidOperationException($"Not enough stock for product: {product.Name}. Available: {product.Stock}, Requested: {cartItem.Quantity}");
                        }
                        var orderItem = new OrderItem
                        {
                            ProductId = cartItem.ProductId.Value,
                            ProductName = product.Name,
                            Quantity = cartItem.Quantity,
                            UnitPrice = cartItem.UnitPrice,
                            TotalPrice = cartItem.UnitPrice * cartItem.Quantity,
                            CreatedAt = DateTime.Now
                        };
                        product.Stock -= cartItem.Quantity;
                        _unitOfWork.Products.Update(product);
                        order.OrderItems.Add(orderItem);
                    }
                    else if (cartItem.ListingId.HasValue)
                    {
                        var listing = await _unitOfWork.TreeListings.GetByIdAsync(cartItem.ListingId.Value, includeTrees: false, ct);
                        if (listing == null)
                        {
                            throw new InvalidOperationException($"Tree listing with ID {cartItem.ListingId} not found.");
                        }

                        // Get product name from the associated post
                        var post = listing.Post;
                        if (post == null)
                        {
                            throw new InvalidOperationException($"Post not found for tree listing with ID {cartItem.ListingId}.");
                        }

                        var orderItem = new OrderItem
                        {
                            ProductId = null, // Null for tree items (will be made nullable in DB)
                            ProductName = null, // Null for tree items (will be made nullable in DB)
                            UnitPrice = 0, // Required field - not used for trees
                            Quantity = 0, // Required field - not used for trees
                            TotalPrice = null, // Null for tree items (will be made nullable in DB)
                            ListingId = cartItem.ListingId,
                            TreeQuantity = cartItem.TreeQuantity,
                            TreeUnitPrice = cartItem.TreeUnitPrice,
                            TreeYears = cartItem.TreeYears,
                            TreeTotalPrice = (cartItem.TreeQuantity ?? 0) * (cartItem.TreeUnitPrice ?? 0),
                            CreatedAt = DateTime.Now
                        };
                        order.OrderItems.Add(orderItem);
                    }
                }

                // Step 5: Save the order
                await _unitOfWork.Orders.AddAsync(order);

                // Step 6: Clear the cart
                _unitOfWork.CartItems.RemoveRange(cart.CartItems);
                cart.Status = "Ordered"; // Change cart status
                cart.UpdatedAt = DateTime.Now;

                await _unitOfWork.SaveChangesAsync(ct);

                // Get the full order with all related data
                createdOrder = await _unitOfWork.Orders.GetByIdAsync(order.OrderId);
            });

            return createdOrder;
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            return await _unitOfWork.Orders.GetByIdAsync(orderId);
        }

        public async Task<List<Order>?> GetOrdersByCustomerIdAsync(int customerId)
        {
            return await _unitOfWork.Orders.GetByCustomerIdAsync(customerId);
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, string status, string? reason = null)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null)
            {
                return false;
            }

            // Convert input status to uppercase for case-insensitive comparison
            string normalizedStatus = status?.ToUpper() ?? string.Empty;

            // Validate the status is one of our defined constants
            if (normalizedStatus != OrderStatuses.Pending &&
                normalizedStatus != OrderStatuses.Processing &&
                normalizedStatus != OrderStatuses.Shipped &&
                normalizedStatus != OrderStatuses.Delivered &&
                normalizedStatus != OrderStatuses.Completed &&
                normalizedStatus != OrderStatuses.Cancelled &&
                normalizedStatus != OrderStatuses.Refunded)
            {
                throw new ArgumentException($"Invalid order status: {status}");
            }

            order.Status = normalizedStatus; // Store the uppercase version
            order.UpdatedAt = DateTime.Now;

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdatePaymentStatusAsync(int orderId, string paymentStatus, string? transactionId = null)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null)
            {
                return false;
            }

            // Better (case-insensitive)
            string normalizedPaymentStatus = paymentStatus?.ToUpper() ?? string.Empty;
            if (normalizedPaymentStatus != PaymentStatuses.Pending &&
                normalizedPaymentStatus != PaymentStatuses.Completed &&
                normalizedPaymentStatus != PaymentStatuses.Failed &&
                normalizedPaymentStatus != PaymentStatuses.Refunded &&
                normalizedPaymentStatus != PaymentStatuses.PartiallyRefunded &&
                normalizedPaymentStatus != PaymentStatuses.OnHold)
            {
                throw new ArgumentException($"Invalid payment status: {paymentStatus}");
            }

            // Fix: Use normalizedPaymentStatus instead of paymentStatus
            order.PaymentStatus = normalizedPaymentStatus;
            order.UpdatedAt = DateTime.Now;

            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}

