using GrowMate.Models;
using GrowMate.Repositories.Data;
using GrowMate.Repositories.Extensions;
using GrowMate.Repositories.Interfaces;
using GrowMate.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace GrowMate.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly EXE201_GrowMateContext _dbContext;

        public UserRepository(EXE201_GrowMateContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<User?> GetByEmailAsync(string normalizedEmail, bool includeCustomer = false, CancellationToken ct = default)
        {
            var query = _dbContext.Users.AsQueryable();
            if (includeCustomer)
            {
                query = query.Include(u => u.Customer);
            }

            // Case-insensitive lookup
            return await query.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail, ct);
        }

        public async Task<User?> GetByIdAsync(int id, bool includeCustomer = false, CancellationToken ct = default)
        {
            var query = _dbContext.Users.AsQueryable();
            if (includeCustomer)
            {
                query = query.Include(u => u.Customer);
            }

            return await query.FirstOrDefaultAsync(u => u.UserId == id, ct);
        }

        public Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken ct = default)
        {
            return _dbContext.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail, ct);
        }

        public async Task AddAsync(User user, CancellationToken ct = default)
        {
            await _dbContext.Users.AddAsync(user, ct);
        }

        public void Update(User user)
        {
            _dbContext.Users.Update(user);
        }

        public Task<PageResult<User>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
        {
            var item = _dbContext.Users.AsNoTracking().OrderByDescending(a => a.CreatedAt);
            return item.ToPagedResultAsync(page,pageSize, ct);
        }

        public Task<bool> PhoneExistsAsync(string phone, CancellationToken ct = default)
        {
            return _dbContext.Users.AsNoTracking().AnyAsync(p => p.Phone == phone, ct);
        }
    }
}
