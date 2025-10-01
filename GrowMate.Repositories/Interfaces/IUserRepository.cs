using GrowMate.Models;
using GrowMate.Repositories.Extensions;
using GrowMate.Repositories.Models;

namespace GrowMate.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<PageResult<User>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
        Task<User?> GetByEmailAsync(string normalizedEmail, bool includeCustomer = false, CancellationToken ct = default);
        Task<User?> GetByIdAsync(int id, bool includeCustomer = false, CancellationToken ct = default);
        Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken ct = default);
        Task<bool> PhoneExistsAsync(string phone, CancellationToken ct = default);
        Task AddAsync(User user, CancellationToken ct = default);
        void Update(User user);
    }
}
