using GrowMate.Repositories.Models;

namespace GrowMate.Repositories.Interfaces
{
    public interface IEmailVerificationRepository
    {
        Task DeleteUnverifiedByUserAsync(int userId, CancellationToken ct = default);
        Task AddAsync(EmailVerification verification, CancellationToken ct = default);
        Task<EmailVerification?> GetLatestUnverifiedAsync(int userId, CancellationToken ct = default);
    }
}