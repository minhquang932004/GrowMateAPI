using GrowMate.DTOs.Extensions;
using GrowMate.DTOs.Requests;
using GrowMate.DTOs.Responses;
using GrowMate.Models;

namespace GrowMate.Services.UserAccount
{
    public interface IUserAccountService
    {
        Task<PageResult<User>> GetAllUserAsync(int page, int pageSize);

        Task<UserDto> GetUserByIdAsync(int id);

        Task<UserDto> GetUserByEmailAsync(string email);

        Task<AuthResponseDto> CreateUserByAdminAsync(CreateUserByAdminRequest request);
        Task<AuthResponseDto> UpdateUserAsync(int id, UpdateUserRequest request);
        Task<AuthResponseDto> UpdateUserByAdminAsync(int id, UpdateUserByAdminRequest request);
        Task<AuthResponseDto> DeleteUserAsync(int id);
        Task<AuthResponseDto> UpdateUserPasswordAsync(int id, UpdateUserPwdRequest request);
        Task<UserDto> GetUserByPhoneAsync(string phone);
    }
}
