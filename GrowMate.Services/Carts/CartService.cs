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
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
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
                    CreatedAt = DateTime.Now
                };
                await _unitOfWork.CartItems.AddAsync(cartItem);
            }
            
            // Update the cart's timestamp
            cart.UpdatedAt = DateTime.Now;
            
            await _unitOfWork.SaveChangesAsync();

            // Return the updated cart
            return await _unitOfWork.Carts.GetByCustomerIdAsync(customerId);
        }

        public async Task<Cart> AddTreeToCartAsync(int customerId, int listingId, int quantity)
        {
            // Get or create cart
            var cart = await _unitOfWork.Carts.GetByCustomerIdAsync(customerId);
            if (cart == null)
            {
                cart = new Cart
                {
                    CustomerId = customerId,
                    Status = "Active",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                await _unitOfWork.Carts.AddAsync(cart);
                await _unitOfWork.SaveChangesAsync();
            }

            // Fetch listing and validate
            var listing = await _unitOfWork.TreeListings.GetByIdAsync(listingId, includeTrees: false);
            if (listing == null)
            {
                throw new InvalidOperationException("Tree listing not found");
            }
            if (quantity <= 0)
            {
                throw new InvalidOperationException("Quantity must be greater than zero");
            }
            if (listing.AvailableQuantity < quantity)
            {
                throw new InvalidOperationException("Not enough available trees in this listing");
            }

            // Find existing cart item by listing
            var cartItem = cart.CartItems?.FirstOrDefault(ci => ci.ListingId == listingId);
            if (cartItem != null)
            {
                cartItem.TreeQuantity = (cartItem.TreeQuantity ?? 0) + quantity;
            }
            else
            {
                cartItem = new CartItem
                {
                    CartId = cart.CartId,
                    ListingId = listingId,
                    TreeQuantity = quantity,
                    TreeUnitPrice = listing.PricePerTree,
                    CreatedAt = DateTime.Now
                };
                await _unitOfWork.CartItems.AddAsync(cartItem);
            }

            cart.UpdatedAt = DateTime.Now;
            await _unitOfWork.SaveChangesAsync();

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
                cart.UpdatedAt = DateTime.Now;
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
                throw new InvalidOperationException("Cart item not found");
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
                // Update the correct quantity field based on item type
                if (cartItem.ProductId.HasValue)
                {
                    cartItem.Quantity = quantity;
                }
                else if (cartItem.ListingId.HasValue)
                {
                    // Optional: validate against available quantity of listing
                    var listing = await _unitOfWork.TreeListings.GetByIdAsync(cartItem.ListingId.Value, includeTrees: false);
                    if (listing == null)
                    {
                        throw new InvalidOperationException("Tree listing not found");
                    }
                    if (quantity > listing.AvailableQuantity)
                    {
                        throw new InvalidOperationException("Requested tree quantity exceeds available quantity");
                    }
                    cartItem.TreeQuantity = quantity;
                }
                
                // Update the cart's timestamp
                var cart = await _unitOfWork.Carts.GetByIdAsync(cartItem.CartId);
                if (cart != null)
                {
                    cart.UpdatedAt = DateTime.Now;
                }
                
                await _unitOfWork.SaveChangesAsync();
                return cartItem;
            }
        }
    }
}
