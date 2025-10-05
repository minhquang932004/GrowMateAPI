using GrowMate.Contracts.Responses;
using GrowMate.Models;
using GrowMate.Repositories.Extensions;

namespace GrowMate.Services.Products
{
    public interface IProductService
    {
        Task<int> CreateProductAsync(Product product, CancellationToken ct = default);

        // Existing entity-based methods (kept if used elsewhere)
        Task<PageResult<Product>> GetPendingAsync(int page, int pageSize, CancellationToken ct = default);
        Task<PageResult<Product>> GetApprovedAsync(int page, int pageSize, CancellationToken ct = default);
        Task<Product?> GetByIdAsync(int id, bool includeCollections = false, CancellationToken ct = default);
        Task<bool> UpdateProductAsync(int id, Action<Product> applyChanges, CancellationToken ct = default);
        Task<bool> UpdateProductStatusAsync(int id, string status, CancellationToken ct = default);

        // New DTO-focused endpoints
        Task<ProductDetailResponse?> GetDetailAsync(int id, CancellationToken ct = default);
        Task<PageResult<ProductListItemResponse>> GetApprovedListAsync(int page, int pageSize, CancellationToken ct = default);
    }
}
