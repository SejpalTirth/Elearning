using Gateway.DTOs;
using GatewayService.BLL.Interface;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class GatewayAuthController : ControllerBase
{
    private readonly IAuthService _auth;
    private readonly ILogger<GatewayAuthController> _log;

    public GatewayAuthController(IAuthService auth, ILogger<GatewayAuthController> log)
    {
        _auth = auth;
        _log = log;
    }

    private string SafeReturn(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return "/";
        return Url.IsLocalUrl(url) ? url : "/";
    }

    // Google login
    [HttpGet("google-login")]
    public IActionResult Google([FromQuery] string? returnUrl = "/")
    {
        returnUrl = SafeReturn(returnUrl);

        var redirectUri = Url.Action(nameof(ExternalResponse),
                                     "GatewayAuth",
                                     new { returnUrl },
                                     Request.Scheme);

        var props = new AuthenticationProperties
        {
            RedirectUri = redirectUri
        };
        props.Items["prompt"] = "select_account";

        return Challenge(props, GoogleDefaults.AuthenticationScheme);
    }

    // Microsoft login
    [HttpGet("microsoft-login")]
    public IActionResult Microsoft([FromQuery] string? returnUrl = "/")
    {
        returnUrl = SafeReturn(returnUrl);

        var redirectUri = Url.Action(nameof(ExternalResponse),
                                     "GatewayAuth",
                                     new { returnUrl },
                                     Request.Scheme);

        var props = new AuthenticationProperties
        {
            RedirectUri = redirectUri
        };
        props.Items["prompt"] = "select_account";

        return Challenge(props, "Microsoft");
    }

    // Callback
    [AllowAnonymous]
    [HttpGet("external-response")]
    public async Task<IActionResult> ExternalResponse([FromQuery] string? returnUrl)
    {
        returnUrl = "http://localhost:4200/gateway/auth/callback";

        var external = await HttpContext.AuthenticateAsync("External");

        if (!external.Succeeded)
            return BadRequest("External authentication failed");

        var provider = external.Properties.Items[".AuthScheme"] ?? "External";
        var sub = external.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = external.Principal.FindFirst(ClaimTypes.Email)?.Value;
        var name = external.Principal.FindFirst(ClaimTypes.Name)?.Value;

        if (sub == null || email == null)
            return BadRequest("Invalid external data");

        var result = await _auth.SignInExternalAsync(provider, sub, email, name);

        if (result.IsNewUser && result.UserId.HasValue)
        {
            await HttpContext.SignOutAsync("External");
            return Redirect($"{returnUrl}?isNewUser=true&userId={result.UserId}");
        }

        // Create cookie
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, result.UserId?.ToString() ?? sub),
            new Claim(ClaimTypes.Name, name ?? ""),
            new Claim(ClaimTypes.Email, email ?? "")
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = true });

        await HttpContext.SignOutAsync("External");

        if (result.Tokens != null)
            return Redirect($"{returnUrl}?token={result.Tokens.AccessToken}&refresh={result.Tokens.RefreshToken}");

        return Redirect(returnUrl);
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        var tokens = await _auth.RefreshTokenAsync(request.RefreshToken);

        if (tokens == null)
        {
            _log.LogWarning("Refresh failed for token (maybe not found/expired/revoked)");
            return Unauthorized(new { message = "Invalid or expired refresh token" });
        }
        return Ok(tokens);
    }

    public class LogoutRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    [AllowAnonymous]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest req)
    {
        if (!string.IsNullOrWhiteSpace(req.RefreshToken))
            await _auth.RevokeRefreshTokenAsync(req.RefreshToken);

        // Clears identity cookies
        await HttpContext.SignOutAsync();
        await HttpContext.SignOutAsync("External");

        return Ok(new { message = "Logged out" });
    }
}