using Gateway.Contracts.Auth;
using GatewayService.BLL.Interface;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class GatewayAuthController : ControllerBase
{
    private readonly IAuthService _auth;
    private readonly ILogger<GatewayAuthController> _log;
    private readonly IConfiguration _config;

    public GatewayAuthController(
        IAuthService auth,
        ILogger<GatewayAuthController> log,
        IConfiguration config)
    {
        _auth = auth;
        _log = log;
        _config = config;
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
        returnUrl = "http://localhost:4200/auth/callback";

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
        {
            Response.Cookies.Append(
                "access_token",
                result.Tokens.AccessToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Path = "/",
                    Domain = "localhost"
                }
            );

            Response.Cookies.Append(
                "refresh_token",
                result.Tokens.RefreshToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Path = "/",
                    Domain = "localhost"
                }
            );
        }
        return Redirect(returnUrl);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken = Request.Cookies["refresh_token"];
        if (string.IsNullOrEmpty(refreshToken))
            return Unauthorized();

        var tokens = await _auth.RefreshTokenAsync(refreshToken);
        if (tokens == null)
            return Unauthorized();

        Response.Cookies.Append(
            "access_token",
            tokens.AccessToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/",
                Domain = "localhost"
            });

        Response.Cookies.Append(
            "refresh_token",
            tokens.RefreshToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/",
                Domain = "localhost"
            });

        return Ok();
    }


    [AllowAnonymous]
    [HttpGet("logout")]
    public async Task<IActionResult> Logout([FromQuery] string? redirectUrl)
    {
        var refreshToken = Request.Cookies["refresh_token"];
        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            await _auth.RevokeRefreshTokenAsync(refreshToken);
        }

        var cookieOptions = new CookieOptions
        {
            Path = "/",
            Domain = "localhost",
            Secure = true,
            SameSite = SameSiteMode.None
        };

        Response.Cookies.Delete("access_token", cookieOptions);
        Response.Cookies.Delete("refresh_token", cookieOptions);
        Response.Cookies.Delete(".Gateway.Auth", cookieOptions);
        Response.Cookies.Delete(".Gateway.External", cookieOptions);

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignOutAsync("External");

        if (!string.IsNullOrWhiteSpace(redirectUrl))
        {
            return Redirect(redirectUrl);
        }

        return Ok(new { message = "Logged out successfully" });
    }


    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var userId =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        return Ok(new
        {
            userId,
            email = User.FindFirst(ClaimTypes.Email)?.Value
                    ?? User.FindFirst(JwtRegisteredClaimNames.Email)?.Value,
            name = User.FindFirst("name")?.Value
                   ?? User.FindFirst(ClaimTypes.Name)?.Value,
            role = User.FindFirst(ClaimTypes.Role)?.Value,
            provider = User.FindFirst("provider")?.Value
        });
    }

    [HttpPost("local-register")]
    public async Task<IActionResult> LocalRegister([FromBody] RegisterRequest request)
    {
        try
        {
            var userId = await _auth.RegisterLocalAsync(
                request.Email,
                request.Password
            );

            return Ok(new
            {
                isNewUser = true,
                userId = userId
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("local-login")]
    public async Task<IActionResult> LocalLogin([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { message = "Email and password are required" });

        var tokens = await _auth.LoginLocalAsync(request.Email, request.Password);

        if (tokens == null)
            return BadRequest(new { message = "Invalid email or password" });

        Response.Cookies.Append(
            "access_token",
            tokens.AccessToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/",
                Domain = "localhost"
            }
        );

        Response.Cookies.Append(
            "refresh_token",
            tokens.RefreshToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/",
                Domain = "localhost"
            }
        );

        return Ok(new
        {
            success = true,
            message = "Login successful"
        });
    }
}