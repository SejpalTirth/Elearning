using Microsoft.AspNetCore.Mvc;

namespace Gateway.Web.Controllers
{
    [ApiController]
    [Route("api/progress")]
    public class ProgressGatewayController : ControllerBase
    {
        private readonly HttpClient _client;

        public ProgressGatewayController(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient("ProgressService");
        }

        // GET: api/progress/{userId}
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserProgress(Guid userId)
        {
            var response = await _client.GetAsync($"/api/progress/{userId}");
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }

        // GET: api/progress/summary?userId={id}
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary([FromQuery] Guid userId)
        {
            var response = await _client.GetAsync($"/api/progress/summary?userId={userId}");
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }
    }
}
