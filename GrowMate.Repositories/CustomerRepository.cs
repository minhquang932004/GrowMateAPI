using GrowMate.Models;
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
            => _dbContext.Customers.FirstOrDefaultAsync(c => c.UserId == userId, ct);

        public Task<Customer?> GetByCustomerIdAsync(int customerId, CancellationToken ct = default)
            => _dbContext.Customers.FirstOrDefaultAsync(c => c.CustomerId == customerId, ct);

        public Task<bool> AnyAsync(int userId, CancellationToken ct = default)
            => _dbContext.Customers.AnyAsync(c => c.UserId == userId, ct);

        public async Task CreateAsync(Customer customer, CancellationToken ct = default)
        {
            await _dbContext.Customers.AddAsync(customer, ct);
        }

        public async Task Remove(Customer customer)
        {
            var item = _dbContext.Customers.Remove(customer);
            await _dbContext.SaveChangesAsync();
        }

        public void UpdateAsync(Customer customer)
        {
            _dbContext.Customers.Update(customer);
        }
    }
}
