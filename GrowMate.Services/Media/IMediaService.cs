using GrowMate.Contracts.Requests;
using GrowMate.Contracts.Responses;
using GrowMate.Models;
using GrowMate.Repositories.Extensions;

namespace GrowMate.Services.Media
{
    public interface IMediaService
    {
        Task<PageResult<Medium>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
        Task<MediaResponse> GetByIdAsync(int id, CancellationToken ct = default);
        Task<List<Medium>> GetByPostIdAsync(int postId, CancellationToken ct = default);
        Task<List<Medium>> GetByReportIdAsync(int reportId, CancellationToken ct = default);
        Task<List<Medium>> GetByProductIdAsync(int productId, CancellationToken ct = default);
        void Update(Medium media);
        Task RemoveAsync(Medium media, CancellationToken ct = default);
        Task CreateMediaAsync(int? postId, int? reportId, int? productId, IEnumerable<CreateMediaRequest>? request, CancellationToken ct = default);
        Task ReplacePostMediaAsync(int postId, IEnumerable<CreateMediaRequest>? request, CancellationToken ct = default);
    }

}
