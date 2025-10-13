using GrowMate.Contracts.Requests.User;
using GrowMate.Contracts.Responses.Auth;
using GrowMate.Contracts.Responses.User;
using GrowMate.Models;
using GrowMate.Repositories.Extensions;


namespace GrowMate.Services.Users
{
    public interface IUserService
    {
        Task<PageResult<User>> GetAllUserAsync(int page, int pageSize, CancellationToken ct = default);

        Task<UserResponse> GetUserByIdAsync(int id, bool includeCustomer, CancellationToken ct = default);

        Task<UserResponse> GetUserByEmailAsync(string email, bool includeCustomer, CancellationToken ct = default);

        Task<AuthResponse> CreateUserByAdminAsync(CreateUserByAdminRequest request, CancellationToken ct = default);
        Task<AuthResponse> UpdateUserAsync(int id, UpdateUserRequest request, CancellationToken ct = default);
        Task<AuthResponse> UpdateUserByAdminAsync(int id, UpdateUserByAdminRequest request, CancellationToken ct = default);
        Task<AuthResponse> DeleteUserAsync(int id, CancellationToken ct = default);
        Task<AuthResponse> UpdateUserPasswordAsync(int id, UpdateUserPasswordRequest request, CancellationToken ct = default);
    }
}
