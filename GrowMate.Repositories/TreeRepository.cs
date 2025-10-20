using GrowMate.Models;
using GrowMate.Repositories.Data;
using GrowMate.Repositories.Extensions;
using GrowMate.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GrowMate.Repositories
{
    public class TreeRepository : ITreeRepository
    {
        private readonly EXE201_GrowMateContext _db;

        public TreeRepository(EXE201_GrowMateContext db)
        {
            _db = db;
        }

        public async Task<PageResult<Tree>> GetByListingIdAsync(int listingId, int page, int pageSize, CancellationToken ct = default)
        {
            var query = _db.Trees
                .AsNoTracking()
                .Include(t => t.Listing)
                .ThenInclude(l => l.Post)
                .Include(t => t.Listing)
                .ThenInclude(l => l.Farmer)
                .Where(t => t.ListingId == listingId)
                .OrderByDescending(t => t.CreatedAt);

            return await query.ToPagedResultAsync(page, pageSize, ct);
        }

        public async Task<PageResult<Tree>> GetByFarmerIdAsync(int farmerId, int page, int pageSize, CancellationToken ct = default)
        {
            var query = _db.Trees
                .AsNoTracking()
                .Include(t => t.Listing)
                .ThenInclude(l => l.Post)
                .Include(t => t.Listing)
                .ThenInclude(l => l.Farmer)
                .Where(t => t.Listing.FarmerId == farmerId)
                .OrderByDescending(t => t.CreatedAt);

            return await query.ToPagedResultAsync(page, pageSize, ct);
        }

        public async Task<PageResult<Tree>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
        {
            var query = _db.Trees
                .AsNoTracking()
                .Include(t => t.Listing)
                .ThenInclude(l => l.Post)
                .Include(t => t.Listing)
                .ThenInclude(l => l.Farmer)
                .OrderByDescending(t => t.CreatedAt);

            return await query.ToPagedResultAsync(page, pageSize, ct);
        }

        public async Task<Tree?> GetByIdAsync(int treeId, CancellationToken ct = default)
        {
            return await _db.Trees
                .Include(t => t.Listing)
                .ThenInclude(l => l.Post)
                .Include(t => t.Listing)
                .ThenInclude(l => l.Farmer)
                .FirstOrDefaultAsync(t => t.TreeId == treeId, ct);
        }

        public async Task<Tree?> GetByIdWithDetailsAsync(int treeId, CancellationToken ct = default)
        {
            return await _db.Trees
                .Include(t => t.Listing)
                .ThenInclude(l => l.Post)
                .Include(t => t.Listing)
                .ThenInclude(l => l.Farmer)
                .Include(t => t.Adoptions)
                .ThenInclude(a => a.Customer)
                .ThenInclude(c => c.User)
                .Include(t => t.Camera)
                .FirstOrDefaultAsync(t => t.TreeId == treeId, ct);
        }

        public async Task AddAsync(Tree tree, CancellationToken ct = default)
        {
            await _db.Trees.AddAsync(tree, ct);
        }

        public void Update(Tree tree)
        {
            _db.Trees.Update(tree);
        }

        public void Remove(Tree tree)
        {
            _db.Trees.Remove(tree);
        }

        public async Task<bool> ExistsAsync(int treeId, CancellationToken ct = default)
        {
            return await _db.Trees.AnyAsync(t => t.TreeId == treeId, ct);
        }

        public async Task<bool> ExistsByUniqueCodeAsync(string uniqueCode, CancellationToken ct = default)
        {
            return await _db.Trees.AnyAsync(t => t.UniqueCode == uniqueCode, ct);
        }

        public async Task<int> GetCountByListingIdAsync(int listingId, CancellationToken ct = default)
        {
            return await _db.Trees.CountAsync(t => t.ListingId == listingId, ct);
        }
    }
}
