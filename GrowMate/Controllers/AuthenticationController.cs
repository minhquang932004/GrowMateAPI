using GrowMate.Data;
using GrowMate.DTOs.Requests;
using GrowMate.DTOs.Responses;
using GrowMate.Services.Auth;
using GrowMate.Services.EmailRegister;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GrowMate.Models.Roles;

namespace GrowMate.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IRegisterService _registerService;
        private readonly EXE201_GrowMateContext _dbContext;
        private readonly ITokenService _tokenService;

        public AuthenticationController(IRegisterService registerService, EXE201_GrowMateContext dbContext, ITokenService tokenService)
        {
            _registerService = registerService;
            _dbContext = dbContext;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequestDto request)
        {
            var result = await _registerService.RegisterAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail(VerifyEmailRequestDto request)
        {
            var result = await _registerService.VerifyEmailAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto request)
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var user = await _dbContext.Users
                .Include(u => u.Customer)
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user is null)
                return BadRequest(new LoginResponseDto { Success = false, Message = "Invalid credentials." });

            if (user.IsActive != true)
                return BadRequest(new LoginResponseDto { Success = false, Message = "Email not verified." });

            var valid = BCrypt.Net.BCrypt.Verify(request.Password, user.Password);
            if (!valid)
                return BadRequest(new LoginResponseDto { Success = false, Message = "Invalid credentials." });

            var (token, expiresAt) = _tokenService.GenerateToken(user);

            var userDto = new UserDto
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                Phone = user.Phone,
                Role = user.Role,
                RoleName = UserRoles.ToName(user.Role),
                ProfileImageUrl = user.ProfileImageUrl,
                IsActive = user.IsActive ?? false
            };

            var customerDto = user.Customer is null ? null : new CustomerDto
            {
                CustomerId = user.Customer.CustomerId,
                ShippingAddress = user.Customer.ShippingAddress,
                WalletBalance = user.Customer.WalletBalance,
            };

            return Ok(new LoginResponseDto
            {
                Success = true,
                Message = "Login successful.",
                Token = token,
                ExpiresAt = expiresAt,
                User = userDto,
                Customer = customerDto
            });
        }
    }
}
