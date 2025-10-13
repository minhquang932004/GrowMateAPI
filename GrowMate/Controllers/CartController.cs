using GrowMate.Contracts.Requests;
using GrowMate.Contracts.Requests.Cart;
using GrowMate.Contracts.Responses.Cart;
using GrowMate.Services.Carts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrowMate.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Require authentication for all cart operations
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        private int GetCurrentCustomerId()
        {
            // This is a common way to get the user's ID from the claims.
            // Ensure your token generation includes the "CustomerId" claim.
            var customerIdClaim = User.FindFirst("CustomerId");
            if (customerIdClaim == null || !int.TryParse(customerIdClaim.Value, out var customerId))
            {
                throw new InvalidOperationException("User is not a valid customer.");
            }
            return customerId;
        }

        /// <summary>
        /// Get the current customer's shopping cart.
        /// </summary>
        /// <remarks>Role: Authenticated Customer</remarks>
        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var customerId = GetCurrentCustomerId();
            var cart = await _cartService.GetCartByCustomerIdAsync(customerId);
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
                return BadRequest("Invalid request payload.");
            }

            var customerId = GetCurrentCustomerId();
            var cart = await _cartService.AddToCartAsync(customerId, request.ProductId, request.Quantity);
            
            // Map the cart to a response model
            var response = MapCartToResponse(cart);
            return Ok(response);
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
                return BadRequest("Invalid request payload.");
            }

            var updatedItem = await _cartService.UpdateCartItemQuantityAsync(cartItemId, request.Quantity);
            if (updatedItem == null && request.Quantity > 0)
            {
                return NotFound("Cart item not found.");
            }

            var cart = await _cartService.GetCartByCustomerIdAsync(GetCurrentCustomerId());
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
            var success = await _cartService.RemoveFromCartAsync(cartItemId);
            if (!success)
            {
                return NotFound("Cart item not found.");
            }
            
            var cart = await _cartService.GetCartByCustomerIdAsync(GetCurrentCustomerId());
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
                    // Get the first media item's URL or null if no media exists
                    string imageUrl = item.Product?.Media?.FirstOrDefault()?.MediaUrl;
                    
                    response.CartItems.Add(new CartItemResponse
                    {
                        CartItemId = item.CartItemId,
                        CartId = item.CartId,
                        ProductId = item.ProductId,
                        ProductName = item.Product?.Name ?? "Unknown Product",
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        CreatedAt = item.CreatedAt,
                        ProductImageUrl = imageUrl
                    });
                }
            }
            return response;
        }
    }
}
