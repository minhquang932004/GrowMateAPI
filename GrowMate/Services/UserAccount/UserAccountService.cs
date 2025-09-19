//using BCrypt.Net;
//using GrowMate.Data;
//using GrowMate.DTOs.Extensions;
//using GrowMate.DTOs.Requests;
//using GrowMate.DTOs.Responses;
//using GrowMate.Models;
//using GrowMate.Models.Roles;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Logging;

//namespace GrowMate.Services.UserAccount
//{
//    public class UserAccountService : IUserAccountService
//    {
//        private readonly EXE201_GrowMateContext _context;
//        private readonly ILogger<UserAccountService> _logger;
//        public UserAccountService(EXE201_GrowMateContext context, ILogger<UserAccountService> logger)
//        {
//            _context = context;
//            _logger = logger;
//        }

//        public async Task<AuthResponseDto> CreateUserByAdminAsync(CreateUserByAdminRequest request)
//        {
//            await using var tx = await _context.Database.BeginTransactionAsync();
//            try
//            {
//                if (await _context.Users.AnyAsync(e => e.Email.Equals(request.Email)))
//                {
//                    return new AuthResponseDto { Success = false, Message = "Email này đã tồn tại" };
//                }

//                var user = new User
//                {
//                    Email = request.Email.Trim().ToLowerInvariant(),
//                    Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
//                    FullName = request.FullName,
//                    Phone = request.Phone,
//                    Role = request.Role,
//                    ProfileImageUrl = request.ProfileImageUrl,
//                    IsActive = true,
//                    CreatedAt = DateTime.Now,
//                    UpdatedAt = DateTime.Now,
//                };
//                _context.Add(user);
//                await _context.SaveChangesAsync();
//                if (request.Role.Equals(UserRoles.Customer))
//                {
//                    _context.Customers.Add(new Customer
//                    {
//                        CustomerId = user.UserId,
//                        ShippingAddress = request.CustomerRequest?.ShippingAddress,
//                        WalletBalance = request.CustomerRequest?.WalletBalance,
//                        CreatedAt = DateTime.Now
//                    });
//                }
//                else if (request.Role.Equals(UserRoles.Farmer))
//                {
//                    _context.Farmers.Add(new Farmer
//                    {
//                        FarmerId = user.UserId,
//                        FarmName = request.FarmerRequest?.FarmName.Trim(),
//                        FarmAddress = request.FarmerRequest?.FarmAddress.Trim(),
//                        ContactPhone = request.FarmerRequest?.ContactPhone.Trim(),
//                        VerificationStatus = request.FarmerRequest?.VerificationStatus ?? "Pending",
//                        CreatedAt = DateTime.Now,
//                    });
//                }
//                await _context.SaveChangesAsync();
//                await tx.CommitAsync();
//            }
//            catch (Exception ex)
//            {
//                await tx.RollbackAsync();
//                _logger.LogError(ex, "Tạo User thất bại");
//                return new AuthResponseDto
//                {
//                    Success = false,
//                    Message = "Tạo mới user thất bại"
//                };
//            }
//            return new AuthResponseDto
//            {
//                Success = true,
//                Message = "Tạo mới user thành công!"
//            };
//        }

//        public async Task<AuthResponseDto> DeleteUserAsync(int id)
//        {
//            var user = await _context.Users.FirstOrDefaultAsync(a => a.UserId == id);
//            if (user == null)
//            {
//                return new AuthResponseDto
//                {
//                    Success = false,
//                    Message = "Không tìm thấy userId: " + id
//                };
//            }
//            user.IsActive = false;
//            user.UpdatedAt = DateTime.Now;
//            await _context.SaveChangesAsync();
//            return new AuthResponseDto
//            {
//                Success = true,
//                Message = "Xóa user: " + id + " thành công!"
//            };
//        }

//        public async Task<PageResult<User>> GetAllUserAsync(int page, int pageSize)
//        {
//            return await _context.Users.AsNoTracking().OrderByDescending(a => a.CreatedAt).ToPagedResultAsync(page, pageSize);
//        }

