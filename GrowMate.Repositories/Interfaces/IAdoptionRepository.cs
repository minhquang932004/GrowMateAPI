using GrowMate.Models;
using GrowMate.Repositories.Extensions;

namespace GrowMate.Repositories.Interfaces
{
    public interface IAdoptionRepository
    {
        Task<PageResult<Adoption>> GetByCustomerIdAsync(int customerId, int page, int pageSize, CancellationToken ct = default);
        Task<PageResult<Adoption>> GetByFarmerIdAsync(int farmerId, int page, int pageSize, CancellationToken ct = default);
        Task<PageResult<Adoption>> GetByOrderIdAsync(int orderId, int page, int pageSize, CancellationToken ct = default);
        Task<PageResult<Adoption>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
        Task<Adoption?> GetByIdAsync(int adoptionId, CancellationToken ct = default);
        Task<Adoption?> GetByIdWithDetailsAsync(int adoptionId, CancellationToken ct = default);
        Task AddAsync(Adoption adoption, CancellationToken ct = default);
        void Update(Adoption adoption);
        void Remove(Adoption adoption);
        Task<bool> ExistsAsync(int adoptionId, CancellationToken ct = default);
    }
}
