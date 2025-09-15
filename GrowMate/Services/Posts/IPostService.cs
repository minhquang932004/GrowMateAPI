using GrowMate.DTOs.Extensions;
using GrowMate.DTOs.Requests;
using GrowMate.DTOs.Responses;
using GrowMate.Models;

namespace GrowMate.Services.Posts
{
    public interface IPostService
    {
        Task<PageResult<Post>> GetAllPostsAsync(int page, int pageSize);
        Task<PostResponse> GetPostByIdAsync(int id);
        Task<PageResult<Post>> GetAllPostsByFarmerIdAsync(int id, int page, int pageSize);

        Task<AuthResponseDto> CreatePostAsync(CreatePostRequest request);
        Task<AuthResponseDto> DeletePostAsync(int id);
    }
}
