using GrowMate.Models;

namespace GrowMate.Services.Auth
{
    public interface ITokenService
    {
        (string token, DateTime expiresAt) GenerateToken(User user);
    }
}