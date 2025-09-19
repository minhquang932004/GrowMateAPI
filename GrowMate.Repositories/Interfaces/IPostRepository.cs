using GrowMate.Repositories.Extensions;
using GrowMate.Repositories.Models;

namespace GrowMate.Repositories.Interfaces
{
    public interface IPostRepository
    {
        Task AddAsync(Post post, CancellationToken ct = default);
        void Update(Post post);

        Task<Post?> GetByIdAsync(int id, bool includeCollections = false, CancellationToken ct = default);

        Task<PageResult<Post>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
        Task<PageResult<Post>> GetByFarmerIdAsync(int farmerId, int page, int pageSize, CancellationToken ct = default);
    }
}