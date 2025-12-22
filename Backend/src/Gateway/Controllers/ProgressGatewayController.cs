using Gateway.Contracts.Progress;
using Gateway.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route("api/progress")]
public class ProgressGatewayController : BaseGatewayController
{
    private const string BASE = "api/progress";

    public ProgressGatewayController(IHttpClientFactory factory)
        : base(factory.CreateClient("ProgressService"))
    {
    }

    [HttpPost("user")]
    public Task<IActionResult> GetUserProgress() =>
        ForwardPost($"{BASE}/user", new { });

    [HttpPost("complete-module")]
    public Task<IActionResult> CompleteModule(
        [FromBody] ModuleCompleteRequest payload) =>
        ForwardPost($"{BASE}/complete-module", payload);
}
