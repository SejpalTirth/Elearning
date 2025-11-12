// Controllers/GatewayAuthController.cs
using GatewayService.BLL.DTOs;
using GatewayService.BLL.Interface;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Gateway.Controllers
{
    

    [ApiController]
    [Route("api/[controller]")]
    public class GatewayAuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<GatewayAuthController> _logger;

        public GatewayAuthController(IAuthService authService, ILogger<GatewayAuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        // Start Google login
        [HttpGet("google-login")]
        public IActionResult GoogleLogin([FromQuery] string returnUrl = "/")
        {
            var properties = new AuthenticationProperties { RedirectUri = Url.Action(nameof(GoogleResponse), new { returnUrl }) };
            return Challenge(properties, "Google");
        }

        // Google callback
        [HttpGet("google-response")]
        public async Task<IActionResult> GoogleResponse([FromQuery] string returnUrl = "/")
        {
            // Authenticate the external cookie scheme (set in Startup)
            var result = await HttpContext.AuthenticateAsync("External");
            if (!result.Succeeded || result.Principal == null)
                return BadRequest("External authentication failed");

            var claims = result.Principal.Claims;
            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var providerId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(providerId))
                return BadRequest("Missing email or provider id from external provider");

            var tokens = await _authService.SignInExternalAsync("Google", providerId, email, name);

            // Option A: return tokens directly (JSON) to SPA or client
            return Ok(tokens);

            // Option B: redirect to client app with tokens in query string (beware security)
            // return Redirect($"{returnUrl}?accessToken={tokens.AccessToken}&refreshToken={tokens.RefreshToken}");
        }

        // Start Microsoft login
        [HttpGet("microsoft-login")]
        public IActionResult MicrosoftLogin([FromQuery] string returnUrl = "/")
        {
            var properties = new AuthenticationProperties { RedirectUri = Url.Action(nameof(MicrosoftResponse), new { returnUrl }) };
            return Challenge(properties, "Microsoft");
        }

        [HttpGet("microsoft-response")]
        public async Task<IActionResult> MicrosoftResponse([FromQuery] string returnUrl = "/")
        {
            var result = await HttpContext.AuthenticateAsync("External");
            if (!result.Succeeded || result.Principal == null)
                return BadRequest("External authentication failed");

            var claims = result.Principal.Claims;
            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var providerId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(providerId))
                return BadRequest("Missing email or provider id from external provider");

            var tokens = await _authService.SignInExternalAsync("Microsoft", providerId, email, name);
            return Ok(tokens);
        }

        // Refresh endpoint
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto dto)
        {
            var tokens = await _authService.RefreshTokenAsync(dto.RefreshToken);
            if (tokens == null) return Unauthorized();
            return Ok(tokens);
        }

        // Revoke endpoint
        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke([FromBody] RefreshRequestDto dto)
        {
            await _authService.RevokeRefreshTokenAsync(dto.RefreshToken);
            return NoContent();
        }
    }

}
