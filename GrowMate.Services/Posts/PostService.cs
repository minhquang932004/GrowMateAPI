using GrowMate.Contracts.Requests;
using GrowMate.Contracts.Requests.Post;  // Add domain-specific namespace for Post requests
using GrowMate.Contracts.Responses;
using GrowMate.Contracts.Responses.Auth;  // Add domain-specific namespace for Auth responses
using GrowMate.Contracts.Responses.Post;  // Add domain-specific namespace for Post responses
using GrowMate.Models;
using GrowMate.Repositories.Extensions;
using GrowMate.Repositories.Interfaces;
using GrowMate.Repositories.Models;
using GrowMate.Repositories.Models.Statuses;
using GrowMate.Services.Farmers;
using GrowMate.Services.Media;
using GrowMate.Services.TreeListings;
using Microsoft.Extensions.Logging;

namespace GrowMate.Services.Posts
{
    public class PostService : IPostService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFarmerService _farmerService;
        private readonly ILogger<PostService> _logger;
        private readonly IMediaService _mediaService;
        private readonly ITreeListingService _treeListingService;

        public PostService(IUnitOfWork unitOfWork, IFarmerService farmerService, ILogger<PostService> logger, IMediaService mediaService, ITreeListingService treeListingService)
        {
            _unitOfWork = unitOfWork;
            _farmerService = farmerService;
            _logger = logger;
            _mediaService = mediaService;
            _treeListingService = treeListingService;
        }

        public async Task<PageResult<PostListItemResponse>> GetAllPostsAsync(int page, int pageSize, CancellationToken ct = default)
        {
            var result = await _unitOfWork.Posts.GetAllAsync(page, pageSize, ct);
            
            var items = new List<PostListItemResponse>();
            foreach (var post in result.Items)
            {
                var primaryMedia = await _unitOfWork.Media.GetPrimaryImageByPostIdAsync(post.PostId, ct);
                
                items.Add(new PostListItemResponse
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
                    PrimaryImageUrl = primaryMedia?.MediaUrl ?? ""
                });
            }
            
