using GrowMate.Models;
using GrowMate.Repositories.Data;
using GrowMate.Repositories.Extensions;
using GrowMate.Repositories.Interfaces;
using GrowMate.Repositories.Models;
using GrowMate.Repositories.Models.Statuses;
using Microsoft.EntityFrameworkCore;

namespace GrowMate.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly EXE201_GrowMateContext _dbContext;

        public PostRepository(EXE201_GrowMateContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(Post post, CancellationToken ct = default)
        {
            await _dbContext.Posts.AddAsync(post, ct);
        }

        public void Update(Post post)
        {
            _dbContext.Posts.Update(post);
        }

        public async Task<Post?> GetByIdAsync(int id, bool includeCollections = false, CancellationToken ct = default)
        {
            IQueryable<Post> q = _dbContext.Posts;

            if (includeCollections)
            {
                q = q.Include(p => p.Media)
                     .Include(p => p.PostComments);
            }

            return await q.AsNoTracking().FirstOrDefaultAsync(p => p.PostId == id, ct);
        }

        public Task<PageResult<Post>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
        {
            var q = _dbContext.Posts.AsNoTracking().OrderByDescending(p => p.CreatedAt);
            return q.ToPagedResultAsync(page, pageSize, ct);
        }

        public Task<PageResult<Post>> GetByFarmerIdAsync(int farmerId, int page, int pageSize, CancellationToken ct = default)
        {
            var q = _dbContext.Posts.AsNoTracking()
                .Where(p => p.FarmerId == farmerId)
                .OrderByDescending(p => p.CreatedAt);

            return q.ToPagedResultAsync(page, pageSize, ct);
        }

        public Task<PageResult<Post>> GetByStatusAsync(string status, int page, int pageSize, CancellationToken ct = default)
        {
            var q = _dbContext.Posts.AsNoTracking()
                .Where(p => p.Status == status)
                .OrderByDescending(p => p.CreatedAt);

            return q.ToPagedResultAsync(page, pageSize, ct);
        }
    }
}