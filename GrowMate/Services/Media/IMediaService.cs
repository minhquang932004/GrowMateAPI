using GrowMate.DTOs.Requests;

namespace GrowMate.Services.Media
{
    public interface IMediaService
    {
        Task CreatePostMediaAsync(int postId, IEnumerable<CreateMediaPostRequest> request);
    }
}
