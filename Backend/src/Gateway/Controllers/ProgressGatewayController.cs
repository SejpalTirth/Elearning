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
    [ProducesResponseType(typeof(List<ProgressRecordDto>), 200)]
    public Task<IActionResult> GetUserProgress() =>
        ForwardPost($"{BASE}/user", new { });

    [HttpPost("complete-module")]
    public Task<IActionResult> CompleteModule(
        [FromBody] ModuleCompleteRequest payload) =>
        ForwardPost($"{BASE}/complete-module", payload);

    public class ProgressRecordDto
    {
        public int CourseId { get; set; }
        public int? ModuleId { get; set; }
        public bool IsCompleted { get; set; }
        public double ProgressPercent { get; set; }
    }

}
