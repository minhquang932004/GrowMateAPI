using GrowMate.Data;
using GrowMate.DTOs.Requests;
using GrowMate.Models;

namespace GrowMate.Services.Media
{
    public class MediaService : IMediaService
    {
        private readonly EXE201_GrowMateContext _context;
        public MediaService(EXE201_GrowMateContext context)
        {
            _context = context;
        }

        //SaveChange ở bên PostService để không bị tách thành 2 commit tránh trường hợp không thể rollback
        public Task CreatePostMediaAsync(int postId, IEnumerable<CreateMediaPostRequest> request)
        {
            if(request == null) return Task.CompletedTask;
            var mediaItem = request.Select(m => new Medium
            {
                PostId = postId,
                MediaUrl = m.MediaUrl,
                MediaType = m.MediaType,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            }).ToList();
            if (mediaItem.Count == 0) return Task.CompletedTask;
            _context.AddRange(mediaItem);
            return Task.CompletedTask;
        }
    }
}
