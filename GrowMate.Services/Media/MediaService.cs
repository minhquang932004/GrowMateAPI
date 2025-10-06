using System;
using System.Linq;
using GrowMate.Contracts.Requests;
using GrowMate.Contracts.Responses;
using GrowMate.Models;
using GrowMate.Repositories.Extensions;
using GrowMate.Repositories.Interfaces;
using GrowMate.Repositories.Models.Constraints;

namespace GrowMate.Services.Media
{
    public class MediaService : IMediaService
    {
        private readonly IUnitOfWork _unitOfWork;


        public MediaService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

        }

        // Creates media items associated with a post, product, or report
        public async Task CreateMediaAsync(List<MediaItemDto> mediaItems, int? postId = null, int? productId = null, int? reportId = null, CancellationToken ct = default)
        {
            if (mediaItems == null || (postId == null && productId == null && reportId == null))
                throw new ArgumentException("Media items and at least one association id are required.");

            int setCount = new[] { postId, productId, reportId }.Count(x => x != null);
            if (setCount != 1)
                throw new ArgumentException("Exactly one of postId, productId, or reportId must be set.");

            var mediaList = new List<Medium>();
            foreach (var mediaItem in mediaItems)
            {
                if (!Enum.TryParse<MediaType>(mediaItem.MediaType, out var mediaTypeEnum))
                    throw new ArgumentException($"Invalid media type: {mediaItem.MediaType}");

                var medium = new Medium
                {
                    PostId = postId,
                    ProductId = productId,
                    ReportId = reportId,
                    MediaUrl = mediaItem.MediaUrl,
                    MediaType = mediaTypeEnum.ToString(),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                mediaList.Add(medium);
            }
            _unitOfWork.Media.AddRange(mediaList);
            await _unitOfWork.SaveChangesAsync(ct);
        }

        // Replaces all media for a post with new media items
        public async Task ReplacePostMediaAsync(int postId, List<MediaItemDto> newMedia, CancellationToken ct = default)
        {
            var existing = await _unitOfWork.Media.GetByPostIdAsync(postId, ct);
            if (existing.Count > 0)
                _unitOfWork.Media.RemoveRange(existing);

            if (newMedia != null && newMedia.Count > 0)
                await CreateMediaAsync(newMedia, postId: postId, ct: ct);

            await _unitOfWork.SaveChangesAsync(ct);
        }

        // Replaces all media for a product with new media items
        public async Task ReplaceProductMediaAsync(int productId, List<MediaItemDto> newMedia, CancellationToken ct = default)
        {
            var oldMedia = await _unitOfWork.Media.GetByProductIdAsync(productId, ct);
            if (oldMedia.Any())
                _unitOfWork.Media.RemoveRange(oldMedia);

            if (newMedia != null && newMedia.Count > 0)
                await CreateMediaAsync(newMedia, productId: productId, ct: ct);

            await _unitOfWork.SaveChangesAsync(ct);
        }

        // Replaces all media for a report with new media items
        public async Task ReplaceReportMediaAsync(int reportId, List<MediaItemDto> newMedia, CancellationToken ct = default)
        {
            var existing = await _unitOfWork.Media.GetByReportIdAsync(reportId, ct);
            if (existing.Count > 0)
                _unitOfWork.Media.RemoveRange(existing);

            if (newMedia != null && newMedia.Count > 0)
                await CreateMediaAsync(newMedia, reportId: reportId, ct: ct);

            await _unitOfWork.SaveChangesAsync(ct);
        }

        public void Update(Medium media)
        {
            throw new NotImplementedException();
        }
        public Task RemoveAsync(Medium media, CancellationToken ct = default)
        {
            throw new NotImplementedException();
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
    }
}
