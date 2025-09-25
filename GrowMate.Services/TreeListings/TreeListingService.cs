using GrowMate.Contracts.Responses;
using GrowMate.Repositories.Extensions;
using GrowMate.Repositories.Interfaces;
using GrowMate.Repositories.Models;
using GrowMate.Services.Posts;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrowMate.Services.TreeListings
{
    public class TreeListingService : ITreeListingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TreeListingService> _logger;

        public TreeListingService(IUnitOfWork unitOfWork, ILogger<TreeListingService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task AddAsync(TreeListing treeListing, CancellationToken ct = default)
        {
            await _unitOfWork.TreeListings.AddAsync(treeListing, ct);
        }

        public Task<PageResult<TreeListing>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
            => _unitOfWork.TreeListings.GetAllAsync(page, pageSize, ct);

        public Task<PageResult<TreeListing>> GetByFarmerIdAsync(int farmerId, int page, int pageSize, CancellationToken ct = default)
            => _unitOfWork.TreeListings.GetByFarmerIdAsync(farmerId, page, pageSize, ct);

        public async Task<TreeListingResponse?> GetByIdAsync(int id, bool includeTrees, CancellationToken ct = default)
        {
            var item = await _unitOfWork.TreeListings.GetByIdAsync(id, includeTrees, ct);
            if (item == null)
            {
                return null;
            }
            return new TreeListingResponse
            {
                ListingId = item.ListingId,
                PostId = item.PostId,
                FarmerId = item.FarmerId,
                PricePerTree = item.PricePerTree,
                TotalQuantity = item.TotalQuantity,
                AvailableQuantity = item.AvailableQuantity,
                Status = item.Status,
                CreatedAt = item.CreatedAt,
                UpdatedAt = item.UpdatedAt,
                TreeResponses = item.Trees?.Select(a => new TreeResponse
                {
                    TreeId = a.TreeId,
                    ListingId = a.ListingId,
                    UniqueCode = a.UniqueCode,
                    Description = a.Description,
                    Coordinates = a.Coordinates,
                    HealthStatus = a.HealthStatus,
                    AvailabilityStatus = a.AvailabilityStatus,
                    CreatedAt = a.CreatedAt
                }).ToList() ?? new List<TreeResponse>(),
            };
        }

        public async Task<TreeListingResponse> GetByPostIdAsync(int postId, bool includeTrees, CancellationToken ct = default)
        {
            var item = await _unitOfWork.TreeListings.GetByIdAsync(postId, includeTrees, ct);
            if (item == null)
            {
                return null;
            }
            return new TreeListingResponse
            {
                ListingId = item.ListingId,
                PostId = item.PostId,
                FarmerId = item.FarmerId,
                PricePerTree = item.PricePerTree,
                TotalQuantity = item.TotalQuantity,
                AvailableQuantity = item.AvailableQuantity,
                Status = item.Status,
                CreatedAt = item.CreatedAt,
                UpdatedAt = item.UpdatedAt,
                TreeResponses = item.Trees?.Select(a => new TreeResponse
                {
                    TreeId = a.TreeId,
                    ListingId = a.ListingId,
                    UniqueCode = a.UniqueCode,
                    Description = a.Description,
                    Coordinates = a.Coordinates,
                    HealthStatus = a.HealthStatus,
                    AvailabilityStatus = a.AvailabilityStatus,
                    CreatedAt = a.CreatedAt
                }).ToList() ?? new List<TreeResponse>(),
            };
        }

        public void UpdateAsync(TreeListing treeListing)
        {
            _unitOfWork.TreeListings.UpdateAsync(treeListing);
        }
    }
}
