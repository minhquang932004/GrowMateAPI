using Azure;
using GrowMate.Models;
using GrowMate.Repositories.Data;
using GrowMate.Repositories.Extensions;
using GrowMate.Repositories.Interfaces;
using GrowMate.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace GrowMate.Repositories
{
    public class MediaRepository : IMediaRepository
    {
        private readonly EXE201_GrowMateContext _dbContext;

        public MediaRepository(EXE201_GrowMateContext dbContext)
        {
            _dbContext = dbContext;
        }
        public Task<PageResult<Medium>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
        {
            var q = _dbContext.Media.AsNoTracking().OrderByDescending(p => p.CreatedAt);
            return q.ToPagedResultAsync(page, pageSize, ct);
        }
        public async Task<Medium?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _dbContext.Media.AsNoTracking().FirstOrDefaultAsync(p => p.MediaId == id, ct);
        }
        public Task<List<Medium>> GetByPostIdAsync(int postId, CancellationToken ct = default)
        {
            // Tracked list so RemoveRange works without extra attach
            return _dbContext.Media.Where(m => m.PostId == postId).ToListAsync(ct);
        }
        public Task<List<Medium>> GetByReportIdAsync(int reportId, CancellationToken ct = default)
        {
            // Tracked list so RemoveRange works without extra attach
            return _dbContext.Media.Where(m => m.ReportId == reportId).ToListAsync(ct);
        }
        public Task<List<Medium>> GetByProductIdAsync(int productId, CancellationToken ct = default)
        {
            // Tracked list so RemoveRange works without extra attach
            return _dbContext.Media.Where(m => m.ProductId == productId).ToListAsync(ct);
        }
        public async Task AddAsync(Medium media, CancellationToken ct = default)
        {
            await _dbContext.Media.AddAsync(media, ct);
        }
        public void Update(Medium media)
        {
            _dbContext.Media.Update(media);
        }
        public async Task RemoveAsync(Medium media, CancellationToken ct = default)
        {
            _dbContext.Media.Remove(media);
        }
        public void AddRange(IEnumerable<Medium> media)
        {
            if (media == null) return;
            _dbContext.Media.AddRange(media);
        }

        public void RemoveRange(IEnumerable<Medium> media)
        {
            if (media == null) return;
            _dbContext.Media.RemoveRange(media);
        }

        public async Task<Medium?> GetPrimaryImageByPostIdAsync(int postId, CancellationToken ct = default)
        {
            return await _dbContext.Media
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.PostId == postId && m.IsPrimary, ct);
        }

        public async Task<Medium?> GetPrimaryImageByProductIdAsync(int productId, CancellationToken ct = default)
        {
            return await _dbContext.Media
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ProductId == productId && m.IsPrimary, ct);
        }
    }
}