using GrowMate.Models;
using GrowMate.Repositories.Extensions;
using GrowMate.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace GrowMate.Repositories.Interfaces
{
    public interface IMediaRepository
    {
        Task<PageResult<Medium>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
        Task<Medium?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<List<Medium>> GetByPostIdAsync(int postId, CancellationToken ct = default);
        Task<List<Medium>> GetByReportIdAsync(int reportId, CancellationToken ct = default);
        Task<List<Medium>> GetByProductIdAsync(int productId, CancellationToken ct = default);
        Task AddAsync(Medium media, CancellationToken ct = default);
        void Update(Medium media);
        Task RemoveAsync(Medium media, CancellationToken ct = default);
        void AddRange(IEnumerable<Medium> media);
        void RemoveRange(IEnumerable<Medium> media);
        Task<Medium?> GetPrimaryImageByPostIdAsync(int postId, CancellationToken ct = default);
        Task<Medium?> GetPrimaryImageByProductIdAsync(int productId, CancellationToken ct = default);
    }
}