using GrowMate.Models;
using GrowMate.Repositories.Models;

namespace GrowMate.Repositories.Interfaces
{
    public interface IFarmerRepository
    {
        Task<bool> ExistsAsync(int id, CancellationToken ct = default);
        Task<Farmer?> GetByIdAsync(int id, CancellationToken ct = default);

        // New helpers for User-bound flows
        Task<Farmer?> GetByUserIdAsync(int userId, CancellationToken ct = default);
        Task<bool> ExistsByUserIdAsync(int userId, CancellationToken ct = default);

        Task CreateAsync(Farmer farmer, CancellationToken ct = default);

        Task Remove(Farmer farmer);

        void UpdateAsync(Farmer farmer);
    }
}