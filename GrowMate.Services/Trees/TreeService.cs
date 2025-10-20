using GrowMate.Contracts.Requests.Tree;
using GrowMate.Contracts.Responses.Tree;
using GrowMate.Contracts.Responses.Auth;
using GrowMate.Repositories.Interfaces;
using GrowMate.Repositories.Extensions;
using Microsoft.Extensions.Logging;

namespace GrowMate.Services.Trees
{
    public interface ITreeService
    {
        Task<PageResult<TreeResponse>> GetByListingIdAsync(int listingId, int page, int pageSize, CancellationToken ct = default);
        Task<PageResult<TreeResponse>> GetByFarmerIdAsync(int farmerId, int page, int pageSize, CancellationToken ct = default);
        Task<PageResult<TreeResponse>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
        Task<TreeResponse?> GetByIdAsync(int treeId, CancellationToken ct = default);
        Task<TreeDetailResponse?> GetDetailAsync(int treeId, CancellationToken ct = default);
        Task<AuthResponse> CreateTreeAsync(CreateTreeRequest request, CancellationToken ct = default);
        Task<AuthResponse> UpdateTreeAsync(int treeId, UpdateTreeRequest request, CancellationToken ct = default);
        Task<AuthResponse> UpdateTreeStatusAsync(int treeId, UpdateTreeStatusRequest request, CancellationToken ct = default);
        Task<AuthResponse> DeleteTreeAsync(int treeId, CancellationToken ct = default);
    }

    public class TreeService : ITreeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TreeService> _logger;

