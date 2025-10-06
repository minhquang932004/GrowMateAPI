using System;
using System.Linq;
using GrowMate.Contracts.Requests;
using GrowMate.Models;
using GrowMate.Repositories.Interfaces;
using GrowMate.Repositories.Models;

namespace GrowMate.Services.Media
{
    public class MediaService : IMediaService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MediaService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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

        public async Task ReplacePostMediaAsync(int postId, IEnumerable<CreateMediaRequest>? request, CancellationToken ct = default)
        {
            var existing = await _unitOfWork.Media.GetByPostIdAsync(postId, ct);
            if (existing.Count > 0)
                _unitOfWork.Media.RemoveRange(existing);

            await CreateMediaAsync(postId,0,0, request, ct);
        }
    }
}
