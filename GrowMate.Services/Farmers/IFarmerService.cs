using GrowMate.Contracts.Requests;
using GrowMate.Contracts.Requests.Farmer; // Add the domain-specific namespace

namespace GrowMate.Services.Farmers
{
    public interface IFarmerService
    {
        Task<bool> GetFarmerByIdAsync(int id);

        Task UpdateFarmerAsync(int id, FarmerRequest request, CancellationToken ct);

        Task RemoveByUserIdAsync(int userId, CancellationToken ct);

        Task CreateByUserId(int userId, FarmerRequest? request, CancellationToken ct);
    }
}
