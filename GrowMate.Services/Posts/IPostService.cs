using GrowMate.Contracts.Requests;
using GrowMate.Contracts.Responses;
using GrowMate.Repositories.Extensions;
using GrowMate.Repositories.Models;

namespace GrowMate.Services.Posts
{
    public interface IPostService
    {
        Task<PageResult<Post>> GetAllPostsAsync(int page, int pageSize, CancellationToken ct = default);
        Task<PostResponse?> GetPostByIdAsync(int id, CancellationToken ct = default);
        Task<PageResult<Post>> GetAllPostsByFarmerIdAsync(int id, int page, int pageSize, CancellationToken ct = default);
        Task<AuthResponseDto> CreatePostAsync(CreatePostRequest request, CancellationToken ct = default);
        Task<AuthResponseDto> DeletePostAsync(int id, CancellationToken ct = default);
        Task<AuthResponseDto> UpdatePostStatusAsync(int id, string status, CancellationToken ct = default);
        Task<AuthResponseDto> UpdatePostAsync(int id, CreatePostRequest request, CancellationToken ct = default);
    }
}
