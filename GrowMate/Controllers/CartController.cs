using GrowMate.Contracts.Requests.Cart;
using GrowMate.Contracts.Responses.Cart;
using GrowMate.Services.Carts;
using GrowMate.Services.Customers; // Add this using if you have a customer service
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;

namespace GrowMate.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Require authentication for all cart operations
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly ICustomerService _customerService; // Inject customer service

        public CartController(ICartService cartService, ICustomerService customerService)
        {
            _cartService = cartService;
            _customerService = customerService;
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
        /// Get the current customer's shopping cart.
        /// </summary>
        /// <remarks>Role: Authenticated Customer</remarks>
        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var customerId = await GetCurrentCustomerIdAsync();
            if (customerId == null)
            {
                return NotFound(new { Message = "We couldn't find your customer profile. Please contact support if this is unexpected." });
            }

            var cart = await _cartService.GetCartByCustomerIdAsync(customerId.Value);
            if (cart == null)
            {
                return Ok(new { Message = "Your cart is empty." });
            }

            // Map the cart to a response model
            var response = MapCartToResponse(cart);
            return Ok(response);
        }

        /// <summary>
        /// Add a product to the customer's cart.
        /// </summary>
        /// <remarks>Role: Authenticated Customer</remarks>
        [HttpPost("items")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            if (request == null || request.Quantity <= 0)
            {
                return BadRequest(new { Message = "Invalid request payload." });
            }

            var customerId = await GetCurrentCustomerIdAsync();
            if (customerId == null)
            {
                return NotFound(new { Message = "We couldn't find your customer profile. Please contact support if this is unexpected." });
            }

            var cart = await _cartService.AddToCartAsync(customerId.Value, request.ProductId, request.Quantity);

            // Map the cart to a response model
            var response = MapCartToResponse(cart);
            return Ok(response);
        }

        /// <summary>
        /// Add a tree listing (adoption) to the customer's cart.
        /// </summary>
        /// <remarks>Role: Authenticated Customer</remarks>
        [HttpPost("trees")]
        public async Task<IActionResult> AddTreeToCart([FromBody] AddTreeToCartRequest request)
        {
            if (request == null || request.Quantity <= 0 || request.Years <= 0)
            {
                return BadRequest(new { Message = "Invalid request payload." });
            }

            var customerId = await GetCurrentCustomerIdAsync();
            if (customerId == null)
            {
                return NotFound(new { Message = "We couldn't find your customer profile. Please contact support if this is unexpected." });
            }

            try
            {
                var cart = await _cartService.AddTreeToCartAsync(customerId.Value, request.ListingId, request.Quantity, request.Years);
                var response = MapCartToResponse(cart);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Update the quantity of an item in the cart.
        /// </summary>
        /// <remarks>Role: Authenticated Customer</remarks>
        [HttpPut("items/{cartItemId}")]
        public async Task<IActionResult> UpdateCartItem(int cartItemId, [FromBody] UpdateCartItemRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { Message = "Invalid request payload." });
            }

            var customerId = await GetCurrentCustomerIdAsync();
            if (customerId == null)
            {
                return NotFound(new { Message = "We couldn't find your customer profile. Please contact support if this is unexpected." });
            }

            try
            {
                var updatedItem = await _cartService.UpdateCartItemQuantityAsync(cartItemId, request.Quantity, request.Years);
            }
            catch (InvalidOperationException)
            {
                return NotFound(new { Message = "Cart item not found." });
            }

            var cart = await _cartService.GetCartByCustomerIdAsync(customerId.Value);
            var response = MapCartToResponse(cart);
            return Ok(response);
        }

        /// <summary>
        /// Remove an item from the cart.
        /// </summary>
        /// <remarks>Role: Authenticated Customer</remarks>
        [HttpDelete("items/{cartItemId}")]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var customerId = await GetCurrentCustomerIdAsync();
            if (customerId == null)
            {
                return NotFound(new { Message = "We couldn't find your customer profile. Please contact support if this is unexpected." });
            }

            var success = await _cartService.RemoveFromCartAsync(cartItemId);
            if (!success)
            {
                return NotFound(new { Message = "Cart item not found." });
            }

            var cart = await _cartService.GetCartByCustomerIdAsync(customerId.Value);
            var response = MapCartToResponse(cart);
            return Ok(response);
        }

        // Helper method to map a cart entity to a response model
        private CartResponse MapCartToResponse(Models.Cart cart)
        {
            if (cart == null)
            {
                return null;
            }

            var response = new CartResponse
            {
                CartId = cart.CartId,
                CustomerId = cart.CustomerId,
                Status = cart.Status,
                CreatedAt = cart.CreatedAt,
                UpdatedAt = cart.UpdatedAt,
                CartItems = new List<CartItemResponse>()
            };

            if (cart.CartItems != null)
            {
                foreach (var item in cart.CartItems)
                {
                    // If this is a product item
                    if (item.ProductId.HasValue)
                    {
                        string imageUrl = item.Product?.Media?.FirstOrDefault(m => m.IsPrimary)?.MediaUrl ?? item.Product?.Media?.FirstOrDefault()?.MediaUrl;
                        response.CartItems.Add(new CartItemResponse
                        {
                            CartItemId = item.CartItemId,
                            CartId = item.CartId,
                            ProductId = item.ProductId ?? 0,
                            ListingId = null,
                            ProductName = item.Product?.Name ?? "Unknown Product",
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice,
                            CreatedAt = item.CreatedAt,
                            ProductImageUrl = imageUrl
                        });
                    }
                    else if (item.ListingId.HasValue)
                    {
                        // Tree listing item mapping - reuse fields
                        response.CartItems.Add(new CartItemResponse
                        {
                            CartItemId = item.CartItemId,
                            CartId = item.CartId,
                            ProductId = 0,
                            ListingId = item.ListingId,
                            ProductName = item.Listing?.Post?.ProductName ?? "Tree Listing",
                            Quantity = item.TreeQuantity ?? 0,
                            UnitPrice = item.TreeUnitPrice ?? 0,
                            CreatedAt = item.CreatedAt,
                            ProductImageUrl = item.Listing?.Post?.Media?.FirstOrDefault(m => m.IsPrimary)?.MediaUrl ?? item.Listing?.Post?.Media?.FirstOrDefault()?.MediaUrl,
                            Years = item.TreeYears
                        });
                    }
                }
            }
            return response;
        }
    }
}
