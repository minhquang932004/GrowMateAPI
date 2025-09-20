using GrowMate.Repositories.Models;

namespace GrowMate.Repositories.Interfaces
{
    public interface IFarmerRepository
    {
        Task<bool> ExistsAsync(int id, CancellationToken ct = default);
        Task<Farmer?> GetByIdAsync(int id, CancellationToken ct = default);

        Task CreateAsync(Farmer farmer, CancellationToken ct = default);
        void UpdateAsync(Farmer farmer);
        Task Remove(Farmer farmer);
    }
}