        public TreeService(IUnitOfWork unitOfWork, ILogger<TreeService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<PageResult<TreeResponse>> GetByListingIdAsync(int listingId, int page, int pageSize, CancellationToken ct = default)
        {
            var trees = await _unitOfWork.Trees.GetByListingIdAsync(listingId, page, pageSize, ct);
            return await MapTreesToResponseAsync(trees, ct);
        }

        public async Task<PageResult<TreeResponse>> GetByFarmerIdAsync(int farmerId, int page, int pageSize, CancellationToken ct = default)
        {
            var trees = await _unitOfWork.Trees.GetByFarmerIdAsync(farmerId, page, pageSize, ct);
            return await MapTreesToResponseAsync(trees, ct);
        }

        public async Task<PageResult<TreeResponse>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
        {
            var trees = await _unitOfWork.Trees.GetAllAsync(page, pageSize, ct);
            return await MapTreesToResponseAsync(trees, ct);
        }

        public async Task<TreeResponse?> GetByIdAsync(int treeId, CancellationToken ct = default)
        {
            var tree = await _unitOfWork.Trees.GetByIdAsync(treeId, ct);
            if (tree == null) return null;

            return await MapTreeToResponseAsync(tree, ct);
        }

        public async Task<TreeDetailResponse?> GetDetailAsync(int treeId, CancellationToken ct = default)
        {
            var tree = await _unitOfWork.Trees.GetByIdWithDetailsAsync(treeId, ct);
            if (tree == null) return null;

            // Get primary image from the associated post
            var primaryImage = await _unitOfWork.Media.GetPrimaryImageByPostIdAsync(tree.Listing.PostId, ct);

            return new TreeDetailResponse
            {
                TreeId = tree.TreeId,
                ListingId = tree.ListingId,
                UniqueCode = tree.UniqueCode,
                Description = tree.Description,
                Coordinates = tree.Coordinates,
                HealthStatus = tree.HealthStatus,
                AvailabilityStatus = tree.AvailabilityStatus,
                CreatedAt = tree.CreatedAt,
                TreeName = tree.Listing.Post?.ProductName ?? "Unknown Tree",
                FarmerName = tree.Listing.Farmer?.FarmName ?? "Unknown Farmer",
                PrimaryImageUrl = primaryImage?.MediaUrl ?? "",
                PricePerTree = tree.Listing.PricePerTree,
                TotalQuantity = tree.Listing.TotalQuantity,
                AvailableQuantity = tree.Listing.AvailableQuantity,
                ListingStatus = tree.Listing.Status,
                Adoptions = tree.Adoptions?.Select(a => new AdoptionSummaryResponse
                {
                    AdoptionId = a.AdoptionId,
                    CustomerId = a.CustomerId,
                    CustomerName = a.Customer?.User?.FullName ?? "Unknown Customer",
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    Status = a.Status,
                    CreatedAt = a.CreatedAt
                }).ToList() ?? new List<AdoptionSummaryResponse>()
            };
        }

        public async Task<AuthResponse> CreateTreeAsync(CreateTreeRequest request, CancellationToken ct = default)
        {
            try
            {
                await _unitOfWork.ExecuteInTransactionAsync(async innerCt =>
                {
                    // Validate listing exists and has available quantity
                    var listing = await _unitOfWork.TreeListings.GetByIdAsync(request.ListingId, includeTrees: false, innerCt);
                    if (listing == null)
                    {
                        throw new InvalidOperationException($"Tree listing with ID {request.ListingId} not found.");
                    }

                    if (listing.AvailableQuantity <= 0)
                    {
                        throw new InvalidOperationException($"No available quantity in tree listing {request.ListingId}.");
                    }

                    // Generate unique code từ PostCode + ListingId + sequence
                    var treeCount = await _unitOfWork.Trees.GetCountByListingIdAsync(request.ListingId, innerCt);
                    var uniqueCode = $"{listing.PostCode}_{listing.ListingId}_{treeCount + 1:D3}";

                    // Check if unique code already exists
                    if (await _unitOfWork.Trees.ExistsByUniqueCodeAsync(uniqueCode, innerCt))
                    {
                        throw new InvalidOperationException($"Tree with unique code '{uniqueCode}' already exists.");
                    }

                    // Create tree
                    var tree = new Models.Tree
                    {
                        ListingId = request.ListingId,
                        UniqueCode = uniqueCode, // Sử dụng generated code
                        Description = request.Description,
                        Coordinates = request.Coordinates,
                        HealthStatus = request.HealthStatus ?? "HEALTHY",
                        AvailabilityStatus = request.AvailabilityStatus ?? "AVAILABLE",
                        CreatedAt = DateTime.Now
                    };

                    await _unitOfWork.Trees.AddAsync(tree, innerCt);

                    // Chỉ CREATE mới trừ availableQuantity
                    // UPDATE và DELETE không động vào availableQuantity
                    listing.AvailableQuantity -= 1;
                    listing.UpdatedAt = DateTime.Now;
                    _unitOfWork.TreeListings.UpdateAsync(listing);

                    await _unitOfWork.SaveChangesAsync(innerCt);
                }, ct);

                return new AuthResponse { Success = true, Message = "Tạo tree thành công!" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create tree for listing {ListingId}", request.ListingId);
                return new AuthResponse { Success = false, Message = "Tạo tree thất bại: " + ex.Message };
            }
        }

        public async Task<AuthResponse> UpdateTreeAsync(int treeId, UpdateTreeRequest request, CancellationToken ct = default)
        {
            try
            {
                var tree = await _unitOfWork.Trees.GetByIdAsync(treeId, ct);
                if (tree == null) 
                    return new AuthResponse { Success = false, Message = "Không tìm thấy treeId: " + treeId };

                await _unitOfWork.ExecuteInTransactionAsync(async innerCt =>
                {
                    if (!string.IsNullOrEmpty(request.UniqueCode) && request.UniqueCode != tree.UniqueCode)
                    {
                        // Check if new unique code already exists
                        if (await _unitOfWork.Trees.ExistsByUniqueCodeAsync(request.UniqueCode, innerCt))
                        {
                            throw new InvalidOperationException($"Tree with unique code '{request.UniqueCode}' already exists.");
                        }
                        tree.UniqueCode = request.UniqueCode;
                    }

                    if (request.Description != null) tree.Description = request.Description;
                    if (request.Coordinates != null) tree.Coordinates = request.Coordinates;
                    if (request.HealthStatus != null) tree.HealthStatus = request.HealthStatus;
                    if (request.AvailabilityStatus != null) tree.AvailabilityStatus = request.AvailabilityStatus;

                    _unitOfWork.Trees.Update(tree);
                    await _unitOfWork.SaveChangesAsync(innerCt);
                }, ct);

                return new AuthResponse { Success = true, Message = "Cập nhật tree thành công!" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update tree {TreeId}", treeId);
                return new AuthResponse { Success = false, Message = "Cập nhật tree thất bại: " + ex.Message };
            }
        }

        public async Task<AuthResponse> UpdateTreeStatusAsync(int treeId, UpdateTreeStatusRequest request, CancellationToken ct = default)
        {
            try
            {
                var tree = await _unitOfWork.Trees.GetByIdAsync(treeId, ct);
                if (tree == null) 
                    return new AuthResponse { Success = false, Message = "Không tìm thấy treeId: " + treeId };

                await _unitOfWork.ExecuteInTransactionAsync(async innerCt =>
                {
                    tree.HealthStatus = request.HealthStatus;
                    tree.AvailabilityStatus = request.AvailabilityStatus;

                    _unitOfWork.Trees.Update(tree);
                    await _unitOfWork.SaveChangesAsync(innerCt);
                }, ct);

                return new AuthResponse { Success = true, Message = "Cập nhật trạng thái tree thành công!" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update tree status {TreeId}", treeId);
                return new AuthResponse { Success = false, Message = "Cập nhật trạng thái tree thất bại: " + ex.Message };
            }
        }

        public async Task<AuthResponse> DeleteTreeAsync(int treeId, CancellationToken ct = default)
        {
            try
            {
                var tree = await _unitOfWork.Trees.GetByIdAsync(treeId, ct);
                if (tree == null) 
                    return new AuthResponse { Success = false, Message = "Không tìm thấy treeId: " + treeId };

                await _unitOfWork.ExecuteInTransactionAsync(async innerCt =>
                {
                    // Soft delete by changing availability status
                    tree.AvailabilityStatus = "DELETED";

                    _unitOfWork.Trees.Update(tree);
                    await _unitOfWork.SaveChangesAsync(innerCt);
                }, ct);

                return new AuthResponse { Success = true, Message = "Xóa tree thành công!" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete tree {TreeId}", treeId);
                return new AuthResponse { Success = false, Message = "Xóa tree thất bại: " + ex.Message };
            }
        }

        private async Task<PageResult<TreeResponse>> MapTreesToResponseAsync(PageResult<Models.Tree> trees, CancellationToken ct)
        {
            var responses = new List<TreeResponse>();
            foreach (var tree in trees.Items)
            {
                responses.Add(await MapTreeToResponseAsync(tree, ct));
            }

            return new PageResult<TreeResponse>
            {
                Items = responses,
                PageNumber = trees.PageNumber,
                PageSize = trees.PageSize,
                TotalItems = trees.TotalItems,
                TotalPages = trees.TotalPages
            };
        }

        private async Task<TreeResponse> MapTreeToResponseAsync(Models.Tree tree, CancellationToken ct)
        {
            // Get primary image from the associated post
            var primaryImage = await _unitOfWork.Media.GetPrimaryImageByPostIdAsync(tree.Listing.PostId, ct);

            return new TreeResponse
            {
                TreeId = tree.TreeId,
                ListingId = tree.ListingId,
                UniqueCode = tree.UniqueCode,
                Description = tree.Description,
                Coordinates = tree.Coordinates,
                HealthStatus = tree.HealthStatus,
                AvailabilityStatus = tree.AvailabilityStatus,
                CreatedAt = tree.CreatedAt,
                TreeName = tree.Listing.Post?.ProductName ?? "Unknown Tree",
                FarmerName = tree.Listing.Farmer?.FarmName ?? "Unknown Farmer",
                PrimaryImageUrl = primaryImage?.MediaUrl ?? ""
            };
        }
    }
}
