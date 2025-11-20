using GatewayService.BLL.Interface;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

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

    // Helper to validate returnUrl and prevent open redirect attacks
    private string SanitizeReturnUrl(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl))
            return "/";

        if (Url.IsLocalUrl(returnUrl))
            return returnUrl;

        _logger.LogWarning("Blocked open redirect attempt to: {Url}", returnUrl);
        return "/";
    }

    // GOOGLE LOGIN
    [HttpGet("google-login")]
    public IActionResult GoogleLogin([FromQuery] string? returnUrl = "/")
    {
        returnUrl = SanitizeReturnUrl(returnUrl);

        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action("ExternalResponse", new { returnUrl = "http://localhost:4200/gateway/auth/callback" })
        };

        // IMPORTANT FIX
        properties.Items["prompt"] = "select_account";

        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }



    // MICROSOFT LOGIN
    [HttpGet("microsoft-login")]
    public IActionResult MicrosoftLogin([FromQuery] string? returnUrl = "/")
    {
        returnUrl = SanitizeReturnUrl(returnUrl);

        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action("ExternalResponse", new { returnUrl = "http://localhost:4200/gateway/auth/callback" })

        };

        // IMPORTANT FIX
        properties.Items["prompt"] = "select_account";

        return Challenge(properties, "Microsoft");
    }


    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] string refreshToken)
    {
        try
        {
            // 1. Revoke refresh token in DB
            await _authService.RevokeRefreshTokenAsync(refreshToken);

            // 2. Clear external auth cookies
            await HttpContext.SignOutAsync("External");
            await HttpContext.SignOutAsync();

            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Logout failed");
            return StatusCode(500, "Logout failed");
        }
    }


    // CALLBACK FROM GOOGLE/MICROSOFT
    [HttpGet("external-response")]
    public async Task<IActionResult> ExternalResponse([FromQuery] string? returnUrl = null)
    {
        // Always fallback to Angular callback page
        returnUrl ??= "http://localhost:4200/gateway/auth/callback";

        var result = await HttpContext.AuthenticateAsync("External");

        if (!result.Succeeded)
        {
            _logger.LogError("External authentication failed. Details: {Error}", result.Failure?.Message);
            return BadRequest("External authentication error");
        }

        var provider = result.Properties?.Items?[".AuthScheme"] ?? "Unknown";
        var providerUserId = result.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;
        var name = result.Principal.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown User";

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(providerUserId))
            return BadRequest("Missing required external login fields.");

        var signInResult = await _authService.SignInExternalAsync(provider, providerUserId, email, name);

        if (signInResult.IsNewUser && signInResult.UserId.HasValue)
        {
            return Redirect($"{returnUrl}?isNewUser=true&userId={signInResult.UserId}");
        }

        if (signInResult.Tokens is null)
        {
            _logger.LogError("Sign-in returned no tokens for existing user {Email}", email);
            return StatusCode(500, "Sign-in error");
        }

        return Redirect($"{returnUrl}?token={signInResult.Tokens.AccessToken}&refresh={signInResult.Tokens.RefreshToken}");
    }



}
