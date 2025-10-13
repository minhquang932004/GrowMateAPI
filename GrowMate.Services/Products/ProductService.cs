using GrowMate.Contracts.Requests;
using GrowMate.Contracts.Requests.Product; // Add domain-specific namespace for Product requests
using GrowMate.Contracts.Requests.Media;   // Add domain-specific namespace for Media requests
using GrowMate.Contracts.Responses;
using GrowMate.Contracts.Responses.Product; // Add domain-specific namespace for Product responses
using GrowMate.Contracts.Responses.Media;   // Add domain-specific namespace for Media responses
using GrowMate.Models;
using GrowMate.Repositories.Extensions;
using GrowMate.Repositories.Interfaces;
using GrowMate.Repositories.Models.Statuses;
using GrowMate.Repositories.Models.Constraints; // For MediaType enum
using Microsoft.Extensions.Logging;

namespace GrowMate.Services.Products
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<ProductService> _logger;
        private readonly IMediaService _mediaService;

        public ProductService(IUnitOfWork uow, ILogger<ProductService> logger, IMediaService mediaService)
        {
            _uow = uow;
            _logger = logger;
            _mediaService = mediaService;
        }

        // Farmer creates a product -> PENDING (mirrors CreatePost)
        public async Task<int> CreateProductAsync(CreateProductRequest request, CancellationToken ct = default)
        {
            var product = new Product
            {
                Name = request.Name,
                Description = request.Description,
                // ... other fields
                Status = ProductStatuses.Pending,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _uow.ExecuteInTransactionAsync(async token =>
            {
                await _uow.Products.AddAsync(product, token);
                await _uow.SaveChangesAsync(token);

                if (request.Media != null)
                {
                    foreach (var mediaItem in request.Media)
                    {
                        // Validate media type
                        if (!Enum.TryParse<MediaType>(mediaItem.MediaType, out var mediaTypeEnum))
                        {
                            throw new ArgumentException($"Invalid media type: {mediaItem.MediaType}");
                        }

                        var medium = new Medium
                        {
                            ProductId = product.ProductId,
                            MediaUrl = mediaItem.MediaUrl,
                            MediaType = mediaTypeEnum.ToString(), // Store as string for compatibility
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        };
                        await _uow.Media.AddAsync(medium, token);
                    }
                    await _uow.SaveChangesAsync(token);
                }
            }, ct);

            return product.ProductId;
        }

        // Admin lists pending products (entity-based, unchanged)
        public Task<PageResult<Product>> GetPendingAsync(int page, int pageSize, CancellationToken ct = default)
            => _uow.Products.GetPendingAsync(page, pageSize, includeCollections: false, ct);

        // Entity-based approved page (if still needed elsewhere)
        public Task<PageResult<Product>> GetApprovedAsync(int page, int pageSize, CancellationToken ct = default)
            => _uow.Products.GetApprovedAsync(page, pageSize, includeCollections: false, ct);

        // Entity-based detail (kept for internal usage)
        public Task<Product?> GetByIdAsync(int id, bool includeCollections = false, CancellationToken ct = default)
            => _uow.Products.GetByIdAsync(id, includeCollections, ct);

        // Farmer updates product -> reset to PENDING (mirrors UpdatePost)
        public async Task<bool> UpdateProductAsync(int id, CreateProductRequest request, CancellationToken ct = default)
        {
            var product = await _uow.Products.GetByIdAsync(id, includeCollections: false, ct);
            if (product == null)
                return false;

            try
            {
                await _uow.ExecuteInTransactionAsync(async innerCt =>
                {
                    // Update product fields
                    product.FarmerId = request.FarmerId;
                    product.CategoryId = request.CategoryId;
                    product.ProductTypeId = request.ProductTypeId;
                    product.UnitId = request.UnitId;
                    product.Name = request.Name;
                    product.Slug = request.Slug;
                    product.Description = request.Description;
                    product.Price = request.Price;
                    product.Stock = request.Stock;
                    product.Status = ProductStatuses.Pending;
                    product.UpdatedAt = DateTime.Now;

                    _uow.Products.Update(product);

                    // Replace all media for this product
                    if (request.Media != null)
                    {
                        await _mediaService.ReplaceProductMediaAsync(id, request.Media, innerCt);
                    }

                    await _uow.SaveChangesAsync(innerCt);
                }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update product {ProductId}", id);
                return false;
            }

            return true;
        }

        // Admin approves/rejects (mirrors UpdatePostStatus)
        public async Task<bool> UpdateProductStatusAsync(int id, string status, CancellationToken ct = default)
        {
            var product = await _uow.Products.GetByIdAsync(id, includeCollections: false, ct);
            if (product is null) return false;

            try
            {
                await _uow.ExecuteInTransactionAsync(async innerCt =>
                {
                    product.Status = status;
                    product.UpdatedAt = DateTime.Now;
                    _uow.Products.Update(product);
                    await _uow.SaveChangesAsync(innerCt);
                }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update product status {ProductId}", id);
                return false;
            }

            return true;
        }

        // New DTO: Detail
        public async Task<ProductDetailResponse?> GetDetailAsync(int id, CancellationToken ct = default)
        {
            var p = await _uow.Products.GetByIdAsync(id, includeCollections: true, ct);
            if (p is null) return null;

            return new ProductDetailResponse
            {
                ProductId = p.ProductId,
                FarmerId = p.FarmerId,
                Name = p.Name,
                Slug = p.Slug,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                Status = p.Status,
                CategoryName = p.Category?.Name,
                ProductTypeName = p.ProductType?.Name,
                UnitName = p.Unit?.Name,
                FarmerName = p.Farmer?.FarmName,
                Media = p.Media?.Select(m => new MediaResponse
                {
                    MediaId = m.MediaId,
                    MediaUrl = m.MediaUrl,
                    MediaType = m.MediaType
                }).ToList() ?? new List<MediaResponse>(),
                MainImageUrl = p.Media?.FirstOrDefault()?.MediaUrl
            };
        }

        public async Task<bool> DeleteProductAsync(int id, CancellationToken ct = default)
        {
            var product = await _uow.Products.GetByIdAsync(id, includeCollections: false, ct);
            if (product == null)
                return false;

            try
            {
                await _uow.ExecuteInTransactionAsync(async innerCt =>
                {
                    product.Status = ProductStatuses.Canceled; // Mark as canceled (soft delete)
                    product.UpdatedAt = DateTime.Now;
                    _uow.Products.Update(product);

                    // Optionally, remove all associated media (Medium/Media)
                    var mediaList = await _uow.Media.GetByProductIdAsync(id, innerCt);
                    if (mediaList.Any())
                        _uow.Media.RemoveRange(mediaList);

                    await _uow.SaveChangesAsync(innerCt);
                }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete product {ProductId}", id);
                return false;
            }

            return true;
        }

        // New DTO: Approved list (paged)
        public async Task<PageResult<ProductListItemResponse>> GetApprovedListAsync(int page, int pageSize, CancellationToken ct = default)
        {
            var res = await _uow.Products.GetApprovedAsync(page, pageSize, includeCollections: true, ct);
            return new PageResult<ProductListItemResponse>
            {
                Items = res.Items.Select(p => new ProductListItemResponse
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Slug = p.Slug,
                    Price = p.Price,
                    Stock = p.Stock,
                    Status = p.Status,
                    CategoryName = p.Category?.Name,
                    ProductTypeName = p.ProductType?.Name,
                    UnitName = p.Unit?.Name,
                    FarmerName = p.Farmer?.FarmName,
                    MainImageUrl = p.Media?.Select(m => m.MediaUrl).FirstOrDefault()
                }).ToList(),
                PageNumber = res.PageNumber,
                PageSize = res.PageSize,
                TotalItems = res.TotalItems,
                TotalPages = res.TotalPages
            };
        }
    }
}