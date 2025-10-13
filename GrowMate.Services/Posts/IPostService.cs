using GrowMate.Contracts.Requests;
using GrowMate.Contracts.Requests.Post; // Add domain-specific namespace for Post requests
using GrowMate.Contracts.Responses;
using GrowMate.Contracts.Responses.Auth; // Add domain-specific namespace for Auth responses
using GrowMate.Contracts.Responses.Post; // Add domain-specific namespace for Post responses
using GrowMate.Models;
using GrowMate.Repositories.Extensions;
using GrowMate.Repositories.Models;

namespace GrowMate.Services.Posts
{
    public interface IPostService
    {
        Task<PageResult<Post>> GetAllPostsAsync(int page, int pageSize, CancellationToken ct = default);
        Task<PostResponse?> GetPostByIdAsync(int id, CancellationToken ct = default);
        Task<PageResult<Post>> GetAllPostsByFarmerIdAsync(int id, int page, int pageSize, CancellationToken ct = default);
        Task<AuthResponse> CreatePostAsync(CreatePostRequest request, CancellationToken ct = default);
        Task<AuthResponse> DeletePostAsync(int id, CancellationToken ct = default);
        Task<AuthResponse> UpdatePostStatusAsync(int id, string status, CancellationToken ct = default);
        Task<AuthResponse> UpdatePostAsync(int id, CreatePostRequest request, CancellationToken ct = default);
    }
}
