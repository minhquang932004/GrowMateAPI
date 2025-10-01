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

        public Task CreatePostMediaAsync(int postId, IEnumerable<CreateMediaPostRequest>? request, CancellationToken ct = default)
        {
            if (request == null) return Task.CompletedTask;

            var media = request.Select(m => new Medium
            {
                PostId = postId,
                MediaUrl = m.MediaUrl,
                MediaType = m.MediaType,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            }).ToList();

            if (media.Count == 0) return Task.CompletedTask;

            _unitOfWork.Media.AddRange(media);
            return Task.CompletedTask;
        }

        public async Task ReplacePostMediaAsync(int postId, IEnumerable<CreateMediaPostRequest>? request, CancellationToken ct = default)
        {
            var existing = await _unitOfWork.Media.GetByPostIdAsync(postId, ct);
            if (existing.Count > 0)
                _unitOfWork.Media.RemoveRange(existing);

            await CreatePostMediaAsync(postId, request, ct);
        }
    }
}
