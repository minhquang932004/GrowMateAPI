using GrowMate.Contracts.Requests.Auth;
using GrowMate.Contracts.Responses.Auth;

namespace GrowMate.Services.EmailRegister
{
    public interface IRegisterService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
        Task<AuthResponse> VerifyEmailAsync(VerifyEmailRequest request, CancellationToken ct = default);
        Task<AuthResponse> ResendVerificationCodeAsync(ResendVerificationRequest request, CancellationToken ct = default);
    }
}