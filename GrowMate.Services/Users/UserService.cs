using GrowMate.Contracts.Requests;
using GrowMate.Contracts.Responses;
using GrowMate.Repositories.Extensions;
using GrowMate.Repositories.Interfaces;
using GrowMate.Repositories.Models;
using GrowMate.Repositories.Models.Roles;
using GrowMate.Services.Authentication;
using GrowMate.Services.Customers;
using GrowMate.Services.Farmers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrowMate.Services.Users
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICustomerRepository _customerRepository;
        private readonly IUserRepository _userRepository;
        private readonly IFarmerRepository _farmerRepository;
        private readonly ILogger<UserService> _logger;
        private readonly ICustomerService _customerService;
        private readonly IFarmerService _farmerService;

        public UserService(IUnitOfWork unitOfWork, ICustomerRepository customerRepository, IUserRepository userRepository, IFarmerRepository farmerRepository, ILogger<UserService> logger, ICustomerService customerService, IFarmerService farmerService)
        {
            _unitOfWork = unitOfWork;
            _customerRepository = customerRepository;
            _userRepository = userRepository;
            _farmerRepository = farmerRepository;
            _logger = logger;
            _customerService = customerService;
            _farmerService = farmerService;
        }

        public async Task<AuthResponseDto> CreateUserByAdminAsync(CreateUserByAdminRequest request, CancellationToken ct = default)
        {
            var emailExist = await _unitOfWork.Users.GetByEmailAsync(request.Email, false, ct);
            if (emailExist != null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Email này đã sử dụng rồi"
                };
            }
            try
            {
                await _unitOfWork.ExecuteInTransactionAsync(async innerCt =>
                {
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
                    await _unitOfWork.Users.AddAsync(user, innerCt);
                    await _unitOfWork.SaveChangesAsync(innerCt);
                    if (user.Role.Equals(UserRoles.Customer) && request.CustomerRequest != null)
                    {
                        await _customerService.CreateByUserId(user.UserId, request.CustomerRequest, innerCt);
                    }
                    else if (user.Role.Equals(UserRoles.Farmer) && request.FarmerRequest != null)
                    {
                        await _farmerService.CreateByUserId(user.UserId, request.FarmerRequest, innerCt);
                    }
                    await _unitOfWork.SaveChangesAsync(innerCt);
                }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tạo user thất bại");
                return new AuthResponseDto { Success = false, Message = "Tạo mới user thất bại" };
            }
            return new AuthResponseDto
            {
                Success = true,
                Message = "Tạo mới user thành công!"
            };
        }

        public async Task<AuthResponseDto> DeleteUserAsync(int id, CancellationToken ct = default)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id, false, ct);
            if (user == null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Không tìm thấy userId: " + id
                };
            }
            user.IsActive = false;
            user.UpdatedAt = DateTime.Now;
            await _unitOfWork.SaveChangesAsync(ct);
            return new AuthResponseDto
            {
                Success = true,
                Message = "Xóa user: " + id + " thành công!"
            };
        }

        public Task<PageResult<User>> GetAllUserAsync(int page, int pageSize, CancellationToken ct = default)
            => _unitOfWork.Users.GetAllAsync(page, pageSize, ct);

        public async Task<UserDto> GetUserByEmailAsync(string email, bool includeCustomer, CancellationToken ct = default)
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(email, includeCustomer, ct);
            if (user == null) return null;
            if (user.Role.Equals(UserRoles.Customer))
            {
                user.Customer = await _unitOfWork.Customers.GetByUserIdAsync(user.UserId, ct);
            }
            else if (user.Role.Equals(UserRoles.Farmer))
            {
                user.Farmer = await _unitOfWork.Farmers.GetByIdAsync(user.UserId, ct);
            }
            var userResponse = new UserDto
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
                IsActive = user.IsActive ?? false,
                Customer = user.Role.Equals(UserRoles.Customer) && user.Customer != null ? new CustomerDto
                {
                    CustomerId = user.Customer.CustomerId,
                    ShippingAddress = user.Customer?.ShippingAddress,
                    WalletBalance = user.Customer?.WalletBalance,

                } : null,
                FarmerResponse = user.Role.Equals(UserRoles.Farmer) && user.Farmer != null ? new FarmerResponse
                {
                    FarmerId = user.Farmer.FarmerId,
                    FarmName = user.Farmer.FarmName,
                    FarmAddress = user.Farmer.FarmAddress,
                    ContactPhone = user.Farmer.ContactPhone,
                } : null
            };
            return userResponse;
        }

        public async Task<UserDto> GetUserByIdAsync(int id, bool includeCustomer, CancellationToken ct = default)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id, includeCustomer, ct);
            if (user == null) return null;
            if (user.Role.Equals(UserRoles.Customer))
            {
                user.Customer = await _unitOfWork.Customers.GetByUserIdAsync(user.UserId, ct);
            }
            else if (user.Role.Equals(UserRoles.Farmer))
            {
                user.Farmer = await _unitOfWork.Farmers.GetByUserIdAsync(user.UserId, ct);
            }
            var userResponse = new UserDto
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
                IsActive = user.IsActive ?? false,
                Customer = user.Role.Equals(UserRoles.Customer) && user.Customer != null ? new CustomerDto
                {
                    CustomerId = user.Customer.CustomerId,
                    ShippingAddress = user.Customer?.ShippingAddress,
                    WalletBalance = user.Customer?.WalletBalance,

                } : null,
                FarmerResponse = user.Role.Equals(UserRoles.Farmer) && user.Farmer != null ? new FarmerResponse
                {
                    FarmerId = user.Farmer.FarmerId,
                    FarmName = user.Farmer.FarmName,
                    FarmAddress = user.Farmer.FarmAddress,
                    ContactPhone = user.Farmer.ContactPhone,
                } : null
            };
            return userResponse;
        }

        public async Task<AuthResponseDto> UpdateUserAsync(int id, UpdateUserRequest request, CancellationToken ct = default)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id, false, ct);
            if (user == null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Không tìm thấy userId: " + id
                };
            }
            try
            {
                await _unitOfWork.ExecuteInTransactionAsync(async innerCt =>
                {
                    user.Email = request.Email;
                    user.FullName = request.FullName;
                    user.Phone = request.Phone;
                    user.UpdatedAt = DateTime.Now;

                    _unitOfWork.Users.Update(user);
                    await _unitOfWork.SaveChangesAsync(innerCt);

                    if (user.Role.Equals(UserRoles.Customer) && request.UpdateCustomer != null)
                    {
                        await _customerService.UpdateCustomerAsync(user.UserId, request.UpdateCustomer, innerCt);
                    }
                    else if (user.Role.Equals(UserRoles.Farmer) && request.UpdateFarmer != null)
                    {
                        await _farmerService.UpdateFarmerAsync(user.UserId, request.UpdateFarmer, innerCt);
                    }
                    await _unitOfWork.SaveChangesAsync(innerCt);
                }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cập nhật user thất bại");
                return new AuthResponseDto { Success = false, Message = "Cập nhật user thất bại" };
            }
            return new AuthResponseDto
            {
                Success = true,
                Message = "Cập nhật user thành công!"
            };
        }

        public async Task<AuthResponseDto> UpdateUserByAdminAsync(int id, UpdateUserByAdminRequest request, CancellationToken ct = default)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id, false, ct);
            if (user == null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Không tìm thấy userId: " + id
                };
            }
            try
            {
                await _unitOfWork.ExecuteInTransactionAsync(async innerCt =>
                {
                    user.Email = request.Email;
                    user.FullName = request.FullName;
                    user.Phone = request.Phone;
                    user.IsActive = request.IsActive ?? false;
                    user.UpdatedAt = DateTime.Now;

                    _unitOfWork.Users.Update(user);
                    await _unitOfWork.SaveChangesAsync(innerCt);
                    if (user.Role != request.Role)
                    {
                        //if role has changed
                        await HandleRoleChange(user, request.Role, request.UpdateCustomer, request.UpdateFarmer, innerCt);
                    }
                    else
                    {
                        if (request.Role.Equals(UserRoles.Customer) && request.UpdateCustomer != null)
                        {
                            await _customerService.UpdateCustomerAsync(user.UserId, request.UpdateCustomer, innerCt);
                        }
                        else if (request.Role.Equals(UserRoles.Farmer) && request.UpdateFarmer != null)
                        {
                            await _farmerService.UpdateFarmerAsync(user.UserId, request.UpdateFarmer, innerCt);
                        }
                    }
                    await _unitOfWork.SaveChangesAsync(innerCt);
                }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cập nhật user thất bại");
                return new AuthResponseDto { Success = false, Message = "Cập nhật user thất bại" };
            }
            return new AuthResponseDto
            {
                Success = true,
                Message = "Cập nhật user thành công!"
            };
        }

        public async Task<AuthResponseDto> UpdateUserPasswordAsync(int id, UpdateUserPwdRequest request, CancellationToken ct = default)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id, false ,ct);
            if (user == null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Không tìm thấy userId: " + id
                };
            }
            //Verify thì phải mật khẩu của DTO, mật khẩu dưới DB
            //Ngược lại sẽ sai
            bool checkPassword = BCrypt.Net.BCrypt.Verify(request.oldPassword, user.Password);
            if (!checkPassword)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Mật khẩu cũ không đúng "
                };
            }
            user.Password = BCrypt.Net.BCrypt.HashPassword(request.newPassword);
            user.UpdatedAt = DateTime.Now;
            await _unitOfWork.SaveChangesAsync(ct);
            return new AuthResponseDto
            {
                Success = true,
                Message = "Đổi mật khẩu thành công, mật khẩu mới: " + request.newPassword
            };
        }

        private async Task HandleRoleChange(User user, int newRole, CustomerRequest? customerRequest,
            FarmerRequest? farmerRequest, CancellationToken ct = default)
        {
            if (user.Role.Equals(UserRoles.Customer))
            {
                await _customerService.RemoveByUserIdAsync(user.UserId, ct);
            }
            else if (user.Role.Equals(UserRoles.Farmer))
            {
                await _farmerService.RemoveByUserIdAsync(user.UserId, ct);
            }

            //update Role
            user.Role = newRole;

            //Create new table after changed role
            if (newRole.Equals(UserRoles.Customer))
            {
                await _customerService.CreateByUserId(user.UserId, customerRequest, ct);
            }
            else if (newRole.Equals(UserRoles.Farmer))
            {
                await _farmerService.CreateByUserId(user.UserId, farmerRequest, ct);
            }
        }
    }
}
