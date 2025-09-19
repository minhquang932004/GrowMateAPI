using GrowMate.Repositories.Interfaces;

namespace GrowMate.Services.Farmers
{
    public class FarmerService : IFarmerService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FarmerService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Task<bool> GetFarmerByIdAsync(int id)
            => _unitOfWork.Farmers.ExistsAsync(id);
    }
}
