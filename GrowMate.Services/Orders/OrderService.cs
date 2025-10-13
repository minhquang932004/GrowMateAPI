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
            string? shippingAddress = null, string? notes = null, string? paymentMethod = null)
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
                
                // Calculate subtotal from cart items
                foreach (var item in cart.CartItems)
                {
                    subtotal += item.Quantity * item.UnitPrice;
                }
                
                // Calculate shipping fee (can be based on distance, weight, etc.)
                // For now, use a simple fixed fee
                decimal shippingFee = 30000; // 30,000 VND
                
                // Calculate total amount (subtotal + shipping fee)
                decimal totalAmount = subtotal + shippingFee;
                
                // Check if the first cart item and its product are not null
                var firstCartItem = cart.CartItems.FirstOrDefault();
                if (firstCartItem?.Product == null)
                {
                    throw new InvalidOperationException("Cart items have invalid products.");
                }
                
                // Step 3: Create a new order
                var order = new Order
                {
                    CustomerId = cart.CustomerId,
                    // Get the first seller ID from cart items
                    SellerId = firstCartItem.Product.FarmerId,
                    Status = OrderStatuses.Pending, // Initial order status
                    PaymentStatus = PaymentStatuses.Pending, // Initial payment status
                    Currency = "VND", // Vietnamese Dong
                    Subtotal = CurrencyUtils.RoundToNearestThousand(subtotal), // Round to nearest 1000 VND
                    ShippingFee = shippingFee,
                    TotalAmount = CurrencyUtils.RoundToNearestThousand(totalAmount), // Round to nearest 1000 VND
                    ShipAddress = shippingAddress, // Use the core property directly
                    Note = notes, // Use the core property directly
                    OrderItems = new List<OrderItem>(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Step 4: Create order items from cart items
                foreach (var cartItem in cart.CartItems)
                {
                    var product = await _unitOfWork.Products.GetByIdAsync(cartItem.ProductId);
                    
                    if (product == null)
                    {
                        throw new InvalidOperationException($"Product with ID {cartItem.ProductId} not found.");
                    }
                    
                    if (product.Stock < cartItem.Quantity)
                    {
                        throw new InvalidOperationException($"Not enough stock for product: {product.Name}. Available: {product.Stock}, Requested: {cartItem.Quantity}");
                    }

                    // Create order item
                    var orderItem = new OrderItem
                    {
                        ProductId = cartItem.ProductId,
                        Quantity = cartItem.Quantity,
                        UnitPrice = cartItem.UnitPrice,
                        CreatedAt = DateTime.UtcNow
                    };
                    
                    // Decrease product stock
                    product.Stock -= cartItem.Quantity;
                    _unitOfWork.Products.Update(product);
                    
                    // Add order item to order
                    order.OrderItems.Add(orderItem);
                }

                // Step 5: Save the order
                await _unitOfWork.Orders.AddAsync(order);
                
                // Step 6: Clear the cart
                _unitOfWork.CartItems.RemoveRange(cart.CartItems);
                cart.Status = "Ordered"; // Change cart status
                cart.UpdatedAt = DateTime.UtcNow;
                
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
            order.UpdatedAt = DateTime.UtcNow;
            
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
            order.UpdatedAt = DateTime.UtcNow;
            
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}

