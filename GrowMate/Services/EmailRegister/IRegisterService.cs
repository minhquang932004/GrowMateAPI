using GrowMate.DTOs.Requests;
using GrowMate.DTOs.Responses;

namespace GrowMate.Services.EmailRegister
{
    public interface IRegisterService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
        Task<AuthResponseDto> VerifyEmailAsync(VerifyEmailRequestDto request);
    }
}
