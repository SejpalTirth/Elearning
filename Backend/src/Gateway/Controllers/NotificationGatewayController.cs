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

        // ------------------- SEND EMAIL -------------------

        [HttpPost("send")]
        public Task<IActionResult> Send(
            [FromBody] EmailRequest request) =>
            ForwardPost($"{BASE}/send", request);

        // ------------------- SEND TEMPLATE EMAIL -------------------

        [HttpPost("template/send")]
        public Task<IActionResult> SendTemplate(
            [FromBody] TemplateRequest request) =>
            ForwardPost($"{BASE}/template/send", request);

        // ------------------- TRIGGER NOTIFICATION -------------------

        [HttpPost("trigger")]
        public Task<IActionResult> Trigger(
            [FromBody] TriggerNotification request) =>
            ForwardPost($"{BASE}/trigger", request);

        // ------------------- USER NOTIFICATIONS -------------------

        [HttpPost("user")]
        public Task<IActionResult> GetUserNotifications(
            [FromBody] UserIdRequest request) =>
            ForwardPost($"{BASE}/user", request);
    }
}
