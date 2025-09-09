using BCrypt.Net;
using GrowMate.Data;
using GrowMate.DTOs.Responses;
using GrowMate.Models;
using GrowMate.Models.Roles;
using Microsoft.EntityFrameworkCore;
using System.Security.Principal;

namespace GrowMate.Services.Auth
{
    public class LoginService : ILoginService
    {
        private readonly EXE201_GrowMateContext _dbContext;
        private readonly ITokenService _tokenService;

        public LoginService(EXE201_GrowMateContext dbContext, ITokenService tokenService)
        {
            _dbContext = dbContext;
            _tokenService = tokenService;
        }

        public async Task<LoginResponseDto> LoginWithGoogle(string email, string name)
        {
            var account = await _dbContext.Users.FirstOrDefaultAsync(a => a.Email.Equals(email));
            string accessToken = string.Empty;
            if(account == null)
            {
                var newAccount = new User
                {
                    Email = email,
                    FullName = name,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Password = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()),
                    Role = UserRoles.Customer
                };
                _dbContext.Users.Add(newAccount);
                await _dbContext.SaveChangesAsync();
                // Ensure a corresponding Customer row exists (shared PK with UserId)
                var hasCustomer = await _dbContext.Customers.AnyAsync(c => c.CustomerId == newAccount.UserId);
                if (!hasCustomer)
                {
                    var customer = new Customer
                    {
                        CustomerId = newAccount.UserId,
                        CreatedAt = DateTime.Now
                    };
                    _dbContext.Customers.Add(customer);
                }

                await _dbContext.SaveChangesAsync();
                var (token, expiresAt) = _tokenService.GenerateToken(newAccount);
                var customerDto = newAccount.Customer is null ? null : new CustomerDto
                {
                    CustomerId = newAccount.Customer.CustomerId,
                    ShippingAddress = newAccount.Customer.ShippingAddress,
                    WalletBalance = newAccount.Customer.WalletBalance,
                };
                return new LoginResponseDto
                {
                    Success = true,
                    Message = "Đăng nhập bằng Google thành công",
                    Token = token,
                    ExpiresAt = expiresAt,
                    User = new UserDto
                    {
                        Email = newAccount.Email,
                        FullName = newAccount.FullName,
                        IsActive = newAccount.IsActive ?? false
                    },
                    Customer = customerDto
                };
            }
            else
            {
                //if existed email
                var (token, expiresAt) = _tokenService.GenerateToken(account);
                var customerDto = account.Customer is null ? null : new CustomerDto
                {
                    CustomerId = account.Customer.CustomerId,
                    ShippingAddress = account.Customer.ShippingAddress,
                    WalletBalance = account.Customer.WalletBalance,
                };
                return new LoginResponseDto
                {
                    Success = true,
                    Message = "Đăng nhập bằng Google thành công",
                    Token = token,
                    ExpiresAt = expiresAt,
                    User = new UserDto
                    {
                        Email = account.Email,
                        FullName = account.FullName,
                        IsActive = account.IsActive ?? false
                    },
                    Customer = customerDto
                };
            }
        }
    }
}
