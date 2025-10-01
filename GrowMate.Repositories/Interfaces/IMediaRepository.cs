using GrowMate.Models;
using GrowMate.Repositories.Models;

namespace GrowMate.Repositories.Interfaces
{
    public interface IMediaRepository
    {
        Task<List<Medium>> GetByPostIdAsync(int postId, CancellationToken ct = default);
        void AddRange(IEnumerable<Medium> media);
        void RemoveRange(IEnumerable<Medium> media);
    }
}