using GrowMate.Repositories.Models;

namespace GrowMate.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string normalizedEmail, bool includeCustomer = false, CancellationToken ct = default);
        Task<User?> GetByIdAsync(int id, bool includeCustomer = false, CancellationToken ct = default);
        Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken ct = default);
        Task AddAsync(User user, CancellationToken ct = default);
        void Update(User user);
    }
}
