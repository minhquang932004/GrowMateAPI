using BCrypt.Net;
using GrowMate.Data;
using GrowMate.DTOs.Extensions;
using GrowMate.DTOs.Requests;
using GrowMate.DTOs.Responses;
using GrowMate.Models;
using GrowMate.Models.Roles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrowMate.Services.UserAccount
{
    public class UserAccountService : IUserAccountService
    {
        private readonly EXE201_GrowMateContext _context;
        private readonly ILogger<UserAccountService> _logger;
        public UserAccountService(EXE201_GrowMateContext context, ILogger<UserAccountService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<AuthResponseDto> CreateUserByAdminAsync(CreateUserByAdminRequest request)
        {
            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                if (await _context.Users.AnyAsync(e => e.Email.Equals(request.Email)))
                {
                    return new AuthResponseDto { Success = false, Message = "Email này đã tồn tại" };
                }

                var user = new User
                {
                    Email = request.Email.Trim().ToLowerInvariant(),
                    Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    FullName = request.FullName,
                    Phone = request.Phone,
                    Role = request.Role,
                    ProfileImageUrl = request.ProfileImageUrl,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                };
                _context.Add(user);
                await _context.SaveChangesAsync();
                if (request.Role.Equals(UserRoles.Customer))
                {
                    _context.Customers.Add(new Customer
                    {
                        CustomerId = user.UserId,
                        ShippingAddress = request.CustomerRequest?.ShippingAddress,
                        WalletBalance = request.CustomerRequest?.WalletBalance,
                        CreatedAt = DateTime.Now
                    });
                }
                else if (request.Role.Equals(UserRoles.Farmer))
                {
                    _context.Farmers.Add(new Farmer
                    {
                        FarmerId = user.UserId,
                        FarmName = request.FarmerRequest?.FarmName.Trim(),
                        FarmAddress = request.FarmerRequest?.FarmAddress.Trim(),
                        ContactPhone = request.FarmerRequest?.ContactPhone.Trim(),
                        VerificationStatus = request.FarmerRequest?.VerificationStatus,
                        CreatedAt = DateTime.Now,
                    });
                }
                await _context.SaveChangesAsync();
                await tx.CommitAsync();
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex, "Tạo User thất bại");
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Tạo mới user thất bại"
                };
            }
            return new AuthResponseDto
            {
                Success = true,
                Message = "Tạo mới user thành công!"
            };
        }

        public Task<bool> DeleteUserAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<PageResult<User>> GetAllUserAsync(int page, int pageSize)
        {
            return await _context.Users.AsNoTracking().OrderByDescending(a => a.CreatedAt).ToPagedResultAsync(page, pageSize);
        }

        public async Task<UserDto> GetUserByIdAsync(int id)
        {
            var user = await _context.Users.AsNoTracking()
                .Where(a => a.UserId == id)
                .Select(a => new UserDto
                {
                    UserId = a.UserId,
                    Email = a.Email,
                    FullName = a.FullName,
                    Phone = a.Phone,
                    Role = a.Role,
                    RoleName = UserRoles.ToName(a.Role),
                    ProfileImageUrl = a.ProfileImageUrl,
                    CreateAt = a.CreatedAt,
                    UpdateAt = a.UpdatedAt,
                    IsActive = a.IsActive ?? false,
                })
                .FirstOrDefaultAsync();
            if (user == null)
            {
                return null;
            }
            if (user.Role.Equals(UserRoles.Customer))
            {
                user.Customer = await _context.Customers.AsNoTracking()
                    .Where(c => c.CustomerId == user.UserId)
                    .Select(c => new CustomerDto
                    {
                        CustomerId = c.CustomerId,
                        ShippingAddress = c.ShippingAddress,
                        WalletBalance = c.WalletBalance,
                    })
                    .FirstOrDefaultAsync();
            }
            else if (user.Role.Equals(UserRoles.Farmer))
            {
                user.FarmerResponse = await _context.Farmers.AsNoTracking()
                    .Where(f => f.FarmerId == user.UserId)
                    .Select(f => new FarmerResponse
                    {
                        FarmerId = f.FarmerId,
                        ContactPhone = f.ContactPhone,
                        FarmAddress = f.FarmAddress,
                        FarmName = f.FarmName,
                    })
                    .FirstOrDefaultAsync();
            }
            return user;
        }

        public async Task<UserDto> GetUserByEmailAsync(string email)
        {
            var emailItem = email.Trim().ToLowerInvariant();
            var key = await _context.Users.AsNoTracking()
                .Where(a => a.Email.Equals(emailItem))
                .Select(a => new
                {
                    a.UserId,
                })
                .FirstOrDefaultAsync();
            return key == null ? null : await GetUserByIdAsync(key.UserId);
        }

        public async Task<UserDto> GetUserByPhoneAsync(string phone)
        {
            var key = await _context.Users.AsNoTracking()
                .Where(a => a.Phone.Equals(phone))
                .Select(a => new
                {
                    a.UserId,
                })
                .FirstOrDefaultAsync();
            return key == null ? null : await GetUserByIdAsync(key.UserId);
        }

        public Task<int> UpdateUserAsync(User user)
        {
            throw new NotImplementedException();
        }

    }
}
