using GrowMate.Contracts.Requests.Order;
using GrowMate.Contracts.Responses.Order;
using GrowMate.Services.Carts;
using GrowMate.Services.Customers;
using GrowMate.Services.Farmers;
using GrowMate.Services.Orders;
using GrowMate.Services.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace GrowMate.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;
        private readonly ICustomerService _customerService;
        private readonly IUserService _userService;
        private readonly IFarmerService _farmerService;

        public OrderController(IOrderService orderService,
            ICartService cartService,
            ICustomerService customerService,
            IUserService userService,
            IFarmerService farmerService)
        {
            _orderService = orderService;
            _cartService = cartService;
            _customerService = customerService;
            _userService = userService;
            _farmerService = farmerService;
        }

        private async Task<int?> GetCurrentCustomerIdAsync()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return null;
            }

            var customer = await _customerService.GetCustomerDetailsByIdAsync(userId, HttpContext.RequestAborted);
            if (customer == null)
            {
                return null;
            }

            return customer.CustomerId;
        }

        /// <summary>
        /// Create a new order from the customer's cart.
        /// </summary>
        /// <remarks>Role: Authenticated Customer</remarks>
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            try
            {
                var customerId = await GetCurrentCustomerIdAsync();
                if (customerId == null)
                {
                    return NotFound(new { message = "We couldn't find your customer profile. Please log in again or contact support." });
                }

                var cart = await _cartService.GetCartByCustomerIdAsync(customerId.Value);

                if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
                {
                    return BadRequest(new { message = "Your cart is empty." });
                }

                var order = await _orderService.CreateOrderFromCartAsync(
                    cart.CartId,
                    request?.ShippingAddress,
                    request?.Notes
                );

                var response = MapOrderToResponse(order);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Order creation error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { message = "An unexpected error occurred while creating the order." });
            }
        }

        /// <summary>
        /// Get a specific order by ID.
        /// </summary>
        /// <remarks>Role: Authenticated Customer (own orders only)</remarks>
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrder(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                return NotFound(new { message = "Order not found." });
            }

            var customerId = await GetCurrentCustomerIdAsync();
            if (customerId == null)
            {
                return NotFound(new { message = "We couldn't find your customer profile. Please log in again or contact support." });
            }

            if (order.CustomerId != customerId.Value)
            {
                return Forbid();
            }

            var response = MapOrderToResponse(order);
            return Ok(response);
        }

        /// <summary>
        /// Get all orders for the current customer.
        /// </summary>
        /// <remarks>Role: Authenticated Customer</remarks>
        [HttpGet]
        public async Task<IActionResult> GetMyOrders()
        {
            var customerId = await GetCurrentCustomerIdAsync();
            if (customerId == null)
            {
                return NotFound(new { message = "We couldn't find your customer profile. Please log in again or contact support." });
            }

            var orders = await _orderService.GetOrdersByCustomerIdAsync(customerId.Value);

            if (orders.Count == 0)
            {
                return NotFound(new { message = "No orders found for your account." });
            }

            var responses = orders.Select(MapOrderToResponse).ToList();
            return Ok(responses);
        }

        /// <summary>
        /// Update the status of an order (Processing/Shipped/Delivered/Completed/Cancelled).
        /// </summary>
        /// <remarks>Role: Admin or Farmer</remarks>
        [HttpPut("{orderId}/status")]
        [Authorize(Roles = "Admin,Farmer")]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromBody] UpdateOrderStatusRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Invalid request." });
            }

            try
            {
                var success = await _orderService.UpdateOrderStatusAsync(orderId, request.Status, request.Reason);
                if (!success)
                {
                    return NotFound(new { message = "Order not found." });
                }

                var updatedOrder = await _orderService.GetOrderByIdAsync(orderId);

                var response = MapOrderToResponse(updatedOrder);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update the payment status of an order (Paid/Pending/Failed).
        /// </summary>
        /// <remarks>Role: Admin only</remarks>
        [HttpPut("{orderId}/payment")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdatePaymentStatus(int orderId, [FromBody] UpdatePaymentStatusRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Invalid request." });
            }

            try
            {
                var success = await _orderService.UpdatePaymentStatusAsync(orderId, request.PaymentStatus, request.TransactionId);
                if (!success)
                {
                    return NotFound(new { message = "Order not found." });
                }

                var updatedOrder = await _orderService.GetOrderByIdAsync(orderId);

                var response = MapOrderToResponse(updatedOrder);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Helper method to map an order entity to a response model
        private OrderResponse MapOrderToResponse(Models.Order order)
        {
            if (order == null)
            {
                return null;
            }
            string customerName = "Unknown Customer";
            string sellerName = "Unknown Seller";
            try
            {
                // Get customer name
                var customer = _customerService.GetCustomerDetailsByIdAsync(order.CustomerId, HttpContext.RequestAborted).Result;
                if (customer != null)
                {
                    var user = _userService.GetUserByIdAsync(customer.UserId, false, HttpContext.RequestAborted).Result;
                    if (user != null)
                    {
                        customerName = user.FullName ?? "Customer #" + order.CustomerId;
                        Console.WriteLine($"Found user name: {customerName} for customer ID {order.CustomerId}");
                    }
                }
                if (order.SellerId > 0)
                {
                    // Try to get the farmer directly from the user service
                    var farmerUser = _farmerService.GetFarmerDetailByIdAsync(order.SellerId, HttpContext.RequestAborted).Result;
                    if (farmerUser != null)
                    {
                        sellerName = farmerUser.FarmName ?? "Seller #" + order.SellerId;
                        Console.WriteLine($"Found farm name: {sellerName} for seller ID {order.SellerId}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user names: {ex.Message}");
            }

            var response = new OrderResponse
            {
                OrderId = order.OrderId,
                CustomerId = order.CustomerId,
                CustomerName = customerName,
                SellerId = order.SellerId,
                SellerName = sellerName,
                Status = order.Status,
                PaymentStatus = order.PaymentStatus,
                Currency = order.Currency,
                Subtotal = order.Subtotal,
                ShippingFee = order.ShippingFee,
                TotalAmount = order.TotalAmount,
                ShippingAddress = GetFormattedShippingAddress(order),
                Notes = order.Note,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                OrderItems = new List<OrderItemResponse>()
            };

            if (order.OrderItems != null)
            {
                foreach (var item in order.OrderItems)
                {
                    OrderItemResponse orderItemResponse;

                    if (item.ProductId.HasValue)
                    {
                        // Product item
                        orderItemResponse = new ProductOrderItemResponse
                        {
                            OrderItemId = item.OrderItemId,
                            OrderId = item.OrderId,
                            ProductId = item.ProductId.Value,
                            ProductName = item.ProductName ?? item.Product?.Name ?? "Unknown Product",
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice,
                            TotalPriceFromDb = item.TotalPrice,
                            CreatedAt = item.CreatedAt,
                            ProductImageUrl = item.Product?.Media?.FirstOrDefault(m => m.IsPrimary)?.MediaUrl ?? item.Product?.Media?.FirstOrDefault()?.MediaUrl ?? ""
                        };
                    }
                    else if (item.ListingId.HasValue)
                    {
                        // Tree listing item
                        var post = item.Listing?.Post;
                        orderItemResponse = new TreeOrderItemResponse
                        {
                            OrderItemId = item.OrderItemId,
                            OrderId = item.OrderId,
                            ListingId = item.ListingId.Value,
                            ProductName = item.Listing?.Post?.ProductName ?? "Unknown Tree",
                            ProductType = post?.ProductType ?? "",
                            ProductVariety = post?.ProductVariety ?? "",
                            FarmName = post?.FarmName ?? "",
                            TreeQuantity = item.TreeQuantity ?? 0,
                            TreeUnitPrice = item.TreeUnitPrice ?? 0,
                            TreeYears = item.TreeYears ?? 1,
                            TreeTotalPriceFromDb = item.TreeTotalPrice,
                            CreatedAt = item.CreatedAt,
                            ProductImageUrl = post?.Media?.FirstOrDefault(m => m.IsPrimary)?.MediaUrl ?? post?.Media?.FirstOrDefault()?.MediaUrl ?? ""
                        };
                    }
                    else
                    {
                        // Fallback - shouldn't happen
                        orderItemResponse = new ProductOrderItemResponse
                        {
                            OrderItemId = item.OrderItemId,
                            OrderId = item.OrderId,
                            ProductId = 0,
                            ProductName = "Unknown Item",
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice,
                            TotalPriceFromDb = item.TotalPrice,
                            CreatedAt = item.CreatedAt,
                            ProductImageUrl = ""
                        };
                    }

                    response.OrderItems.Add(orderItemResponse);
                }
            }

            return response;
        }
        private string GetFormattedShippingAddress(Models.Order order)
        {
            if (order == null) return string.Empty;

            return string.Join(", ", new[]
            {
                order.ShipFullName,
                order.ShipPhone,
                order.ShipAddress,
                order.ShipCity,
                order.ShipState,
                order.ShipPostalCode,
                order.ShipCountry
            }.Where(s => !string.IsNullOrWhiteSpace(s)));
        }
    }
}

