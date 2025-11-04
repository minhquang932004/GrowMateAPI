using GrowMate.Models;
using GrowMate.Repositories.Data;
using GrowMate.Repositories.Extensions;
using GrowMate.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GrowMate.Repositories
{
    public class AdoptionRepository : IAdoptionRepository
    {
        private readonly EXE201_GrowMateContext _db;

        public AdoptionRepository(EXE201_GrowMateContext db)
        {
            _db = db;
        }

        public async Task<PageResult<Adoption>> GetByCustomerIdAsync(int customerId, int page, int pageSize, CancellationToken ct = default)
        {
            var query = _db.Adoptions
                .AsNoTracking()
                .Include(a => a.Tree)
                .ThenInclude(t => t.Listing)
                .ThenInclude(l => l.Post)
                .Include(a => a.Tree)
                .ThenInclude(t => t.Listing)
                .ThenInclude(l => l.Farmer)
                .Where(a => a.CustomerId == customerId)
                .OrderByDescending(a => a.CreatedAt);

            return await query.ToPagedResultAsync(page, pageSize, ct);
        }

        public async Task<PageResult<Adoption>> GetByFarmerIdAsync(int farmerId, int page, int pageSize, CancellationToken ct = default)
        {
            var query = _db.Adoptions
                .AsNoTracking()
                .Include(a => a.Tree)
                .ThenInclude(t => t.Listing)
                .ThenInclude(l => l.Post)
                .Include(a => a.Tree)
                .ThenInclude(t => t.Listing)
                .ThenInclude(l => l.Farmer)
                .Where(a => a.Tree.Listing.FarmerId == farmerId)
                .OrderByDescending(a => a.CreatedAt);

            return await query.ToPagedResultAsync(page, pageSize, ct);
        }

        public async Task<PageResult<Adoption>> GetByOrderIdAsync(int orderId, int page, int pageSize, CancellationToken ct = default)
        {
            var query = _db.Adoptions
                .AsNoTracking()
                .Include(a => a.Tree)
                .ThenInclude(t => t.Listing)
                .ThenInclude(l => l.Post)
                .Include(a => a.Customer)
                .ThenInclude(c => c.User)
                .Where(a => a.OrderId == orderId)
                .OrderByDescending(a => a.CreatedAt);

            return await query.ToPagedResultAsync(page, pageSize, ct);
        }

        public async Task<PageResult<Adoption>> GetByPostIdAsync(int postId, int page, int pageSize, CancellationToken ct = default)
        {
            var query = _db.Adoptions
                .AsNoTracking()
                .Include(a => a.Tree)
                .ThenInclude(t => t.Listing)
                .ThenInclude(l => l.Post)
                .Include(a => a.Tree)
                .ThenInclude(t => t.Listing)
                .ThenInclude(l => l.Farmer)
                .Include(a => a.Customer)
                .ThenInclude(c => c.User)
                .Where(a => a.Tree.Listing.PostId == postId)
                .OrderByDescending(a => a.CreatedAt);

            return await query.ToPagedResultAsync(page, pageSize, ct);
        }

        public async Task<PageResult<Adoption>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
        {
            var query = _db.Adoptions
                .AsNoTracking()
                .Include(a => a.Tree)
                .ThenInclude(t => t.Listing)
                .ThenInclude(l => l.Post)
                .Include(a => a.Tree)
                .ThenInclude(t => t.Listing)
                .ThenInclude(l => l.Farmer)
                .Include(a => a.Customer)
                .ThenInclude(c => c.User)
                .OrderByDescending(a => a.CreatedAt);

            return await query.ToPagedResultAsync(page, pageSize, ct);
        }

        public async Task<Adoption?> GetByIdAsync(int adoptionId, CancellationToken ct = default)
        {
            return await _db.Adoptions
                .Include(a => a.Tree)
                .ThenInclude(t => t.Listing)
                .ThenInclude(l => l.Post)
                .Include(a => a.Tree)
                .ThenInclude(t => t.Listing)
                .ThenInclude(l => l.Farmer)
                .FirstOrDefaultAsync(a => a.AdoptionId == adoptionId, ct);
        }

        public async Task<Adoption?> GetByIdWithDetailsAsync(int adoptionId, CancellationToken ct = default)
        {
            return await _db.Adoptions
                .Include(a => a.Tree)
                .ThenInclude(t => t.Listing)
                .ThenInclude(l => l.Post)
                .Include(a => a.Tree)
                .ThenInclude(t => t.Listing)
                .ThenInclude(l => l.Farmer)
                .Include(a => a.Customer)
                .ThenInclude(c => c.User)
                .Include(a => a.MonthlyReports)
                .Include(a => a.Payments)
                .FirstOrDefaultAsync(a => a.AdoptionId == adoptionId, ct);
        }

        public async Task AddAsync(Adoption adoption, CancellationToken ct = default)
        {
            await _db.Adoptions.AddAsync(adoption, ct);
        }

        public void Update(Adoption adoption)
        {
            _db.Adoptions.Update(adoption);
        }

        public void Remove(Adoption adoption)
        {
            _db.Adoptions.Remove(adoption);
        }

        public async Task<bool> ExistsAsync(int adoptionId, CancellationToken ct = default)
        {
            return await _db.Adoptions.AnyAsync(a => a.AdoptionId == adoptionId, ct);
        }
    }
}
