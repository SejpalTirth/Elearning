using Gateway.Contracts.Notification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/notification")]
    public class NotificationGatewayController : BaseGatewayController
    {
        private const string BASE = "api/notification";

        public NotificationGatewayController(IHttpClientFactory factory)
            : base(factory.CreateClient("NotificationService"))
        {
        }

        // ------------------- TEST -------------------

        [HttpGet("test")]
        public Task<IActionResult> Test() =>
            ForwardGet($"{BASE}/test");

        // ------------------- SEND EMAIL -------------------

        [HttpPost("send")]
        public Task<IActionResult> Send([FromBody] EmailRequest request) =>
            ForwardPost($"{BASE}/send", request);

        // ------------------- SEND TEMPLATE EMAIL -------------------

        [HttpPost("template/send")]
        public Task<IActionResult> SendTemplate([FromBody] TemplateRequest request) =>
            ForwardPost($"{BASE}/template/send", request);

        // ------------------- TRIGGER NOTIFICATION -------------------

        [HttpPost("trigger")]
        public Task<IActionResult> Trigger([FromBody] TriggerNotification request) =>
            ForwardPost($"{BASE}/trigger", request);

        // ------------------- GET USER NOTIFICATIONS -------------------

        [HttpGet("{userId:guid}")]
        public Task<IActionResult> GetUserNotifications(Guid userId) =>
            ForwardGet($"{BASE}/{userId}");
    }
}