//        public async Task<UserDto> GetUserByIdAsync(int id)
//        {
//            var user = await _context.Users.AsNoTracking()
//                .Where(a => a.UserId == id)
//                .Select(a => new UserDto
//                {
//                    UserId = a.UserId,
//                    Email = a.Email,
//                    FullName = a.FullName,
//                    Phone = a.Phone,
//                    Role = a.Role,
//                    RoleName = UserRoles.ToName(a.Role),
//                    ProfileImageUrl = a.ProfileImageUrl,
//                    CreateAt = a.CreatedAt,
//                    UpdateAt = a.UpdatedAt,
//                    IsActive = a.IsActive ?? false,
//                })
//                .FirstOrDefaultAsync();
//            if (user == null)
//            {
//                return null;
//            }
//            if (user.Role.Equals(UserRoles.Customer))
//            {
//                user.Customer = await _context.Customers.AsNoTracking()
//                    .Where(c => c.CustomerId == user.UserId)
//                    .Select(c => new CustomerDto
//                    {
//                        CustomerId = c.CustomerId,
//                        ShippingAddress = c.ShippingAddress,
//                        WalletBalance = c.WalletBalance,
//                    })
//                    .FirstOrDefaultAsync();
//            }
//            else if (user.Role.Equals(UserRoles.Farmer))
//            {
//                user.FarmerResponse = await _context.Farmers.AsNoTracking()
//                    .Where(f => f.FarmerId == user.UserId)
//                    .Select(f => new FarmerResponse
//                    {
//                        FarmerId = f.FarmerId,
//                        ContactPhone = f.ContactPhone,
//                        FarmAddress = f.FarmAddress,
//                        FarmName = f.FarmName,
//                    })
//                    .FirstOrDefaultAsync();
//            }
//            return user;
//        }

//        public async Task<UserDto> GetUserByEmailAsync(string email)
//        {
//            var emailItem = email.Trim().ToLowerInvariant();
//            var key = await _context.Users.AsNoTracking()
//                .Where(a => a.Email.Equals(emailItem))
//                .Select(a => new
//                {
//                    a.UserId,
//                })
//                .FirstOrDefaultAsync();
//            return key == null ? null : await GetUserByIdAsync(key.UserId);
//        }

//        public async Task<UserDto> GetUserByPhoneAsync(string phone)
//        {
//            var key = await _context.Users.AsNoTracking()
//                .Where(a => a.Phone.Equals(phone))
//                .Select(a => new
//                {
//                    a.UserId,
//                })
//                .FirstOrDefaultAsync();
//            return key == null ? null : await GetUserByIdAsync(key.UserId);
//        }

//        public async Task<AuthResponseDto> UpdateUserAsync(int id, UpdateUserRequest request)
//        {
//            await using var tx = await _context.Database.BeginTransactionAsync();
//            var user = await _context.Users.FirstOrDefaultAsync(a => a.UserId == id);
//            if (user == null)
//            {
//                return new AuthResponseDto
//                {
//                    Success = false,
//                    Message = "Không tìm thấy userId: " + id
//                };
//            }
//            user.Email = request.Email;
//            user.FullName = request.FullName;
//            user.Phone = request.Phone;
//            user.UpdatedAt = DateTime.Now;
//            if (user.Role.Equals(UserRoles.Customer) && request.UpdateCustomer != null)
//            {
//                await UpdateCustomerAsync(user.UserId, request.UpdateCustomer);
//            }
//            else if (user.Role.Equals(UserRoles.Farmer) && request.UpdateFarmer != null)
//            {
//                await UpdateFarmerAsync(user.UserId, request.UpdateFarmer);
//            }
//            await _context.SaveChangesAsync();
//            await tx.CommitAsync();
//            return new AuthResponseDto
//            {
//                Success = true,
//                Message = "Update thành công userId: " + id
//            };
//        }

//        public async Task<AuthResponseDto> UpdateUserByAdminAsync(int id, UpdateUserByAdminRequest request)
//        {
//            await using var tx = await _context.Database.BeginTransactionAsync();
//            var user = await _context.Users.FirstOrDefaultAsync(a => a.UserId == id);
//            if (user == null)
//            {
//                return new AuthResponseDto
//                {
//                    Success = false,
//                    Message = "Không tìm thấy userId: " + id
//                };
//            }
//            user.Email = request.Email;
//            user.FullName = request.FullName;
//            user.Phone = request.Phone;
//            user.IsActive = request.IsActive ?? false;
//            user.UpdatedAt = DateTime.Now;
//            if(request.Role != user.Role)
//            {
//                //if role have changed
//                await HandleRoleChangeAsync(user, request.Role, request.UpdateCustomer, request.UpdateFarmer);
//            }
//            else
//            {
//                //if role have not changed, continue update Customer or Farmer table
//                if (user.Role.Equals(UserRoles.Customer) && request.UpdateCustomer != null)
//                {
//                    await UpdateCustomerAsync(id, request.UpdateCustomer);
//                } else if(user.Role.Equals(UserRoles.Farmer) && request.UpdateFarmer != null)
//                {
//                    await UpdateFarmerAsync(id,request.UpdateFarmer);
//                }
//            }
//            await _context.SaveChangesAsync();
//            await tx.CommitAsync();
//            return new AuthResponseDto
//            {
//                Success = true,
//                Message = "Cập nhật user: " + id + " thành công"
//            };
//        }


