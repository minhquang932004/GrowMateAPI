using System.Security.Cryptography;
using GrowMate.Contracts.Requests;
using GrowMate.Contracts.Responses;
using GrowMate.Repositories.Interfaces;
using GrowMate.Repositories.Models;
using GrowMate.Repositories.Models.Roles;

namespace GrowMate.Services.EmailRegister
{
    public class RegisterService : IRegisterService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;

        public RegisterService(IUnitOfWork unitOfWork, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken ct = default)
        {
            var email = request.Email.Trim().ToLowerInvariant();

            var user = await _unitOfWork.Users.GetByEmailAsync(email, includeCustomer: false, ct);
            if (user is not null && user.IsActive == true)
            {
                return new AuthResponseDto { Success = false, Message = "Email already registered." };
            }

            if (user is null)
            {
                user = new User
                {
                    FullName = request.FullName.Trim(),
                    Email = email,
                    Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    Role = UserRoles.Guest,
                    IsActive = false
                };

                await _unitOfWork.Users.AddAsync(user, ct);
                await _unitOfWork.SaveChangesAsync(ct); // ensure UserId is generated
            }
            else
            {
                user.FullName = request.FullName.Trim();
                user.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
                _unitOfWork.Users.Update(user);
                await _unitOfWork.SaveChangesAsync(ct);
            }

            // Clean up any previous unverified codes
            await _unitOfWork.EmailVerifications.DeleteUnverifiedByUserAsync(user.UserId, ct);

            // Generate and store new verification code
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

            // Send verification email
            var subject = "GrowMate - Verify your email";
            var body = $@"
                <p>Hello {System.Net.WebUtility.HtmlEncode(user.FullName)},</p>
                <p>Your verification code is: <strong>{code}</strong></p>
                <p>This code will expire in 10 minutes.</p>";

            await _emailService.SendEmailAsync(user.Email, subject, body);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Registration successful. Please check your email for the verification code."
            };
        }

        public async Task<AuthResponseDto> VerifyEmailAsync(VerifyEmailRequestDto request, CancellationToken ct = default)
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var user = await _unitOfWork.Users.GetByEmailAsync(email, includeCustomer: false, ct);
            if (user is null)
            {
                return new AuthResponseDto { Success = false, Message = "User not found." };
            }

            var latest = await _unitOfWork.EmailVerifications.GetLatestUnverifiedAsync(user.UserId, ct);
            if (latest is null)
            {
                return new AuthResponseDto { Success = false, Message = "Verification code is invalid or expired." };
            }

            var ok = BCrypt.Net.BCrypt.Verify(request.Code, latest.CodeHash);
            if (!ok)
            {
                return new AuthResponseDto { Success = false, Message = "Verification code is incorrect." };
            }

            latest.VerifiedAt = DateTime.UtcNow;
            user.IsActive = true;
            user.Role = UserRoles.Customer;

            _unitOfWork.Users.Update(user);

            // Ensure Customer row exists after activation
            var hasCustomer = await _unitOfWork.Customers.AnyAsync(user.UserId, ct);
            if (!hasCustomer)
            {
                await _unitOfWork.Customers.CreateAsync(new Customer
                {
                    UserId = user.UserId,          // FIX: do not set CustomerId (identity)
                    CreatedAt = DateTime.UtcNow
                }, ct);
            }

            await _unitOfWork.SaveChangesAsync(ct);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Email verified successfully."
            };
        }

        public async Task<AuthResponseDto> ResendVerificationCodeAsync(ResendVerificationRequestDto request, CancellationToken ct = default)
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var user = await _unitOfWork.Users.GetByEmailAsync(email, includeCustomer: false, ct);
            if (user is null)
            {
                return new AuthResponseDto { Success = false, Message = "User not found." };
            }

            if (user.IsActive == true)
            {
                return new AuthResponseDto { Success = false, Message = "Email already verified." };
            }

            // Simple cooldown to avoid abuse (60s between resends)
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
                        Message = $"Please wait {remaining} seconds before requesting another verification code."
                    };
                }
            }

            // Remove old unverified codes
            await _unitOfWork.EmailVerifications.DeleteUnverifiedByUserAsync(user.UserId, ct);

            // Create new code
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

            // Send email
            var subject = "GrowMate - Verify your email";
            var body = $@"
                <p>Hello {System.Net.WebUtility.HtmlEncode(user.FullName)},</p>
                <p>Your verification code is: <strong>{code}</strong></p>
                <p>This code will expire in 10 minutes.</p>";

            await _emailService.SendEmailAsync(user.Email, subject, body);

            return new AuthResponseDto
            {
                Success = true,
                Message = "A new verification code has been sent to your email."
            };
        }
    }
}