using GrowMate.DTOs.Responses;

namespace GrowMate.Services.Auth
{
    public interface ILoginService
    {
        Task<LoginResponseDto> LoginWithGoogle(string email, string name);
    }
}
