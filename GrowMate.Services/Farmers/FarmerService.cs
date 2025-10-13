using GrowMate.Contracts.Requests;
using GrowMate.Contracts.Requests.Farmer; // Add domain-specific namespace
using GrowMate.Models;
using GrowMate.Repositories.Interfaces;
using GrowMate.Repositories.Models;

namespace GrowMate.Services.Farmers
{
    public class FarmerService : IFarmerService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FarmerService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task CreateByUserId(int userId, FarmerRequest? request, CancellationToken ct)
        {
            var newFarmer = new Farmer
            {
                UserId = userId,
                FarmAddress = request?.FarmAddress,
                FarmName = request?.FarmName,
                ContactPhone = request?.ContactPhone,
                VerificationStatus = request?.VerificationStatus,
                CreatedAt = DateTime.Now,
            };
            await _unitOfWork.Farmers.CreateAsync(newFarmer, ct);
        }

        public Task<bool> GetFarmerByIdAsync(int id)
            => _unitOfWork.Farmers.ExistsAsync(id);

        public async Task RemoveByUserIdAsync(int userId, CancellationToken ct)
        {
            var farmer = await _unitOfWork.Farmers.GetByUserIdAsync(userId, ct);
            if (farmer != null)
            {
                _unitOfWork.Farmers.Remove(farmer);
            }
        }

        public async Task UpdateFarmerAsync(int id, FarmerRequest request, CancellationToken ct)
        {
            var farmerExist = await _unitOfWork.Farmers.GetByIdAsync(id, ct);
            if (farmerExist != null)
            {
                farmerExist.UserId = id;
                farmerExist.FarmName = request.FarmName;
                farmerExist.FarmAddress = request.FarmAddress;
                farmerExist.ContactPhone = request.ContactPhone;
                farmerExist.VerificationStatus = request.VerificationStatus;
                _unitOfWork.Farmers.UpdateAsync(farmerExist);
            }
        }
    }
}
