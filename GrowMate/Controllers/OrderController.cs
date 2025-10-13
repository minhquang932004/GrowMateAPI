using GrowMate.Contracts.Requests.Order;
using GrowMate.Contracts.Responses.Order;
using GrowMate.Services.Carts;
using GrowMate.Services.Orders;
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

        public OrderController(IOrderService orderService, ICartService cartService)
        {
            _orderService = orderService;
            _cartService = cartService;
        }

        private int GetCurrentCustomerId()
        {
            var customerIdClaim = User.FindFirst("CustomerId");
            if (customerIdClaim == null || !int.TryParse(customerIdClaim.Value, out var customerId))
            {
                throw new InvalidOperationException("User is not a valid customer.");
            }
            return customerId;
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
                // Get the current customer's ID from their claims
                var customerId = GetCurrentCustomerId();
                
                // Get the customer's active cart
                var cart = await _cartService.GetCartByCustomerIdAsync(customerId);

                // Check if the cart exists and has items
                if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
                {
                    return BadRequest(new { message = "Your cart is empty." });
                }

                // Create an order from the cart
                var order = await _orderService.CreateOrderFromCartAsync(
                    cart.CartId,
                    request?.ShippingAddress,
                    request?.Notes,
                    request?.PaymentMethod
                );
                
                // Map the order to a response model
                var response = MapOrderToResponse(order);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // In a real application, you would log this exception
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
            // Get the order from the database
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                return NotFound(new { message = "Order not found." });
            }

            // Security check: Ensure the current customer is the owner of this order
            var customerId = GetCurrentCustomerId();
            if (order.CustomerId != customerId)
            {
                return Forbid(); // Use Forbid() to indicate they are not allowed, or NotFound() to hide the order's existence
            }

            // Map the order to a response model
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
            // Get the current customer's ID from their claims
            var customerId = GetCurrentCustomerId();
            
            // Get all orders for this customer
            var orders = await _orderService.GetOrdersByCustomerIdAsync(customerId);
            
            // Map the orders to response models
            var responses = orders.Select(MapOrderToResponse).ToList();
            return Ok(responses);
        }
        
        /// <summary>
        /// Update the status of an order (Processing/Shipped/Delivered/Completed/Cancelled).
        /// </summary>
        /// <remarks>Role: Admin or Farmer</remarks>
        [HttpPut("{orderId}/status")]
        [Authorize(Roles = "Admin,Farmer")] // Only admins and farmers can update order status
        public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromBody] UpdateOrderStatusRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Invalid request." });
            }
            
            try
            {
                // Update the order status
                var success = await _orderService.UpdateOrderStatusAsync(orderId, request.Status, request.Reason);
                if (!success)
                {
                    return NotFound(new { message = "Order not found." });
                }
                
                // Get the updated order
                var updatedOrder = await _orderService.GetOrderByIdAsync(orderId);
                
                // Map the order to a response model
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
        [Authorize(Roles = "Admin")] // Only admins can update payment status
        public async Task<IActionResult> UpdatePaymentStatus(int orderId, [FromBody] UpdatePaymentStatusRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Invalid request." });
            }
            
            try
            {
                // Update the payment status
                var success = await _orderService.UpdatePaymentStatusAsync(orderId, request.PaymentStatus, request.TransactionId);
                if (!success)
                {
                    return NotFound(new { message = "Order not found." });
                }
                
                // Get the updated order
                var updatedOrder = await _orderService.GetOrderByIdAsync(orderId);
                
                // Map the order to a response model
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
            
            // Create the order response
            var response = new OrderResponse
            {
                OrderId = order.OrderId,
                CustomerId = order.CustomerId,
                CustomerName = order.Customer?.User?.FullName ?? "Unknown Customer",
                SellerId = order.SellerId,
                SellerName = order.Seller?.FarmName ?? "Unknown Seller",
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
            
            // Add the order items
            if (order.OrderItems != null)
            {
                foreach (var item in order.OrderItems)
                {
                    response.OrderItems.Add(new OrderItemResponse
                    {
                        OrderItemId = item.OrderItemId,
                        OrderId = item.OrderId,
                        ProductId = item.ProductId,
                        ProductName = item.Product?.Name ?? "Unknown Product",
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        CreatedAt = item.CreatedAt,
                    });
                }
            }
            
            return response;
        }

        // Helper method to format a complete shipping address from the Order's address fields
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

