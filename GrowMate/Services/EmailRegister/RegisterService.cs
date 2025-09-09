using GrowMate.Data;
using GrowMate.DTOs.Requests;
using GrowMate.DTOs.Responses;
using GrowMate.Models;
using GrowMate.Models.Roles;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace GrowMate.Services.EmailRegister
{
    public class RegisterService : IRegisterService
    {
        private readonly EXE201_GrowMateContext _dbContext;
        private readonly IEmailService _emailService;

        public RegisterService(EXE201_GrowMateContext dbContext, IEmailService emailService)
        {
            _dbContext = dbContext;
            _emailService = emailService;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            var email = request.Email.Trim().ToLowerInvariant();

            // Load existing user if any
            var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Email == email);

            if (user is not null && user.IsActive == true)
                return new AuthResponseDto { Success = false, Message = "Email already registered." };

            if (user is null)
            {
                user = new User
                {
                    FullName = request.FullName.Trim(),
                    Email = email,
                    Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    Role = UserRoles.Guest, // assign Guest role because not verified yet
                    IsActive = false
                };

                _dbContext.Users.Add(user);
                try
                {
                    await _dbContext.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    // Handle race where another request created the same email
                    var exists = await _dbContext.Users.AsNoTracking().AnyAsync(u => u.Email == email);
                    if (exists) return new AuthResponseDto { Success = false, Message = "Email already registered." };
                    throw;
                }
            }
            else
            {
                // If user exists but inactive, allow re-register to refresh name/password if desired
                user.FullName = request.FullName.Trim();
                user.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
                // Keep role as Guest until email is verified
                await _dbContext.SaveChangesAsync();
            }

            // Remove any previous unverified codes for this user
            await _dbContext.EmailVerifications
                .Where(v => v.UserId == user.UserId && v.VerifiedAt == null)
                .ExecuteDeleteAsync();

            // Generate a cryptographically strong 6-digit code
            var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

            var verification = new EmailVerification
            {
                UserId = user.UserId,
                CodeHash = BCrypt.Net.BCrypt.HashPassword(code),
                ExpiresAt = DateTime.Now.AddMinutes(10)
            };

            _dbContext.EmailVerifications.Add(verification);
            await _dbContext.SaveChangesAsync();

            await _emailService.SendEmailAsync(
                user.Email,
                "Verify your email",
                $"Your verification code is: <b style=\"letter-spacing:3px\">{code}</b><br/>This code expires in 10 minutes."
            );

            return new AuthResponseDto { Success = true, Message = "Registration successful. Check your email for the code." };
        }

        public async Task<AuthResponseDto> VerifyEmailAsync(VerifyEmailRequestDto request)
        {
            var email = request.Email.Trim().ToLowerInvariant();

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return new AuthResponseDto { Success = false, Message = "User not found." };
            if (user.IsActive == true) return new AuthResponseDto { Success = true, Message = "Email already verified." };

            var verification = await _dbContext.EmailVerifications
                .Where(v => v.UserId == user.UserId && v.VerifiedAt == null)
                .OrderByDescending(v => v.CreatedAt)
                .FirstOrDefaultAsync();

            if (verification == null)
                return new AuthResponseDto { Success = false, Message = "No verification code found. Please request a new one." };

            if (verification.ExpiresAt < DateTime.Now)
                return new AuthResponseDto { Success = false, Message = "Code expired. Please request a new code." };

            if (!BCrypt.Net.BCrypt.Verify(request.Code, verification.CodeHash))
                return new AuthResponseDto { Success = false, Message = "Invalid code." };

            verification.VerifiedAt = DateTime.Now;
            user.IsActive = true;

            // Promote from Guest to Customer upon successful verification
            if (user.Role == UserRoles.Guest)
                user.Role = UserRoles.Customer;

            // Ensure a corresponding Customer row exists (shared PK with UserId)
            var hasCustomer = await _dbContext.Customers.AnyAsync(c => c.CustomerId == user.UserId);
            if (!hasCustomer)
            {
                var customer = new Customer
                {
                    CustomerId = user.UserId,
                    CreatedAt = DateTime.Now
                };
                _dbContext.Customers.Add(customer);
            }

            await _dbContext.SaveChangesAsync();

            return new AuthResponseDto { Success = true, Message = "Email verified successfully." };
        }
    }
}
