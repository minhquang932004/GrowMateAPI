using GrowMate.Repositories.Data;
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

        public Task<List<Medium>> GetByPostIdAsync(int postId, CancellationToken ct = default)
        {
            // Tracked list so RemoveRange works without extra attach
            return _dbContext.Media.Where(m => m.PostId == postId).ToListAsync(ct);
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
    }
}