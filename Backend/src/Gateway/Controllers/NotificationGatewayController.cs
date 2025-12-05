using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/notification")]
    public class NotificationGatewayController : ControllerBase
    {
        private readonly HttpClient _client;

        public NotificationGatewayController(IHttpClientFactory factory)
        {
            _client = factory.CreateClient("NotificationService");
        }

        // Test Endpoint
        [HttpGet("test")]
        public async Task<IActionResult> Test()
        {
            var response = await _client.GetAsync("/api/notification/test");
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }

        // Send Email Directly
        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] object request)
        {
            var response = await _client.PostAsJsonAsync("/api/notification/send", request);
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }

        // Send Email Using Template
        [HttpPost("template/send")]
        public async Task<IActionResult> SendTemplate([FromBody] object request)
        {
            var response = await _client.PostAsJsonAsync("/api/notification/template/send", request);
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }

        // Trigger Notification (Enrollment, Module Done, Course Done)
        [HttpPost("trigger")]
        public async Task<IActionResult> Trigger([FromBody] object request)
        {
            var response = await _client.PostAsJsonAsync("/api/notification/trigger", request);
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }

        // Get User Notifications
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserNotifications(Guid userId)
        {
            var response = await _client.GetAsync($"/api/notification/{userId}");
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }
    }
}
