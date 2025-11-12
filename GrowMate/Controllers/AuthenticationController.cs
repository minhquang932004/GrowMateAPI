using GrowMate.Contracts.Requests.Auth;
using GrowMate.Services.Authentication;
using GrowMate.Services.EmailRegister;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace GrowMateWebAPIs.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IRegisterService _registerService;
        private readonly ILoginService _loginService;
        private readonly IPasswordResetService _passwordResetService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly GoogleOAuthOptions _googleOptions;

        public AuthenticationController(
            IRegisterService registerService,
            ILoginService loginService,
            IPasswordResetService passwordResetService,
            IHttpClientFactory httpClientFactory,
            IOptions<GoogleOAuthOptions> googleOptions)
        {
            _registerService = registerService;
            _loginService = loginService;
            _passwordResetService = passwordResetService;
            _httpClientFactory = httpClientFactory;
            _googleOptions = googleOptions.Value;
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
        /// Exchange Google authorization code for JWT in GrowMate.
        /// </summary>
        /// <remarks>
        /// The frontend obtains the Google authorization code via Google Identity Services (popup)
        /// and posts it to this endpoint. No cookies are involved, which keeps the flow reliable
        /// even when third-party cookies are blocked by the browser.
        /// </remarks>
        [HttpPost("google")]
        public async Task<IActionResult> LoginWithGoogleCode([FromBody] GoogleAuthCodeRequest request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                return BadRequest("Thiếu Google authorization code.");
            }

            if (string.IsNullOrWhiteSpace(_googleOptions.ClientId) || string.IsNullOrWhiteSpace(_googleOptions.ClientSecret))
            {
                return StatusCode(500, "Google OAuth chưa được cấu hình.");
            }

            try
            {
                var tokens = await ExchangeCodeForTokensAsync(request.Code, ct);
                if (tokens is null || string.IsNullOrWhiteSpace(tokens.IdToken))
                {
                    return BadRequest("Không đổi được token từ Google.");
                }

                var payload = await GoogleJsonWebSignature.ValidateAsync(
                    tokens.IdToken,
                    new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = new[] { _googleOptions.ClientId }
                    });

                if (payload is null || string.IsNullOrWhiteSpace(payload.Email))
                {
                    return BadRequest("Không thể lấy email từ Google.");
                }

                var name = string.IsNullOrWhiteSpace(payload.Name) ? payload.Email : payload.Name;
                var user = await _loginService.LoginWithGoogle(payload.Email, name, ct);

                if (user == null || !user.Success || string.IsNullOrWhiteSpace(user.Token))
                {
                    return BadRequest(user?.Message ?? "Đăng nhập Google thất bại");
                }

                return Ok(user);
            }
            catch (InvalidJwtException ex)
            {
                return BadRequest($"Token Google không hợp lệ: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Đăng nhập Google thất bại: {ex.Message}");
            }
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

                var redirectUrl = $"https://www.growmate.site/google-callback?token={Uri.EscapeDataString(user.Token)}";

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

        private async Task<GoogleTokenResponse?> ExchangeCodeForTokensAsync(string code, CancellationToken ct)
        {
            var client = _httpClientFactory.CreateClient(nameof(AuthenticationController));

            using var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["code"] = code,
                ["client_id"] = _googleOptions.ClientId,
                ["client_secret"] = _googleOptions.ClientSecret,
                ["redirect_uri"] = "postmessage",
                ["grant_type"] = "authorization_code"
            });

            var request = new HttpRequestMessage(HttpMethod.Post, "https://oauth2.googleapis.com/token")
            {
                Content = content
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await client.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            using var stream = await response.Content.ReadAsStreamAsync(ct);
            var json = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

            if (!json.RootElement.TryGetProperty("id_token", out var idTokenElement))
            {
                return null;
            }

            return new GoogleTokenResponse
            {
                IdToken = idTokenElement.GetString() ?? string.Empty
            };
        }

        private sealed class GoogleTokenResponse
        {
            public string IdToken { get; set; } = string.Empty;
        }
    }
}