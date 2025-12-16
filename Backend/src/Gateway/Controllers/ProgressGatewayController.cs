using Gateway.Contracts.Progress;
using Gateway.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Web.Controllers
{
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

        // ------------------- GET USER PROGRESS -------------------

        [HttpGet("{userId:guid}")]
        public Task<IActionResult> GetUserProgress(Guid userId) =>
            ForwardGet($"{BASE}/{userId}");

        // ------------------- COMPLETE MODULE -------------------

        // Expects JSON body: { "userId": "GUID", "moduleId": 24 }
        [HttpPost("complete-module")]
        public Task<IActionResult> CompleteModule([FromBody] ModuleCompleteRequest payload) =>
            ForwardPost($"{BASE}/complete-module", payload);
    }
}
