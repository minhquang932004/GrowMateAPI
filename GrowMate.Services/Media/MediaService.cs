using System;
using System.Linq;
using GrowMate.Contracts.Requests;
using GrowMate.Contracts.Responses;
using GrowMate.Models;
using GrowMate.Repositories.Extensions;
using GrowMate.Repositories.Interfaces;
using GrowMate.Repositories.Models;
using GrowMate.Services.Posts;
using GrowMate.Services.Products;

namespace GrowMate.Services.Media
{
    public class MediaService : IMediaService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPostService _postService;
        private readonly IProductService _productService;

        public MediaService(IUnitOfWork unitOfWork, IPostService postService, IProductService productService)
        {
            _unitOfWork = unitOfWork;
            _postService = postService;
            _productService = productService;
        }

        public Task CreateMediaAsync(int? postId, int? reportId, int? productId, IEnumerable<CreateMediaRequest>? request, CancellationToken ct = default)
        {
            if (request == null) return Task.CompletedTask;

            var media = request.Select(m => new Medium
            {
                PostId = postId ?? null,
                ReportId = reportId ?? null,
                ProductId = productId ?? null,
                MediaUrl = m.MediaUrl,
                MediaType = m.MediaType,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            }).ToList();

            if (media.Count == 0) return Task.CompletedTask;

            _unitOfWork.Media.AddRange(media);
            return Task.CompletedTask;
        }

        public Task<PageResult<Medium>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
            => _unitOfWork.Media.GetAllAsync(page, pageSize, ct);

        public async Task<MediaResponse> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var item = await _unitOfWork.Media.GetByIdAsync(id, ct);
            if (item == null) return null;
            return new MediaResponse
            {
                MediaId = item.MediaId,
                MediaUrl = item.MediaUrl,
                MediaType = item.MediaType
            };
        }

        public Task<List<Medium>> GetByPostIdAsync(int postId, CancellationToken ct = default)
            => _unitOfWork.Media.GetByPostIdAsync(postId, ct);

        public Task<List<Medium>> GetByProductIdAsync(int productId, CancellationToken ct = default)
            => _unitOfWork.Media.GetByProductIdAsync(productId, ct);

        public Task<List<Medium>> GetByReportIdAsync(int reportId, CancellationToken ct = default)
            => _unitOfWork.Media.GetByReportIdAsync(reportId, ct);

        public Task RemoveAsync(Medium media, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public async Task ReplacePostMediaAsync(int postId, IEnumerable<CreateMediaRequest>? request, CancellationToken ct = default)
        {
            var existing = await _unitOfWork.Media.GetByPostIdAsync(postId, ct);
            if (existing.Count > 0)
                _unitOfWork.Media.RemoveRange(existing);

            await CreateMediaAsync(postId,0,0, request, ct);
        }

        public void Update(Medium media)
        {
            throw new NotImplementedException();
        }
    }
}
