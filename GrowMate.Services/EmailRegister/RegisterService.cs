using System.Security.Cryptography;
using GrowMate.Contracts.Requests;
using GrowMate.Contracts.Requests.Auth;
using GrowMate.Contracts.Responses;
using GrowMate.Contracts.Responses.Auth;
using GrowMate.Models;
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

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
        {
            var email = request.Email.Trim().ToLowerInvariant();

            var user = await _unitOfWork.Users.GetByEmailAsync(email, includeCustomer: false, ct);
            if (user is not null && user.IsActive == true)
            {
                return new AuthResponse { Success = false, Message = "Email already registered." };
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
                ExpiresAt = DateTime.Now.AddMinutes(10),
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.EmailVerifications.AddAsync(verification, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            // Send verification email
            var subject = "GrowMate - Verify your email";
            var body = $@"
<!DOCTYPE html>
<html lang='vi'>
<head>
  <meta charset='UTF-8' />
  <title>GrowMate OTP</title>
</head>
<body style='margin:0;padding:0;background-color:#0b0f0d;'>
  <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%' style='background-color:#0b0f0d;'>
    <tr>
      <td align='center' style='padding:40px 0;'>
        <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='480' style='background-color:#111814;border-radius:16px;padding:32px;text-align:center;box-shadow:0 0 20px rgba(34,197,94,0.15);'>
          <tr>
            <td style='font-family:Arial,sans-serif;color:#bbf7d0;font-size:22px;font-weight:bold;'>Xác thực đăng nhập</td>
          </tr>
          <tr><td style='padding:16px 0;font-family:Arial,sans-serif;color:#f3f3f3;font-size:15px;'>
            Xin chào {System.Net.WebUtility.HtmlEncode(user.FullName)},<br/>
            Mã OTP để xác thực tài khoản GrowMate của bạn là:
          </td></tr>
          <tr><td>
            <div style='
              display:inline-block;
              padding:16px 32px;
              font-size:28px;
              letter-spacing:4px;
              font-weight:bold;
              color:#16a34a;
              background-color:#022c22;
              border:2px solid rgba(134,239,172,0.3);
              border-radius:12px;
              '>{code}</div>
          </td></tr>
          <tr><td style='padding-top:24px;font-size:14px;color:#9ca3af;font-family:Arial,sans-serif;'>
            Mã này có hiệu lực trong 10 phút.
          </td></tr>
          <tr><td style='padding-top:24px;font-size:13px;color:#6b7280;font-family:Arial,sans-serif;'>
            © GrowMate Team. All rights reserved.
          </td></tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";

            await _emailService.SendEmailAsync(user.Email, subject, body);

            return new AuthResponse
            {
                Success = true,
                Message = "Registration successful. Please check your email for the verification code."
            };
        }

        public async Task<AuthResponse> VerifyEmailAsync(VerifyEmailRequest request, CancellationToken ct = default)
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var user = await _unitOfWork.Users.GetByEmailAsync(email, includeCustomer: false, ct);
            if (user is null)
            {
                return new AuthResponse { Success = false, Message = "User not found." };
            }

            var latest = await _unitOfWork.EmailVerifications.GetLatestUnverifiedAsync(user.UserId, ct);
            if (latest is null)
            {
                return new AuthResponse { Success = false, Message = "Verification code is invalid or expired." };
            }

            var ok = BCrypt.Net.BCrypt.Verify(request.Code, latest.CodeHash);
            if (!ok)
            {
                return new AuthResponse { Success = false, Message = "Verification code is incorrect." };
            }

            latest.VerifiedAt = DateTime.Now;
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
                    CreatedAt = DateTime.Now
                }, ct);
            }

            await _unitOfWork.SaveChangesAsync(ct);

            return new AuthResponse
            {
                Success = true,
                Message = "Email verified successfully."
            };
        }

        public async Task<AuthResponse> ResendVerificationCodeAsync(ResendVerificationRequest request, CancellationToken ct = default)
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var user = await _unitOfWork.Users.GetByEmailAsync(email, includeCustomer: false, ct);
            if (user is null)
            {
                return new AuthResponse { Success = false, Message = "User not found." };
            }

            if (user.IsActive == true)
            {
                return new AuthResponse { Success = false, Message = "Email already verified." };
            }

            // Simple cooldown to avoid abuse (60s between resends)
            var latest = await _unitOfWork.EmailVerifications.GetLatestUnverifiedAsync(user.UserId, ct);
            if (latest is not null)
            {
                var nextAllowedAt = latest.CreatedAt.AddMinutes(1);
                if (DateTime.Now < nextAllowedAt)
                {
                    var remaining = (int)Math.Ceiling((nextAllowedAt - DateTime.Now).TotalSeconds);
                    return new AuthResponse
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
                ExpiresAt = DateTime.Now.AddMinutes(10),
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.EmailVerifications.AddAsync(verification, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            // Send email
            var subject = "GrowMate - Verify your email";
            var body = $@"
<!DOCTYPE html>
<html lang='vi'>
<head>
  <meta charset='UTF-8' />
  <title>GrowMate OTP</title>
</head>
<body style='margin:0;padding:0;background-color:#0b0f0d;'>
  <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='100%' style='background-color:#0b0f0d;'>
    <tr>
      <td align='center' style='padding:40px 0;'>
        <table role='presentation' cellspacing='0' cellpadding='0' border='0' width='480' style='background-color:#111814;border-radius:16px;padding:32px;text-align:center;box-shadow:0 0 20px rgba(34,197,94,0.15);'>
          <tr>
            <td style='font-family:Arial,sans-serif;color:#bbf7d0;font-size:22px;font-weight:bold;'>Xác thực đăng nhập</td>
          </tr>
          <tr><td style='padding:16px 0;font-family:Arial,sans-serif;color:#f3f3f3;font-size:15px;'>
            Xin chào {System.Net.WebUtility.HtmlEncode(user.FullName)},<br/>
            Mã OTP để xác thực tài khoản GrowMate của bạn là:
          </td></tr>
          <tr><td>
            <div style='
              display:inline-block;
              padding:16px 32px;
              font-size:28px;
              letter-spacing:4px;
              font-weight:bold;
              color:#16a34a;
              background-color:#022c22;
              border:2px solid rgba(134,239,172,0.3);
              border-radius:12px;
              '>{code}</div>
          </td></tr>
          <tr><td style='padding-top:24px;font-size:14px;color:#9ca3af;font-family:Arial,sans-serif;'>
            Mã này có hiệu lực trong 10 phút.
          </td></tr>
          <tr><td style='padding-top:24px;font-size:13px;color:#6b7280;font-family:Arial,sans-serif;'>
            © GrowMate Team. All rights reserved.
          </td></tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";

            await _emailService.SendEmailAsync(user.Email, subject, body);

            return new AuthResponse
            {
                Success = true,
                Message = "A new verification code has been sent to your email."
            };
        }
    }
}