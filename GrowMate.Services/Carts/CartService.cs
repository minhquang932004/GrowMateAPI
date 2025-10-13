using GrowMate.Models;
using GrowMate.Repositories.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GrowMate.Services.Carts
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CartService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Cart> AddToCartAsync(int customerId, int productId, int quantity)
        {
            // First, get the customer's cart. If they don't have one, create it.
            var cart = await _unitOfWork.Carts.GetByCustomerIdAsync(customerId);

            if (cart == null)
            {
                // Create a new cart for the customer
                cart = new Cart
                {
                    CustomerId = customerId,
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                await _unitOfWork.Carts.AddAsync(cart);
                await _unitOfWork.SaveChangesAsync(); // Save to get the CartId
            }

            // Fetch the product to get its current price
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
            {
                throw new Exception("Product not found");
            }

            // Check if the product is already in the cart
            var cartItem = cart.CartItems?.FirstOrDefault(ci => ci.ProductId == productId);

            if (cartItem != null)
            {
                // Update the quantity if the item is already in the cart
                cartItem.Quantity += quantity;
            }
            else
            {
                // Add a new cart item
                cartItem = new CartItem
                {
                    CartId = cart.CartId,
                    ProductId = productId,
                    Quantity = quantity,
                    UnitPrice = product.Price,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.CartItems.AddAsync(cartItem);
            }
            
            // Update the cart's timestamp
            cart.UpdatedAt = DateTime.UtcNow;
            
            await _unitOfWork.SaveChangesAsync();

            // Return the updated cart
            return await _unitOfWork.Carts.GetByCustomerIdAsync(customerId);
        }

        public async Task<Cart> GetCartByCustomerIdAsync(int customerId)
        {
            return await _unitOfWork.Carts.GetByCustomerIdAsync(customerId);
        }

        public async Task<bool> RemoveFromCartAsync(int cartItemId)
        {
            // Find the cart item
            var cartItem = await _unitOfWork.CartItems.GetByIdAsync(cartItemId);
            if (cartItem == null)
            {
                return false;
            }

            // Remove the item from the cart
            _unitOfWork.CartItems.Remove(cartItem);
            
            // Update the cart's timestamp
            var cart = await _unitOfWork.Carts.GetByIdAsync(cartItem.CartId);
            if (cart != null)
            {
                cart.UpdatedAt = DateTime.UtcNow;
            }

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<CartItem> UpdateCartItemQuantityAsync(int cartItemId, int quantity)
        {
            // Find the cart item
            var cartItem = await _unitOfWork.CartItems.GetByIdAsync(cartItemId);
            if (cartItem == null)
            {
                return null;
            }

            if (quantity <= 0)
            {
                // Remove the item if quantity is zero or negative
                _unitOfWork.CartItems.Remove(cartItem);
                await _unitOfWork.SaveChangesAsync();
                return null;
            }
            else
            {
                // Update the quantity
                cartItem.Quantity = quantity;
                
                // Update the cart's timestamp
                var cart = await _unitOfWork.Carts.GetByIdAsync(cartItem.CartId);
                if (cart != null)
                {
                    cart.UpdatedAt = DateTime.UtcNow;
                }
                
                await _unitOfWork.SaveChangesAsync();
                return cartItem;
            }
        }
    }
}
