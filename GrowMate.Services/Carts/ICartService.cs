using GrowMate.Models;
using System.Threading.Tasks;

namespace GrowMate.Services.Carts
{
    public interface ICartService
    {
        Task<Cart> AddToCartAsync(int customerId, int productId, int quantity);
        Task<Cart> AddTreeToCartAsync(int customerId, int listingId, int quantity, int years);
        Task<Cart> GetCartByCustomerIdAsync(int customerId);
        Task<bool> RemoveFromCartAsync(int cartItemId);
        Task<CartItem> UpdateCartItemQuantityAsync(int cartItemId, int quantity, int? years = null);
    }
}
