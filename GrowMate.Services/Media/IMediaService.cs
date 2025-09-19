using GrowMate.Contracts.Requests;

namespace GrowMate.Services.Media
{
    public interface IMediaService
    {
        Task CreatePostMediaAsync(int postId, IEnumerable<CreateMediaPostRequest>? request, CancellationToken ct = default);
        Task ReplacePostMediaAsync(int postId, IEnumerable<CreateMediaPostRequest>? request, CancellationToken ct = default);
    }

}
