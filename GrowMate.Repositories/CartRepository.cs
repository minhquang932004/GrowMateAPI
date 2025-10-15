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
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Listing)
            .ThenInclude(l => l.Post)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId && c.Status == "Active");
    }
}
