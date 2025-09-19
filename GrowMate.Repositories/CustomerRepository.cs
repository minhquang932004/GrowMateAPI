using GrowMate.Repositories.Data;
using GrowMate.Repositories.Interfaces;
using GrowMate.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace GrowMate.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly EXE201_GrowMateContext _dbContext;

        public CustomerRepository(EXE201_GrowMateContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<Customer?> GetByUserIdAsync(int userId, CancellationToken ct = default)
        {
            return _dbContext.Customers.FirstOrDefaultAsync(c => c.CustomerId == userId, ct);
        }

        public Task<bool> AnyAsync(int userId, CancellationToken ct = default)
        {
            return _dbContext.Customers.AnyAsync(c => c.CustomerId == userId, ct);
        }

        public async Task CreateAsync(Customer customer, CancellationToken ct = default)
        {
            await _dbContext.Customers.AddAsync(customer, ct);
        }
    }
}
