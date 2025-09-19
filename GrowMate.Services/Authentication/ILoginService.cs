using GrowMate.Contracts.Requests;
using GrowMate.Contracts.Responses;

namespace GrowMate.Services.Authentication
{
    public interface ILoginService
    {
        Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken ct = default);
    }
}
