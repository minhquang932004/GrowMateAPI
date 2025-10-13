using GrowMate.Models;

namespace GrowMate.Repositories.Interfaces;

public interface ICartItemRepository : IGenericRepository<CartItem>
{
    /// <summary>
    /// Adds multiple cart items to the database at once
    /// </summary>
    Task AddRangeAsync(IEnumerable<CartItem> cartItems);
    
    /// <summary>
    /// Removes multiple cart items from the database at once
    /// </summary>
    void RemoveRange(IEnumerable<CartItem> cartItems);
}
    