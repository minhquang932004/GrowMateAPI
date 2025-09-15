using GrowMate.Data;
using GrowMate.DTOs.Extensions;
using GrowMate.DTOs.Requests;
using GrowMate.DTOs.Responses;
using GrowMate.Models;
using GrowMate.Services.Farmers;
using GrowMate.Services.Media;
using Microsoft.EntityFrameworkCore;

namespace GrowMate.Services.Posts
{
    public class PostService : IPostService
    {
        private readonly EXE201_GrowMateContext _context;
        private readonly IFarmerService _farmerService;
        private readonly ILogger<PostService> _logger;
        private readonly IMediaService _mediaService;

        public PostService(EXE201_GrowMateContext context, IFarmerService farmerService, ILogger<PostService> logger, IMediaService mediaService)
        {
            _context = context;
            _farmerService = farmerService;
            _logger = logger;
            _mediaService = mediaService;
        }

        public async Task<AuthResponseDto> CreatePostAsync(CreatePostRequest request)
        {
            var checkFarmer = await _farmerService.GetFarmerByIdAsync(request.FarmerId);
            if (!checkFarmer)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Không tìm thấy farmerId: " + request.FarmerId
                };
            }
            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {

                var newPost = new Post
                {
                    FarmerId = request.FarmerId,
                    ProductName = request.ProductName,
                    ProductType = request.ProductType,
                    ProductVariety = request.ProductVariety,
                    FarmName = request.FarmName,
                    Origin = request.Origin,
                    PricePerYear = request.PricePerYear,
                    HarvestWeight = request.HarvestWeight,
                    Unit = request.Unit,
                    HarvestFrequency = request.HarvestFrequency,
                    TreeQuantity = request.TreeQuantity,
                    Description = request.Description,
                    Status = "PENDING",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                };
                _context.Posts.Add(newPost);
                await _context.SaveChangesAsync();
                if (request.CreateMediaPostRequests != null && request.CreateMediaPostRequests.Any())
                {
                    await _mediaService.CreatePostMediaAsync(newPost.PostId, request.CreateMediaPostRequests);
                }
                await _context.SaveChangesAsync();
                await tx.CommitAsync();
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex, "Tạo post thất bại");
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Tạo mới post thất bại"
                };
            }
            return new AuthResponseDto
            {
                Success = true,
                Message = "Tạo mới post thành công!"
            };
        }

        public async Task<AuthResponseDto> DeletePostAsync(int id)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(a => a.PostId == id);
            if (post == null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Không tìm thấy postId: " + id
                };
            }
            post.Status = "CANCELED";
            post.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return new AuthResponseDto
            {
                Success = true,
                Message = "Đã CANCELED postId: " + id + " thành công"
            };
        }

        public async Task<PageResult<Post>> GetAllPostsAsync(int page, int pageSize)
        {
            return await _context.Posts.AsNoTracking().OrderByDescending(a => a.CreatedAt).ToPagedResultAsync(page, pageSize);
        }

        public async Task<PageResult<Post>> GetAllPostsByFarmerIdAsync(int id, int page, int pageSize)
        {
            return await _context.Posts.AsNoTracking().Where(a => a.FarmerId == id).OrderByDescending(a => a.CreatedAt).ToPagedResultAsync(page, pageSize);
        }

        public async Task<PostResponse> GetPostByIdAsync(int id)
        {

            var post = await _context.Posts.AsNoTracking().Include(a => a.Media).Include(a => a.PostComments).FirstOrDefaultAsync(a => a.PostId == id);
            if (post == null)
            {
                return null;
            }
            var postResponse = new PostResponse
            {
                PostId = post.PostId,
                FarmerId = post.FarmerId,
                ProductName = post.ProductName,
                ProductType = post.ProductType,
                ProductVariety = post.ProductVariety,
                FarmName = post.FarmName,
                Origin = post.Origin,
                PricePerYear = post.PricePerYear,
                HarvestWeight = post.HarvestWeight,
                HarvestFrequency = post.HarvestFrequency,
                Unit = post.Unit,
                TreeQuantity = post.TreeQuantity,
                Description = post.Description,
                Status = post.Status,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt,
                MediaPostList = post.Media?
                            .Select(m => new MediaPostListResponse
                            {
                                MediaId = m.MediaId,
                                PostId = post.PostId,
                                MediaUrl = m.MediaUrl,
                                MediaType = m.MediaType,
                                CreatedAt = m.CreatedAt,
                                UpdatedAt = m.UpdatedAt
                            }).ToList() ?? new List<MediaPostListResponse>(),
                PostCommentList = post.PostComments?.
                            Select(p => new PostCommentListResponse
                            {
                                CommentId = p.CommentId,
                                PostId = p.PostId,
                                CommentContent = p.CommentContent,
                                UserId = p.UserId,
                                CreatedAt = p.CreatedAt,
                                UpdatedAt = p.UpdatedAt,
                            }).ToList() ?? new List<PostCommentListResponse>()
            };
            return postResponse;
        }
    }
}
