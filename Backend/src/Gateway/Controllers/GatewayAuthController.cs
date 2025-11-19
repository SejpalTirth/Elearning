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
    public IActionResult GoogleLogin()
    {
        var returnUrl = "http://localhost:4200/gateway/auth/callback";

        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action("ExternalResponse", new { returnUrl })
        };

        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }


    // MICROSOFT LOGIN
    [HttpGet("microsoft-login")]
    public IActionResult MicrosoftLogin()
    {
        var returnUrl = "http://localhost:4200/gateway/auth/callback";

        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action("ExternalResponse", new { returnUrl })
        };

        return Challenge(properties, "Microsoft");
    }


    // CALLBACK FROM GOOGLE/MICROSOFT
    [HttpGet("external-response")]
    public async Task<IActionResult> ExternalResponse([FromQuery] string? returnUrl = null)
    {
        // Angular callback URL (DEFAULT)
        returnUrl ??= "http://localhost:4200/gateway/auth/callback";


        var result = await HttpContext.AuthenticateAsync("External");

        if (!result.Succeeded)
        {
            _logger.LogError("External authentication failed. Details: {Error}", result.Failure?.Message);
            return BadRequest("External authentication error");
        }

        // Extract external provider
        var provider = result.Properties?.Items?[".AuthScheme"] ?? "Unknown";

        // Extract provider user ID
        var providerUserId = result.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // Extract email and name
        var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;
        var name = result.Principal.FindFirst(ClaimTypes.Name)?.Value;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(providerUserId))
            return BadRequest("Missing required external login fields.");

        // Generate tokens
        var tokens = await _authService.SignInExternalAsync(
            provider,
            providerUserId,
            email,
            name ?? "Unknown User"
        );

        // Redirect to Angular with tokens
        return Redirect($"{returnUrl}?token={tokens.AccessToken}&refresh={tokens.RefreshToken}");
    }

}
