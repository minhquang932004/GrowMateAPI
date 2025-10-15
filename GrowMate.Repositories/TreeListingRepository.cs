using GrowMate.Models;
using GrowMate.Repositories.Data;
using GrowMate.Repositories.Extensions;
using GrowMate.Repositories.Interfaces;
using GrowMate.Repositories.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrowMate.Repositories
{
    public class TreeListingRepository : ITreeListingRepository
    {
        private readonly EXE201_GrowMateContext _dbContext;
        public TreeListingRepository(EXE201_GrowMateContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(TreeListing treeListing, CancellationToken ct = default)
        {
            await _dbContext.AddAsync(treeListing, ct);
        }

        public Task<PageResult<TreeListing>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
        {
            var item = _dbContext.TreeListings.AsNoTracking().OrderByDescending(a => a.CreatedAt);
            return item.ToPagedResultAsync(page, pageSize, ct);
        }

        public Task<PageResult<TreeListing>> GetByFarmerIdAsync(int farmerId, int page, int pageSize, CancellationToken ct = default)
        {
            var item = _dbContext.TreeListings.AsNoTracking().Where(a => a.FarmerId == farmerId).OrderByDescending(a => a.CreatedAt);
            return item.ToPagedResultAsync(page, pageSize, ct);
        }

        public async Task<TreeListing?> GetByIdAsync(int id, bool includeTrees, CancellationToken ct = default)
        {
            IQueryable<TreeListing> item = _dbContext.TreeListings.Include(a => a.Post);
            if (includeTrees)
            {
                item = item.Include(a => a.Trees);
            }
            return await item.AsNoTracking().FirstOrDefaultAsync(a => a.ListingId == id, ct);
        }

        public async Task<TreeListing?> GetByPostIdAsync(int postId, bool includeTrees, CancellationToken ct = default)
        {
            IQueryable<TreeListing> item = _dbContext.TreeListings.Include(a => a.Post);
            if (includeTrees)
            {
                item = item.Include(a => a.Trees);
            }
            return await item.AsNoTracking().FirstOrDefaultAsync(a => a.PostId  == postId, ct);
        }

        public void UpdateAsync(TreeListing treeListing)
        {
            _dbContext.TreeListings.Update(treeListing);
        }
    }
}
