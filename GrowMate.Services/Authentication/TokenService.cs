using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GrowMate.Repositories.Models;
using GrowMate.Repositories.Models.Roles;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace GrowMate.Services.Authentication
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config) => _config = config;

        public (string token, DateTime expiresAt) GenerateToken(User user)
        {
            var key = _config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured.");
            var issuer = _config["Jwt:Issuer"];
            var audience = _config["Jwt:Audience"];
            var expiresMinutes = int.TryParse(_config["Jwt:ExpiresMinutes"], out var m) ? m : 60;

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new(ClaimTypes.Name, user.FullName ?? string.Empty),
                new(ClaimTypes.Role, UserRoles.ToName(user.Role))
            };

            var expiresAt = DateTime.Now.AddMinutes(expiresMinutes);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return (tokenString, expiresAt);
        }
    }
}