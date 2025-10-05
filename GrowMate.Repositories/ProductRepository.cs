using GrowMate.Models;
using GrowMate.Repositories.Data;
using GrowMate.Repositories.Extensions;
using GrowMate.Repositories.Interfaces;
using GrowMate.Repositories.Models.Statuses;
using Microsoft.EntityFrameworkCore;

namespace GrowMate.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly EXE201_GrowMateContext _db;

        public ProductRepository(EXE201_GrowMateContext db) => _db = db;

        public async Task AddAsync(Product product, CancellationToken ct = default)
            => await _db.Products.AddAsync(product, ct);

        public void Update(Product product) => _db.Products.Update(product);

        public async Task<Product?> GetByIdAsync(int id, bool includeCollections = false, CancellationToken ct = default)
        {
            IQueryable<Product> q = _db.Products;
            if (includeCollections)
            {
                q = q
                    .Include(p => p.Media)
                    .Include(p => p.Category)
                    .Include(p => p.ProductType)
                    .Include(p => p.Unit)
                    .Include(p => p.Farmer);
            }
            return await q.FirstOrDefaultAsync(p => p.ProductId == id, ct);
        }

        public async Task<Product?> GetApprovedByIdAsync(int id, bool includeCollections = false, CancellationToken ct = default)
        {
            IQueryable<Product> q = _db.Products.Where(p => p.Status == ProductStatuses.Approved);
            if (includeCollections)
            {
                q = q
                    .Include(p => p.Media)
                    .Include(p => p.Category)
                    .Include(p => p.ProductType)
                    .Include(p => p.Unit)
                    .Include(p => p.Farmer);
            }
            return await q.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == id, ct);
        }

        public Task<PageResult<Product>> GetApprovedAsync(int page, int pageSize, bool includeCollections = false, CancellationToken ct = default)
        {
            IQueryable<Product> q = _db.Products.AsNoTracking()
                .Where(p => p.Status == ProductStatuses.Approved)
                .OrderByDescending(p => p.UpdatedAt);

            if (includeCollections)
            {
                q = q
                    .Include(p => p.Media)
                    .Include(p => p.Category)
                    .Include(p => p.ProductType)
                    .Include(p => p.Unit)
                    .Include(p => p.Farmer);
            }

            return q.ToPagedResultAsync(page, pageSize, ct);
        }

        public Task<PageResult<Product>> GetPendingAsync(int page, int pageSize, bool includeCollections = false, CancellationToken ct = default)
        {
            IQueryable<Product> q = _db.Products.AsNoTracking()
                .Where(p => p.Status == ProductStatuses.Pending)
                .OrderByDescending(p => p.CreatedAt);

            if (includeCollections)
            {
                q = q
                    .Include(p => p.Media)
                    .Include(p => p.Category)
                    .Include(p => p.ProductType)
                    .Include(p => p.Unit)
                    .Include(p => p.Farmer);
            }

            return q.ToPagedResultAsync(page, pageSize, ct);
        }
    }
}