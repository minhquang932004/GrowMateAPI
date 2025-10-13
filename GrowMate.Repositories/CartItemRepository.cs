using GrowMate.Models;
using GrowMate.Repositories.Data;
using GrowMate.Repositories.Interfaces;

namespace GrowMate.Repositories;

public class CartItemRepository : GenericRepository<CartItem>, ICartItemRepository
{
    public CartItemRepository(EXE201_GrowMateContext context) : base(context)
    {
    }
    
    /// <summary>
    /// Adds multiple cart items to the database at once
    /// </summary>
    public async Task AddRangeAsync(IEnumerable<CartItem> cartItems)
    {
        await _context.CartItems.AddRangeAsync(cartItems);
    }
    
    /// <summary>
    /// Removes multiple cart items from the database at once
    /// </summary>
    public void RemoveRange(IEnumerable<CartItem> cartItems)
    {
        _context.CartItems.RemoveRange(cartItems);
    }
}
