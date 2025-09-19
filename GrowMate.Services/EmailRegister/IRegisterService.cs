using GrowMate.Contracts.Requests;
using GrowMate.Contracts.Responses;

namespace GrowMate.Services.EmailRegister
{
    public interface IRegisterService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken ct = default);
        Task<AuthResponseDto> VerifyEmailAsync(VerifyEmailRequestDto request, CancellationToken ct = default);
    }
}