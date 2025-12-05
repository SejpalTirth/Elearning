using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace Gateway.Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/progress")]
    public class ProgressGatewayController : ControllerBase
    {
        private readonly HttpClient _client;

        public ProgressGatewayController(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient("ProgressService");
        }

        // Forward GET request to progress service
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserProgress(Guid userId)
        {
            var response = await _client.GetAsync($"/api/progress/{userId}");
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }

        // Forward POST request to progress service
        // Expects JSON body: { "userId": "GUID", "moduleId": 24 }
        [HttpPost("complete-module")]
        public async Task<IActionResult> CompleteModule([FromBody] object payload)
        {
            var json = JsonSerializer.Serialize(payload);
            var requestContent = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/progress/complete-module", requestContent);

            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }
    }
}
