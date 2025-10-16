using GrowMate.Models;
using GrowMate.Repositories.Data;
using GrowMate.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GrowMate.Repositories;

public class CartRepository : GenericRepository<Cart>, ICartRepository
{
    public CartRepository(EXE201_GrowMateContext context) : base(context)
    {
    }

    public async Task<Cart> GetByCustomerIdAsync(int customerId)
    {
        return await _context.Carts
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Product)
            .ThenInclude(p => p.Media)
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Listing)
            .ThenInclude(l => l.Post)
            .ThenInclude(p => p.Media)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId && c.Status == "Active");
    }
}
