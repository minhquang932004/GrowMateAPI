using GrowMate.Models;
using GrowMate.Repositories.Extensions;

namespace GrowMate.Repositories.Interfaces
{
    public interface ITreeRepository
    {
        Task<PageResult<Tree>> GetByListingIdAsync(int listingId, int page, int pageSize, CancellationToken ct = default);
        Task<PageResult<Tree>> GetByFarmerIdAsync(int farmerId, int page, int pageSize, CancellationToken ct = default);
        Task<PageResult<Tree>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
        Task<Tree?> GetByIdAsync(int treeId, CancellationToken ct = default);
        Task<Tree?> GetByIdWithDetailsAsync(int treeId, CancellationToken ct = default);
        Task AddAsync(Tree tree, CancellationToken ct = default);
        void Update(Tree tree);
        void Remove(Tree tree);
        Task<bool> ExistsAsync(int treeId, CancellationToken ct = default);
        Task<bool> ExistsByUniqueCodeAsync(string uniqueCode, CancellationToken ct = default);
        Task<int> GetCountByListingIdAsync(int listingId, CancellationToken ct = default);
    }
}
