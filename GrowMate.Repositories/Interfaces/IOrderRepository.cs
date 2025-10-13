using GrowMate.Models;

namespace GrowMate.Repositories.Interfaces;

public interface IOrderRepository : IGenericRepository<Order>
{
    Task<Order> GetByIdAsync(int orderId);
    Task<List<Order>> GetByCustomerIdAsync(int customerId);
}
