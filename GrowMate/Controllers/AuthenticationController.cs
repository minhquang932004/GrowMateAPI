using GrowMate.Contracts.Requests;
using GrowMate.Contracts.Requests.Auth;
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
        private readonly IPasswordResetService _passwordResetService;

        public AuthenticationController(IRegisterService registerService, ILoginService loginService, IPasswordResetService passwordResetService)
        {
            _registerService = registerService;
            _loginService = loginService;
            _passwordResetService = passwordResetService;
        }

        /// <summary>
        /// Register a new user account.
        /// </summary>
        /// <remarks>Role: Anonymous (anyone can access)</remarks>
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request, CancellationToken ct)
        {
            var result = await _registerService.RegisterAsync(request, ct);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Verify a user's email address using a verification code.
        /// </summary>
        /// <remarks>Role: Anonymous (anyone can access)</remarks>
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail(VerifyEmailRequest request)
        {
            var result = await _registerService.VerifyEmailAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Resend the email verification code to a user.
        /// </summary>
        /// <remarks>Role: Anonymous (anyone can access)</remarks>
        [HttpPost("resend-verification")]
        public async Task<IActionResult> ResendVerification(ResendVerificationRequest request, CancellationToken ct)
        {
            var result = await _registerService.ResendVerificationCodeAsync(request, ct);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Request a password reset email.
        /// </summary>
        /// <remarks>Role: Anonymous (anyone can access)</remarks>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request, CancellationToken ct)
        {
            var result = await _passwordResetService.RequestResetAsync(request, ct);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Reset a user's password using a reset code.
        /// </summary>
        /// <remarks>Role: Anonymous (anyone can access)</remarks>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request, CancellationToken ct)
        {
            var result = await _passwordResetService.ResetPasswordAsync(request, ct);

            if (result.Success)
            {
                // Remove stale JWT cookie so the client doesn't carry an invalid token
                Response.Cookies.Delete("Token");
            }

            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Log in with email and password.
        /// </summary>
        /// <remarks>Role: Anonymous (anyone can access)</remarks>
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request, CancellationToken ct)
        {
            var result = await _loginService.LoginAsync(request, ct);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Initiate Google OAuth login.
        /// </summary>
        /// <remarks>Role: Anonymous (anyone can access)</remarks>
        [HttpGet("login-google")]
        public IActionResult LoginWithGoogle()
        {
            // Use absolute callback URL to avoid scheme/host mismatches behind proxies
            var scheme = Request.Scheme;
            var host = Request.Host;
            var redirectUrl = $"{scheme}://{host}/api/auth/google-callback";
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// Google OAuth callback endpoint.
        /// </summary>
        /// <remarks>Role: Anonymous (used by Google OAuth flow)</remarks>
        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback(CancellationToken ct)
        {
            try
            {
                var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                if (!result.Succeeded)
                {
                    // Fallback to Google scheme in case cookie scheme didn't capture the external principal
                    result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
                    if (!result.Succeeded)
                    {
                        return Unauthorized("Đăng nhập Google thất bại");
                    }
                }

                var email = result.Principal?.FindFirstValue(ClaimTypes.Email);
                var name = result.Principal?.FindFirstValue(ClaimTypes.Name);

                if (string.IsNullOrWhiteSpace(email))
                {
                    return BadRequest("Không thể lấy email từ Google");
                }
                if (string.IsNullOrWhiteSpace(name))
                {
                    name = email;
                }

                var user = await _loginService.LoginWithGoogle(email, name, ct);
                if (user == null || !user.Success || string.IsNullOrWhiteSpace(user.Token))
                {
                    return BadRequest(user?.Message ?? "Đăng nhập Google thất bại");
                }

                var redirectUrl = $"https://www.growmate.site/googleCallback?token={Uri.EscapeDataString(user.Token)}";

                Response.Cookies.Append("Token", user.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTimeOffset.Now.AddDays(3)
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