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
        Task<int> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int id);

        Task<UserDto> GetUserByPhoneAsync(string phone);
    }
}
