using GrowMate.Contracts.Responses.Tree;
using GrowMate.Models;
using GrowMate.Repositories.Extensions;


namespace GrowMate.Services.TreeListings
{
    public interface ITreeListingService
    {
        Task<TreeListingResponse?> GetByIdAsync(int id, bool includeTrees, CancellationToken ct = default);
        Task<PageResult<TreeListing>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
        Task<PageResult<TreeListing>> GetByFarmerIdAsync(int farmerId, int page, int pageSize, CancellationToken ct = default);
        Task<TreeListingResponse> GetByPostIdAsync(int postId, bool includeTrees, CancellationToken ct = default);

        Task AddAsync(TreeListing treeListing, CancellationToken ct = default);
        void UpdateAsync(TreeListing treeListing);
    }
}
