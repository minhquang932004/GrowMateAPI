using GrowMate.Contracts.Requests.Auth;
using GrowMate.Contracts.Responses.Auth;

namespace GrowMate.Services.Authentication
{
    public interface ILoginService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);

        Task<LoginResponse> LoginWithGoogle(string email, string name, CancellationToken ct = default);
    }
}