//        private async Task UpdateCustomerAsync(int id, UpdateCustomerRequest request)
//        {
//            var customer = await _context.Customers.FirstOrDefaultAsync(a => a.CustomerId == id);
//            if (customer != null)
//            {
//                customer.WalletBalance = request.WalletBalance;
//                customer.ShippingAddress = request.ShippingAddress;
//            }
//        }
//        private async Task UpdateFarmerAsync(int id, UpdateFarmerRequest request)
//        {
//            var farmer = await _context.Farmers.FirstOrDefaultAsync(a => a.FarmerId == id);
//            if (farmer != null)
//            {
//                farmer.FarmName = request.FarmName;
//                farmer.FarmAddress = request.FarmAddress;
//                farmer.ContactPhone = request.ContactPhone;
//                farmer.VerificationStatus = request.VerificationStatus;
//            }
//        }

//        //If User's role have been changed, remove old table and create a new one
//        private async Task HandleRoleChangeAsync(User user, int newRole, UpdateCustomerRequest? updateCustomerRequest, UpdateFarmerRequest? updateFarmerRequest)
//        {
//            if (user.Role.Equals(UserRoles.Customer))
//            {
//                var oldCustomer = await _context.Customers.FirstOrDefaultAsync(a => a.CustomerId == user.UserId);
//                if (oldCustomer != null)
//                {
//                    _context.Customers.Remove(oldCustomer);
//                }
//            } else if (user.Role.Equals(UserRoles.Farmer))
//            {
//                var oldFarmer = await _context.Farmers.FirstOrDefaultAsync(a => a.FarmerId == user.UserId);
//                if(oldFarmer != null)
//                {
//                    _context.Farmers.Remove(oldFarmer);
//                }
//            }
//            user.Role = newRole;
//            if (newRole.Equals(UserRoles.Customer))
//            {
//                var newCustomer = new Customer
//                {
//                    CustomerId = user.UserId,
//                    ShippingAddress = updateCustomerRequest?.ShippingAddress,
//                    WalletBalance = updateCustomerRequest?.WalletBalance ?? 0,
//                    CreatedAt = DateTime.Now,
//                };
//                await _context.Customers.AddAsync(newCustomer);
//            } else if (newRole.Equals(UserRoles.Farmer))
//            {
//                var newFarmer = new Farmer
//                {
//                    FarmerId = user.UserId,
//                    FarmAddress = updateFarmerRequest?.FarmAddress,
//                    FarmName = updateFarmerRequest?.FarmName,
//                    ContactPhone = updateFarmerRequest?.ContactPhone,
//                    VerificationStatus = updateFarmerRequest?.VerificationStatus ?? "Pending",
//                    CreatedAt = DateTime.Now,
//                };
//                await _context.Farmers.AddAsync(newFarmer);
//            }
//        }

//        public async Task<AuthResponseDto> UpdateUserPasswordAsync(int id, UpdateUserPwdRequest request)
//        {
//            var user = await _context.Users.FirstOrDefaultAsync(a => a.UserId == id);
//            if(user == null)
//            {
//                return new AuthResponseDto
//                {
//                    Success = false,
//                    Message = "Không tìm thấy userId: " + id
//                };
//            }
//            //Verify thì phải mật khẩu của DTO, mật khẩu dưới DB
//            //Ngược lại sẽ sai
//            bool checkPassword = BCrypt.Net.BCrypt.Verify(request.oldPassword, user.Password);
//            if (!checkPassword)
//            {
//                return new AuthResponseDto
//                {
//                    Success = false,
//                    Message = "Mật khẩu cũ không đúng "
//                };
//            }
//            user.Password = BCrypt.Net.BCrypt.HashPassword(request.newPassword);
//            user.UpdatedAt = DateTime.Now;
//            await _context.SaveChangesAsync();
//            return new AuthResponseDto
//            {
//                Success = true,
//                Message = "Đổi mật khẩu thành công, mật khẩu mới: " + request.newPassword
//            };
//        }
//    }
//}
