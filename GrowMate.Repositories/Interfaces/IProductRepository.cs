using GrowMate.Models;
using GrowMate.Repositories.Extensions;

namespace GrowMate.Repositories.Interfaces
{
    public interface IProductRepository
    {
        Task AddAsync(Product product, CancellationToken ct = default);
        void Update(Product product);
        Task<Product?> GetByIdAsync(int id, bool includeCollections = false, CancellationToken ct = default);
        Task<Product?> GetApprovedByIdAsync(int id, bool includeCollections = false, CancellationToken ct = default);
        Task<PageResult<Product>> GetApprovedAsync(int page, int pageSize, bool includeCollections = false, CancellationToken ct = default);
        Task<PageResult<Product>> GetPendingAsync(int page, int pageSize, bool includeCollections = false, CancellationToken ct = default);
    }
}