using GrowMate.Contracts.Requests.Farmer;
using GrowMate.Models; // Add the domain-specific namespace

namespace GrowMate.Services.Farmers
{
    public interface IFarmerService
    {
        Task<bool> GetFarmerByIdAsync(int id);

        Task<Farmer> GetFarmerDetailByIdAsync(int farmerId, CancellationToken ct);

        Task UpdateFarmerAsync(int id, FarmerRequest request, CancellationToken ct);

        Task RemoveByUserIdAsync(int userId, CancellationToken ct);

        Task CreateByUserId(int userId, FarmerRequest? request, CancellationToken ct);
    }
}
