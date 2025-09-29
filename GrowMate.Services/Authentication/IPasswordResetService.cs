using GrowMate.Contracts.Requests;
using GrowMate.Contracts.Responses;

namespace GrowMate.Services.Authentication
{
    public interface IPasswordResetService
    {
        Task<AuthResponseDto> RequestResetAsync(ForgotPasswordRequestDto request, CancellationToken ct = default);
        Task<AuthResponseDto> ResetPasswordAsync(ResetPasswordRequestDto request, CancellationToken ct = default);
    }
}