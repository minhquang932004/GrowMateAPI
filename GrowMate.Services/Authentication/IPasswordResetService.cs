using GrowMate.Contracts.Requests.Auth;
using GrowMate.Contracts.Responses.Auth;

namespace GrowMate.Services.Authentication
{
    public interface IPasswordResetService
    {
        Task<AuthResponse> RequestResetAsync(ForgotPasswordRequest request, CancellationToken ct = default);
        Task<AuthResponse> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default);
    }
}