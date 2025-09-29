using System.Security.Cryptography;
using GrowMate.Contracts.Requests;
using GrowMate.Contracts.Responses;
using GrowMate.Repositories.Interfaces;
using GrowMate.Repositories.Models;
using GrowMate.Services.EmailRegister;

namespace GrowMate.Services.Authentication
{
    public class PasswordResetService : IPasswordResetService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;

        public PasswordResetService(IUnitOfWork unitOfWork, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }

        public async Task<AuthResponseDto> RequestResetAsync(ForgotPasswordRequestDto request, CancellationToken ct = default)
        {
            var email = request.Email?.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(email))
                return new AuthResponseDto { Success = false, Message = "Email is required." };

            var user = await _unitOfWork.Users.GetByEmailAsync(email, includeCustomer: false, ct);
            if (user is null)
                return new AuthResponseDto { Success = false, Message = "User not found." };

            if (user.IsActive != true)
                return new AuthResponseDto { Success = false, Message = "Email is not verified." };

            // Cooldown: 60s between reset requests
            var latest = await _unitOfWork.EmailVerifications.GetLatestUnverifiedAsync(user.UserId, ct);
            if (latest is not null)
            {
                var nextAllowedAt = latest.CreatedAt.AddMinutes(1);
                if (DateTime.UtcNow < nextAllowedAt)
                {
                    var remaining = (int)Math.Ceiling((nextAllowedAt - DateTime.UtcNow).TotalSeconds);
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = $"Please wait {remaining} seconds before requesting another reset code."
                    };
                }
            }

            // Remove previous unverified codes and create a new one-time reset code
            await _unitOfWork.EmailVerifications.DeleteUnverifiedByUserAsync(user.UserId, ct);

            var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
            var verification = new EmailVerification
            {
                UserId = user.UserId,
                CodeHash = BCrypt.Net.BCrypt.HashPassword(code),
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.EmailVerifications.AddAsync(verification, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            // Send reset email
            var subject = "GrowMate - Reset your password";
            var body = $@"
                <p>Hello {System.Net.WebUtility.HtmlEncode(user.FullName)},</p>
                <p>Your password reset code is: <strong>{code}</strong></p>
                <p>This code will expire in 10 minutes.</p>";

            await _emailService.SendEmailAsync(user.Email, subject, body);

            return new AuthResponseDto
            {
                Success = true,
                Message = "A password reset code has been sent to your email."
            };
        }

        public async Task<AuthResponseDto> ResetPasswordAsync(ResetPasswordRequestDto request, CancellationToken ct = default)
        {
            var email = request.Email?.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(email))
                return new AuthResponseDto { Success = false, Message = "Email is required." };
            if (string.IsNullOrWhiteSpace(request.Code))
                return new AuthResponseDto { Success = false, Message = "Reset code is required." };
            if (string.IsNullOrWhiteSpace(request.NewPassword))
                return new AuthResponseDto { Success = false, Message = "New password is required." };

            var user = await _unitOfWork.Users.GetByEmailAsync(email, includeCustomer: false, ct);
            if (user is null)
                return new AuthResponseDto { Success = false, Message = "User not found." };

            if (user.IsActive != true)
                return new AuthResponseDto { Success = false, Message = "Email is not verified." };

            var latest = await _unitOfWork.EmailVerifications.GetLatestUnverifiedAsync(user.UserId, ct);
            if (latest is null || latest.ExpiresAt < DateTime.UtcNow)
                return new AuthResponseDto { Success = false, Message = "Reset code is invalid or expired." };

            var ok = BCrypt.Net.BCrypt.Verify(request.Code, latest.CodeHash);
            if (!ok)
                return new AuthResponseDto { Success = false, Message = "Reset code is incorrect." };

            // Consume the code and update password
            latest.VerifiedAt = DateTime.UtcNow;
            latest.ExpiresAt = DateTime.UtcNow;

            user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync(ct);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Password has been reset successfully."
            };
        }
    }
}