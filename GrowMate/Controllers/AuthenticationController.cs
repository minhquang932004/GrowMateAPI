using GrowMate.Contracts.Requests;
using GrowMate.Services.Authentication;
using GrowMate.Services.EmailRegister;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GrowMateWebAPIs.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IRegisterService _registerService;
        private readonly ILoginService _loginService;
        //private readonly ILoginWithGoogleService _loginWithGoogleService;

        public AuthenticationController(IRegisterService registerService, ILoginService loginService)
        {
            _registerService = registerService;
            _loginService = loginService;
            //_loginWithGoogleService = loginWithGoogleService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequestDto request, CancellationToken ct)
        {
            var result = await _registerService.RegisterAsync(request, ct);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail(VerifyEmailRequestDto request)
        {
            var result = await _registerService.VerifyEmailAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto request, CancellationToken ct)
        {
            var result = await _loginService.LoginAsync(request, ct);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("login-google")]
        public IActionResult LoginWithGoogle()
        {
            var redirectUrl = Url.Action("GoogleCallback", "Authentication");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback(CancellationToken ct)
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

                var user = await _loginService.LoginWithGoogle(email, name, ct);
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