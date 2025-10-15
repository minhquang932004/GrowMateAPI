using GrowMate.Models;
using GrowMate.Repositories.Data;
using GrowMate.Repositories.Interfaces;
using GrowMate.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace GrowMate.Repositories
{
    public class EmailVerificationRepository : IEmailVerificationRepository
    {
        private readonly EXE201_GrowMateContext _dbContext;

        public EmailVerificationRepository(EXE201_GrowMateContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task DeleteUnverifiedByUserAsync(int userId, CancellationToken ct = default)
        {
            return _dbContext.EmailVerifications
                .Where(v => v.UserId == userId && v.VerifiedAt == null)
                .ExecuteDeleteAsync(ct);
        }

        public async Task AddAsync(EmailVerification verification, CancellationToken ct = default)
        {
            await _dbContext.EmailVerifications.AddAsync(verification, ct);
        }

        public Task<EmailVerification?> GetLatestUnverifiedAsync(int userId, CancellationToken ct = default)
        {
            var now = DateTime.Now;
            return _dbContext.EmailVerifications
                .Where(v => v.UserId == userId && v.VerifiedAt == null && v.ExpiresAt > now)
                .OrderByDescending(v => v.CreatedAt)
                .FirstOrDefaultAsync(ct);
        }
    }
}