using GrowMate.Contracts.Requests;
using GrowMate.Contracts.Responses;
using GrowMate.Repositories.Interfaces;
using GrowMate.Repositories.Models;
using GrowMate.Repositories.Models.Roles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Security.Principal;

namespace GrowMate.Services.Authentication
{
    public class LoginService : ILoginService
    {
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<LoginService> _logger;
        private readonly IHostEnvironment _env;
        private readonly ICustomerRepository _customerRepository;

        public LoginService(
            ITokenService tokenService,
            IUnitOfWork unitOfWork,
            ILogger<LoginService> logger,
            IHostEnvironment env,
            ICustomerRepository customerRepository)
        {
            _tokenService = tokenService;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _env = env;
            _customerRepository = customerRepository;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken ct = default)
        {
            // Field validation can be explicit; not a user-enumeration vector
            if (string.IsNullOrWhiteSpace(request.Email) && string.IsNullOrWhiteSpace(request.Password))
                return Fail("Email and password are required.", "Both email and password are missing.", "AUTH_MISSING_FIELDS");

            if (string.IsNullOrWhiteSpace(request.Email))
                return Fail("Email is required.", "Email is missing.", "AUTH_MISSING_EMAIL");

            if (string.IsNullOrWhiteSpace(request.Password))
                return Fail("Password is required.", "Password is missing.", "AUTH_MISSING_PASSWORD");

            var email = request.Email.Trim().ToLowerInvariant();

            var user = await _unitOfWork.Users.GetByEmailAsync(email, includeCustomer: true, ct);
            if (user is null)
            {
                // Generic message in prod, detailed in dev
                return Fail("Invalid email or password.", $"No account found for email '{email}'.", "AUTH_INVALID_CREDENTIALS");
            }

            if (user.IsActive != true)
            {
                // Avoid revealing account existence in prod
                return Fail("Invalid email or password.", $"Email '{email}' exists but is not verified.", "AUTH_INVALID_CREDENTIALS");
            }

            var valid = BCrypt.Net.BCrypt.Verify(request.Password, user.Password);
            if (!valid)
            {
                return Fail("Invalid email or password.", $"Wrong password for email '{email}'.", "AUTH_INVALID_CREDENTIALS");
            }

            var (token, expiresAt) = _tokenService.GenerateToken(user);

            var userDto = new UserDto
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                Phone = user.Phone,
                Role = user.Role,
                RoleName = UserRoles.ToName(user.Role),
                ProfileImageUrl = user.ProfileImageUrl,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                IsActive = user.IsActive ?? false
            };

            var customerDto = user.Customer is null ? null : new CustomerDto
            {
                CustomerId = user.Customer.CustomerId,
                ShippingAddress = user.Customer.ShippingAddress,
                WalletBalance = user.Customer.WalletBalance,
            };

            return new LoginResponseDto
            {
                Success = true,
                Message = "Login successful.",
                Token = token,
                ExpiresAt = expiresAt,
                User = userDto,
                Customer = customerDto
            };
        }

        public async Task<LoginResponseDto> LoginWithGoogle(string email, string name, CancellationToken ct = default)
        {
            var account = await _unitOfWork.Users.GetByEmailAsync(email, includeCustomer: false, ct);
            CustomerDto customerResponse = null;
            FarmerResponse farmerResponse = null;
            try
            {
                await _unitOfWork.ExecuteInTransactionAsync(async innerCt =>
                {
                    if (account == null)
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
                        await _unitOfWork.Users.AddAsync(newAccount, innerCt);
                        await _unitOfWork.SaveChangesAsync(innerCt); // ensure UserId is generated
                                                                     // Ensure a corresponding Customer row exists (shared PK with UserId)
                        var hasCustomer = await _unitOfWork.Customers.AnyAsync(newAccount.UserId, innerCt);
                        if (!hasCustomer)
                        {
                            var customer = new Customer
                            {
                                CustomerId = newAccount.UserId,
                                CreatedAt = DateTime.Now
                            };
                            await _unitOfWork.Customers.CreateAsync(customer, innerCt);
                            await _unitOfWork.SaveChangesAsync(innerCt);
                        }


                        account = newAccount;

                    }
                }, ct);
                var (token, expiresAt) = _tokenService.GenerateToken(account);
                if (account.Role.Equals(UserRoles.Customer))
                {
                    var customer = await _unitOfWork.Customers.GetByUserIdAsync(account.UserId);
                    if (customer != null)
                    {
                        customerResponse = new CustomerDto
                        {
                            CustomerId = customer.CustomerId,
                            ShippingAddress = customer.ShippingAddress,
                            WalletBalance = customer.WalletBalance,
                        };
                    }

                }
                else if (account.Role.Equals(UserRoles.Farmer))
                {
                    var farmer = await _unitOfWork.Farmers.GetByIdAsync(account.UserId);
                    if (farmer != null)
                    {
                        farmerResponse = new FarmerResponse
                        {
                            FarmName = farmer.FarmName,
                            FarmAddress = farmer.FarmAddress,
                            ContactPhone = farmer.ContactPhone,
                        };
                    }
                }
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
                    Customer = customerResponse,
                    FarmerResponse = farmerResponse
                };
            }
            catch
            (Exception ex)
            {
                _logger.LogError(ex, "Log in with Google thất bại");
                return Fail("Log in with Google thất bại", " Exception: " + ex, "AUTH_INVALID_CREDENTIALS");
            }
        }

        private LoginResponseDto Fail(string publicMessage, string detailedMessage, string errorCode)
        {
            var correlationId = Guid.NewGuid().ToString("N");

            // Structured log with correlationId and detailed reason
            _logger.LogInformation("Login failed [{CorrelationId}] {Reason}", correlationId, detailedMessage);

            var message = _env.IsDevelopment() ? detailedMessage : publicMessage;

            return new LoginResponseDto
            {
                Success = false,
                Message = message,
                ErrorCode = errorCode,
                CorrelationId = correlationId
            };
        }
    }
}
