using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GatewayAuthController : ControllerBase
    {
        [HttpGet("google-login")]
        public IActionResult GoogleLogin([FromQuery] string? returnUrl = "/")
        {
            var redirectUrl = Url.Action(nameof(ExternalResponse), "GatewayAuth", new { returnUrl });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, "Google");
        }

        [HttpGet("microsoft-login")]
        public IActionResult MicrosoftLogin([FromQuery] string? returnUrl = "/")
        {
            var redirectUrl = Url.Action(nameof(ExternalResponse), "GatewayAuth", new { returnUrl });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, "Microsoft");
        }

        [HttpGet("external-response")]
        public async Task<IActionResult> ExternalResponse([FromQuery] string? returnUrl = "/")
        {
            var result = await HttpContext.AuthenticateAsync("External");
            if (!result.Succeeded)
                return BadRequest("External authentication error");

            var email = result.Principal.FindFirst(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
            var name = result.Principal.Identity?.Name;

            return Ok(new { Email = email, Name = name, ReturnUrl = returnUrl });
        }
    }
}
