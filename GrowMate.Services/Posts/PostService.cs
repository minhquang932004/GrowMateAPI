using GrowMate.Contracts.Requests;
using GrowMate.Contracts.Responses;
using GrowMate.Repositories.Extensions;
using GrowMate.Repositories.Interfaces;
using GrowMate.Repositories.Models;
using GrowMate.Repositories.Models.Statuses;
using GrowMate.Services.Farmers;
using GrowMate.Services.Media;
using Microsoft.Extensions.Logging;

namespace GrowMate.Services.Posts
{
    public class PostService : IPostService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFarmerService _farmerService;
        private readonly ILogger<PostService> _logger;
        private readonly IMediaService _mediaService;

        public PostService(IUnitOfWork unitOfWork, IFarmerService farmerService, ILogger<PostService> logger, IMediaService mediaService)
        {
            _unitOfWork = unitOfWork;
            _farmerService = farmerService;
            _logger = logger;
            _mediaService = mediaService;
        }

        public Task<PageResult<Post>> GetAllPostsAsync(int page, int pageSize, CancellationToken ct = default)
            => _unitOfWork.Posts.GetAllAsync(page, pageSize, ct);

        public Task<PageResult<Post>> GetAllPostsByFarmerIdAsync(int id, int page, int pageSize, CancellationToken ct = default)
            => _unitOfWork.Posts.GetByFarmerIdAsync(id, page, pageSize, ct);

        public async Task<PostResponse?> GetPostByIdAsync(int id, CancellationToken ct = default)
        {
            var post = await _unitOfWork.Posts.GetByIdAsync(id, includeCollections: true, ct);
            if (post == null) return null;

            return new PostResponse
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
                Unit = post.Unit,
                HarvestFrequency = post.HarvestFrequency,
                TreeQuantity = post.TreeQuantity,
                Description = post.Description,
                Status = post.Status,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt,
                MediaPostList = post.Media?.Select(m => new MediaPostListResponse
                {
                    MediaId = m.MediaId,
                    PostId = post.PostId,
                    MediaUrl = m.MediaUrl,
                    MediaType = m.MediaType,
                    CreatedAt = m.CreatedAt,
                    UpdatedAt = m.UpdatedAt
                }).ToList() ?? new List<MediaPostListResponse>(),
                PostCommentList = post.PostComments?.Select(c => new PostCommentListResponse
                {
                    CommentId = c.CommentId,
                    PostId = c.PostId,
                    UserId = c.UserId,
                    CommentContent = c.CommentContent,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                }).ToList() ?? new List<PostCommentListResponse>()
            };
        }

        public async Task<AuthResponseDto> CreatePostAsync(CreatePostRequest request, CancellationToken ct = default)
        {
            if (!await _farmerService.GetFarmerByIdAsync(request.FarmerId))
                return new AuthResponseDto { Success = false, Message = "Không tìm thấy farmerId: " + request.FarmerId };

            try
            {
                await _unitOfWork.ExecuteInTransactionAsync(async innerCt =>
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
                        Status = PostStatuses.Pending,             // <- was "PENDING"
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    await _unitOfWork.Posts.AddAsync(newPost, innerCt);
                    await _unitOfWork.SaveChangesAsync(innerCt);

                    await _mediaService.CreatePostMediaAsync(newPost.PostId, request.CreateMediaPostRequests, innerCt);
                    await _unitOfWork.SaveChangesAsync(innerCt);
                }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tạo post thất bại");
                return new AuthResponseDto { Success = false, Message = "Tạo mới post thất bại" };
            }

            return new AuthResponseDto { Success = true, Message = "Tạo mới post thành công!" };
        }

        public async Task<AuthResponseDto> DeletePostAsync(int id, CancellationToken ct = default)
        {
            var post = await _unitOfWork.Posts.GetByIdAsync(id, includeCollections: false, ct);
            if (post == null)
                return new AuthResponseDto { Success = false, Message = "Không tìm thấy postId: " + id };

            post.Status = PostStatuses.Canceled;               // <- was "CANCELED"
            post.UpdatedAt = DateTime.Now;
            _unitOfWork.Posts.Update(post);
            await _unitOfWork.SaveChangesAsync(ct);

            return new AuthResponseDto { Success = true, Message = "Đã CANCELED postId: " + id + " thành công" };
        }

        public async Task<AuthResponseDto> UpdatePostAsync(int id, CreatePostRequest request, CancellationToken ct = default)
        {
            var post = await _unitOfWork.Posts.GetByIdAsync(id, includeCollections: false, ct);
            if (post == null)
                return new AuthResponseDto { Success = false, Message = "Không tìm thấy postId: " + id };

            if (!await _farmerService.GetFarmerByIdAsync(request.FarmerId))
                return new AuthResponseDto { Success = false, Message = "Không tìm thấy farmerId: " + request.FarmerId };

            try
            {
                await _unitOfWork.ExecuteInTransactionAsync(async innerCt =>
                {
                    post.FarmerId = request.FarmerId;
                    post.ProductName = request.ProductName;
                    post.ProductType = request.ProductType;
                    post.ProductVariety = request.ProductVariety;
                    post.FarmName = request.FarmName;
                    post.Origin = request.Origin;
                    post.PricePerYear = request.PricePerYear;
                    post.HarvestWeight = request.HarvestWeight;
                    post.Unit = request.Unit;
                    post.HarvestFrequency = request.HarvestFrequency;
                    post.TreeQuantity = request.TreeQuantity;
                    post.Description = request.Description;
                    post.Status = PostStatuses.Pending;          // <- was "PENDING"
                    post.UpdatedAt = DateTime.Now;

                    _unitOfWork.Posts.Update(post);

                    await _mediaService.ReplacePostMediaAsync(id, request.CreateMediaPostRequests, innerCt);
                    await _unitOfWork.SaveChangesAsync(innerCt);
                }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cập nhật post thất bại");
                return new AuthResponseDto { Success = false, Message = "Cập nhật post thất bại" };
            }

            return new AuthResponseDto { Success = true, Message = "Cập nhật postId: " + id + " thành công" };
        }

        public async Task<AuthResponseDto> UpdatePostStatusAsync(int id, string status, CancellationToken ct = default)
        {
            var post = await _unitOfWork.Posts.GetByIdAsync(id, includeCollections: false, ct);
            if (post == null)
            {
                return new AuthResponseDto { Success = false, Message = "Không tìm thấy postId: " + id };
            }

            post.Status = status;
            post.UpdatedAt = DateTime.Now;
            _unitOfWork.Posts.Update(post);
            await _unitOfWork.SaveChangesAsync(ct);

            return new AuthResponseDto { Success = true, Message = "Cập nhật trạng thái của postId: " + id + " thành công" };
        }
    }
}
