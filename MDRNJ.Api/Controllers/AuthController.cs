using MDRNJ.Api.Services.Interfaces;
using MDRNJ.Shared.DTOs.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace MDRNJ.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        // Short-lived cookie that holds the Google result between the callback and /api/auth/google.
        public const string ExternalScheme = "External";
        public const string DefaultAppCallbackUrl = "mdrnjfitness://auth";

        private readonly IAuthService _auth;
        private readonly IAuthenticationSchemeProvider _schemes;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService auth, IAuthenticationSchemeProvider schemes,
            IConfiguration config, ILogger<AuthController> logger)
        {
            _auth = auth;
            _schemes = schemes;
            _config = config;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var result = await _auth.RegisterAsync(dto);
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await _auth.LoginAsync(dto);
            return Ok(result);
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto dto)
        {
            var result = await _auth.VerifyEmailAsync(dto);
            return Ok(result);
        }

        [HttpPost("resend-verification")]
        public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationDto dto)
        {
            await _auth.ResendVerificationAsync(dto);
            return Ok(new { message = "If that account needs verification, a new code is on its way." });
        }

        // The app opens this URL in a browser (WebAuthenticator). First visit redirects to Google;
        // Google returns to /signin-google, which stores the result in the External cookie and sends
        // the browser back here. Second visit issues our JWT and redirects into the app.
        [HttpGet("google")]
        public async Task<IActionResult> Google()
        {
            var appCallback = _config["Google:AppCallbackUrl"] ?? DefaultAppCallbackUrl;

            if (await _schemes.GetSchemeAsync(GoogleDefaults.AuthenticationScheme) is null)
                return Redirect($"{appCallback}#error=google_not_configured");

            var result = await HttpContext.AuthenticateAsync(ExternalScheme);
            if (!result.Succeeded)
            {
                return Challenge(
                    new AuthenticationProperties { RedirectUri = Url.Action(nameof(Google)) },
                    GoogleDefaults.AuthenticationScheme);
            }

            await HttpContext.SignOutAsync(ExternalScheme);

            try
            {
                var auth = await _auth.GoogleSignInAsync(result.Principal!);
                return Redirect($"{appCallback}#access_token={Uri.EscapeDataString(auth.AccessToken)}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Google sign-in failed");
                return Redirect($"{appCallback}#error={Uri.EscapeDataString(ex.Message)}");
            }
        }
    }
}
