using GrowMate.Repositories.Extensions;
using GrowMate.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrowMate.Repositories.Interfaces
{
    public interface ITreeListingRepository
    {
        Task<TreeListing?> GetByIdAsync(int id, bool includeTrees, CancellationToken ct = default);
        Task<PageResult<TreeListing>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
        Task<PageResult<TreeListing>> GetByFarmerIdAsync(int farmerId, int page, int pageSize, CancellationToken ct = default);
        Task<TreeListing?> GetByPostIdAsync(int postId, bool includeTrees, CancellationToken ct = default);

        Task AddAsync(TreeListing treeListing, CancellationToken ct = default);
        void UpdateAsync(TreeListing treeListing);
    }
}
