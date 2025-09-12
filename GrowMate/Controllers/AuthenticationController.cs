using GrowMate.Data;
using GrowMate.DTOs.Requests;
using GrowMate.DTOs.Responses;
using GrowMate.Services.Auth;
using GrowMate.Services.EmailRegister;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GrowMate.Models.Roles;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace GrowMate.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IRegisterService _registerService;
        private readonly EXE201_GrowMateContext _dbContext;
        private readonly ITokenService _tokenService;
        private readonly ILoginWithGoogleService _loginService;

        public AuthenticationController(IRegisterService registerService, EXE201_GrowMateContext dbContext, ITokenService tokenService, ILoginWithGoogleService loginService)
        {
            _registerService = registerService;
            _dbContext = dbContext;
            _tokenService = tokenService;
            _loginService = loginService;
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
        [HttpGet("login-google")]
        public IActionResult LoginWithGoogle()
        {
            var redirectUrl = Url.Action("GoogleCallback", "Authentication");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback()
        {
            try
            {
                var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                if (!result.Succeeded)
                {
                    return Unauthorized("Đăng nhập Google thất bại");

                }

                var email = result.Principal.FindFirstValue(ClaimTypes.Email);
                var name = result.Principal.FindFirstValue(ClaimTypes.Name);
                var picture = result.Principal.FindFirstValue("pictures");

                var user = await _loginService.LoginWithGoogle(email, name);
                var redirectUrl = $"http://localhost:5173?Token={user.Token}";// hardcode FE, will change later

                Response.Cookies.Append("Token", user.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true, // Bật nếu dùng HTTPS
                    SameSite = SameSiteMode.None,
                    Expires = DateTimeOffset.UtcNow.AddDays(3) // Hoặc theo cấu hình
                });
                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}