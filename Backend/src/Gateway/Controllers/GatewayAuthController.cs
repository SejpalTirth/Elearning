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

        // Only allow local URLs
        if (Url.IsLocalUrl(returnUrl))
            return returnUrl;

        _logger.LogWarning("Blocked open redirect attempt to: {Url}", returnUrl);
        return "/";
    }

    [HttpGet("google-login")]
    public IActionResult GoogleLogin([FromQuery] string? returnUrl = "/")
    {
        returnUrl = SanitizeReturnUrl(returnUrl);
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action("ExternalResponse", new { returnUrl })
        };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("external-response")]
    public async Task<IActionResult> ExternalResponse([FromQuery] string? returnUrl = "/")
    {
        returnUrl = SanitizeReturnUrl(returnUrl);

        var result = await HttpContext.AuthenticateAsync("External");

        if (!result.Succeeded)
        {
            _logger.LogError("External authentication failed. Details: {Error}", result.Failure?.Message);
            return BadRequest("External authentication error");
        }

        // Extract user claims safely
        var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;
        var name = result.Principal.FindFirst(ClaimTypes.Name)?.Value;

        if (string.IsNullOrEmpty(email))
        {
            _logger.LogWarning("Google/Microsoft login missing email claim.");
            return BadRequest("Unable to retrieve email from external provider.");
        }

        if (string.IsNullOrEmpty(name))
        {
            _logger.LogWarning("Google/Microsoft login missing name claim.");
            name = "Unknown User"; // fallback if needed
        }

        // Now continue with your auth service logic
        var tokens = await _authService.HandleExternalLogin(email, name);

        return Redirect($"{returnUrl}?token={tokens.AccessToken}&refresh={tokens.RefreshToken}");
    }
}
