using GrowMate.DTOs.Responses;

namespace GrowMate.Services.Auth
{
    public interface ILoginWithGoogleService
    {
        Task<LoginResponseDto> LoginWithGoogle(string email, string name);
    }
}
