using GrowMate.Models;
using GrowMate.Repositories.Data;
using GrowMate.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GrowMate.Repositories;

public class OrderRepository : GenericRepository<Order>, IOrderRepository
{
    public OrderRepository(EXE201_GrowMateContext context) : base(context)
    {
    }

    public override async Task<Order> GetByIdAsync(int orderId)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);
    }

    public async Task<List<Order>> GetByCustomerIdAsync(int customerId)
    {
        return await _context.Orders
            .Where(o => o.CustomerId == customerId)
            .Include(o => o.OrderItems)
            .OrderByDescending(o => o.CreatedAt)  // Using CreatedAt instead of OrderDate based on the model
            .ToListAsync();
    }
}
