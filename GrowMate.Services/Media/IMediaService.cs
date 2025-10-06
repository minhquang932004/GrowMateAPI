using GrowMate.Contracts.Requests;
using GrowMate.Contracts.Responses;
using GrowMate.Models;
using GrowMate.Repositories.Extensions;

public interface IMediaService
{
    Task<PageResult<Medium>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
    Task<MediaResponse> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<Medium>> GetByPostIdAsync(int postId, CancellationToken ct = default);
    Task<List<Medium>> GetByReportIdAsync(int reportId, CancellationToken ct = default);
    Task<List<Medium>> GetByProductIdAsync(int productId, CancellationToken ct = default);
    void Update(Medium media);
    Task RemoveAsync(Medium media, CancellationToken ct = default);
    Task CreateMediaAsync(List<MediaItemDto> mediaItems, int? postId = null, int? productId = null, int? reportId = null, CancellationToken ct = default);
    Task ReplacePostMediaAsync(int postId, List<MediaItemDto> newMedia, CancellationToken ct = default);
    Task ReplaceReportMediaAsync(int reportId, List<MediaItemDto> newMedia, CancellationToken ct = default);
    Task ReplaceProductMediaAsync(int productId, List<MediaItemDto> newMedia, CancellationToken ct = default);
}
