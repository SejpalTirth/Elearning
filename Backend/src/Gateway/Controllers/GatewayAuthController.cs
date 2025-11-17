using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GatewayAuthController : ControllerBase
    {
        private readonly ILogger<GatewayAuthController> _logger;

        public GatewayAuthController(ILogger<GatewayAuthController> logger)
        {
            _logger = logger;
        }

        [HttpGet("google-login")]
        public IActionResult GoogleLogin([FromQuery] string? returnUrl = "/")
        {
            if (!Url.IsLocalUrl(returnUrl))
                returnUrl = "/";

            var redirectUrl = Url.Action(nameof(ExternalResponse), "GatewayAuth", new { returnUrl });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, "Google");
        }

        [HttpGet("microsoft-login")]
        public IActionResult MicrosoftLogin([FromQuery] string? returnUrl = "/")
        {
            if (!Url.IsLocalUrl(returnUrl))
                returnUrl = "/";

            var redirectUrl = Url.Action(nameof(ExternalResponse), "GatewayAuth", new { returnUrl });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, "Microsoft");
        }

        [HttpGet("external-response")]
        public async Task<IActionResult> ExternalResponse([FromQuery] string? returnUrl = "/")
        {
            var result = await HttpContext.AuthenticateAsync("External");

            if (!result.Succeeded)
            {
                _logger.LogWarning("External authentication failed. ReturnUrl: {ReturnUrl}", returnUrl);
                return BadRequest("External authentication error");
            }

            var email = result.Principal?.FindFirst(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
            var name = result.Principal?.Identity?.Name;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(name))
            {
                _logger.LogWarning("Missing essential claims after external authentication. Email: {Email}, Name: {Name}", email, name);
                return BadRequest("Required user claims not found");
            }

            return Ok(new { Email = email, Name = name, ReturnUrl = returnUrl });
        }
    }
}
