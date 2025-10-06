using GrowMate.Contracts.Requests;

namespace GrowMate.Services.Media
{
    public interface IMediaService
    {
        Task CreateMediaAsync(int? postId, int? reportId, int? productId, IEnumerable<CreateMediaRequest>? request, CancellationToken ct = default);
        Task ReplacePostMediaAsync(int postId, IEnumerable<CreateMediaRequest>? request, CancellationToken ct = default);
    }

}
