using GrowMate.Models;

namespace GrowMate.Repositories.Interfaces;

public interface ICartRepository : IGenericRepository<Cart>
{
    Task<Cart> GetByCustomerIdAsync(int customerId);
}