            return new PageResult<PostListItemResponse>
            {
                Items = items,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
                TotalItems = result.TotalItems,
                TotalPages = result.TotalPages
            };
        }

        public async Task<PageResult<PostListItemResponse>> GetAllPostsByFarmerIdAsync(int id, int page, int pageSize, CancellationToken ct = default)
        {
            var result = await _unitOfWork.Posts.GetByFarmerIdAsync(id, page, pageSize, ct);
            
            var items = new List<PostListItemResponse>();
            foreach (var post in result.Items)
            {
                var primaryMedia = await _unitOfWork.Media.GetPrimaryImageByPostIdAsync(post.PostId, ct);
                
                items.Add(new PostListItemResponse
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
                    PrimaryImageUrl = primaryMedia?.MediaUrl ?? ""
                });
            }
            
            return new PageResult<PostListItemResponse>
            {
                Items = items,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
                TotalItems = result.TotalItems,
                TotalPages = result.TotalPages
            };
        }

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
                MediaPostList = post.Media?.Select(m => new MediaPostResponse
                {
                    MediaId = m.MediaId,
                    PostId = post.PostId,
                    MediaUrl = m.MediaUrl,
                    MediaType = m.MediaType,
                    CreatedAt = m.CreatedAt,
                    UpdatedAt = m.UpdatedAt
                }).ToList() ?? new List<MediaPostResponse>(),
                PostCommentList = post.PostComments?.Select(c => new PostCommentResponse
                {
                    CommentId = c.CommentId,
                    PostId = c.PostId,
                    UserId = c.UserId,
                    CommentContent = c.CommentContent,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                }).ToList() ?? new List<PostCommentResponse>(),
                MainImageUrl = post.Media?.FirstOrDefault(m => m.IsPrimary)?.MediaUrl ?? post.Media?.FirstOrDefault()?.MediaUrl
            };
        }

        public async Task<AuthResponse> CreatePostAsync(CreatePostRequest request, CancellationToken ct = default)
        {
            if (!await _farmerService.GetFarmerByIdAsync(request.FarmerId))
                return new AuthResponse { Success = false, Message = "Không tìm thấy farmerId: " + request.FarmerId };

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

                    if (request.Media != null && request.Media.Count > 0)
                        await _mediaService.CreateMediaAsync(request.Media, postId: newPost.PostId, ct: innerCt);
                }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tạo post thất bại");
                return new AuthResponse { Success = false, Message = "Tạo mới post thất bại" };
            }

            return new AuthResponse { Success = true, Message = "Tạo mới post thành công!" };
        }

        public async Task<AuthResponse> DeletePostAsync(int id, CancellationToken ct = default)
        {
            var post = await _unitOfWork.Posts.GetByIdAsync(id, includeCollections: false, ct);
            if (post == null)
                return new AuthResponse { Success = false, Message = "Không tìm thấy postId: " + id };

            post.Status = PostStatuses.Canceled;               // <- was "CANCELED"
            post.UpdatedAt = DateTime.Now;
            _unitOfWork.Posts.Update(post);
            await _unitOfWork.SaveChangesAsync(ct);

            return new AuthResponse { Success = true, Message = "Đã CANCELED postId: " + id + " thành công" };
        }

        public async Task<AuthResponse> UpdatePostAsync(int id, CreatePostRequest request, CancellationToken ct = default)
        {
            var post = await _unitOfWork.Posts.GetByIdAsync(id, includeCollections: false, ct);
            if (post == null)
                return new AuthResponse { Success = false, Message = "Không tìm thấy postId: " + id };

            if (!await _farmerService.GetFarmerByIdAsync(request.FarmerId))
                return new AuthResponse { Success = false, Message = "Không tìm thấy farmerId: " + request.FarmerId };

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
                    post.Status = PostStatuses.Pending;
                    post.UpdatedAt = DateTime.Now;

                    _unitOfWork.Posts.Update(post);

                    await _mediaService.ReplacePostMediaAsync(id, request.Media, innerCt);
                    await _unitOfWork.SaveChangesAsync(innerCt);
                }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cập nhật post thất bại");
                return new AuthResponse { Success = false, Message = "Cập nhật post thất bại" };
            }

            return new AuthResponse { Success = true, Message = "Cập nhật postId: " + id + " thành công" };
        }

        public async Task<AuthResponse> UpdatePostStatusAsync(int id, string status, CancellationToken ct = default)
        {
            var post = await _unitOfWork.Posts.GetByIdAsync(id, includeCollections: false, ct);
            if (post == null)
            {
                return new AuthResponse { Success = false, Message = "Không tìm thấy postId: " + id };
            }
            try
            {
                await _unitOfWork.ExecuteInTransactionAsync(async innerCt =>
                {
                    post.Status = status;
                    post.UpdatedAt = DateTime.Now;
                    _unitOfWork.Posts.Update(post);
                    await _unitOfWork.SaveChangesAsync(innerCt);
                    var checkList = await _treeListingService.GetByPostIdAsync(id, false, innerCt);
                    if (post.Status.Equals(PostStatuses.Approved) && checkList == null)
                    {
                        var newTreeListing = new TreeListing
                        {
                            PostId = post.PostId,
                            FarmerId = post.FarmerId,
                            PricePerTree = post.PricePerYear,
                            TotalQuantity = post.TreeQuantity,
                            AvailableQuantity = post.TreeQuantity,
                            Status = TreeListingStatuses.Active,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now,
                        };
                        await _treeListingService.AddAsync(newTreeListing, innerCt);
                        await _unitOfWork.SaveChangesAsync(innerCt);
                    }
                    else if (post.Status.Equals(PostStatuses.Approved) && checkList != null)
                    {
                        var checkQuantity = checkList.TotalQuantity - checkList.AvailableQuantity + post.TreeQuantity;
                        var newTreeListing = new TreeListing
                        {
                            ListingId = checkList.ListingId,
                            PostId = post.PostId,
                            FarmerId = post.FarmerId,
                            PricePerTree = post.PricePerYear,
                            TotalQuantity = checkQuantity,
                            AvailableQuantity = post.TreeQuantity,
                            Status = TreeListingStatuses.Active,
                            CreatedAt = checkList.CreatedAt,
                            UpdatedAt = DateTime.Now,
                        };
                        _treeListingService.UpdateAsync(newTreeListing);
                        await _unitOfWork.SaveChangesAsync(innerCt);
                    }
                    else if (post.Status.Equals(PostStatuses.Canceled) && checkList != null)
                    {
                        var newTreeListing = new TreeListing
                        {
                            ListingId = checkList.ListingId,
                            PostId = post.PostId,
                            FarmerId = post.FarmerId,
                            PricePerTree = post.PricePerYear,
                            TotalQuantity = post.TreeQuantity,
                            AvailableQuantity = post.TreeQuantity,
                            Status = TreeListingStatuses.Removed,
                            CreatedAt = checkList.CreatedAt,
                            UpdatedAt = DateTime.Now,
                        };
                        _treeListingService.UpdateAsync(newTreeListing);
                        await _unitOfWork.SaveChangesAsync(innerCt);
                    }
                    
                }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cập nhật trạng thái post thất bại");
                return new AuthResponse { Success = false, Message = "Cập nhật trạng thái post thất bại" };
            }
            return new AuthResponse { Success = true, Message = "Cập nhật trạng thái của postId: " + id + " thành công" };
        }
    }
}
