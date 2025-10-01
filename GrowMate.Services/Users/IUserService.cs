using GrowMate.Contracts.Requests;
using GrowMate.Contracts.Responses;
using GrowMate.Models;
using GrowMate.Repositories.Extensions;
using GrowMate.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrowMate.Services.Users
{
    public interface IUserService
    {
        Task<PageResult<User>> GetAllUserAsync(int page, int pageSize, CancellationToken ct = default);

        Task<UserDto> GetUserByIdAsync(int id, bool includeCustomer, CancellationToken ct = default);

        Task<UserDto> GetUserByEmailAsync(string email, bool includeCustomer, CancellationToken ct = default);

        Task<AuthResponseDto> CreateUserByAdminAsync(CreateUserByAdminRequest request, CancellationToken ct = default);
        Task<AuthResponseDto> UpdateUserAsync(int id, UpdateUserRequest request, CancellationToken ct = default);
        Task<AuthResponseDto> UpdateUserByAdminAsync(int id, UpdateUserByAdminRequest request, CancellationToken ct = default);
        Task<AuthResponseDto> DeleteUserAsync(int id, CancellationToken ct = default);
        Task<AuthResponseDto> UpdateUserPasswordAsync(int id, UpdateUserPwdRequest request, CancellationToken ct = default);
    }
}
