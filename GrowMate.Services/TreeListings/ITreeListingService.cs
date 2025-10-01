using GrowMate.Contracts.Responses;
using GrowMate.Models;
using GrowMate.Repositories.Extensions;
using GrowMate.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
