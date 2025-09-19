using GrowMate.Repositories.Models;
namespace GrowMate.Services.Authentication
{
    public interface ITokenService
    {
        (string token, DateTime expiresAt) GenerateToken(User user);
    }
}