using Microsoft.AspNetCore.Mvc;
using NotificationService.BLL.DTOs;
using NotificationService.BLL.Interface;

namespace NotificationService.Web.Controllers
{
    [ApiController]
    [Route("api/notification")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _service;

        public NotificationController(INotificationService service)
        {
            _service = service;
        }

        // ---------------- Direct Email ----------------

        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] EmailRequest req)
        {
            if (req.UserId == Guid.Empty)
                return BadRequest("UserId cannot be empty.");

            await _service.SendEmailAsync(req.UserId, req.Subject, req.Body);
            return Ok("Email sent successfully.");
        }

        // ---------------- Template Email ----------------

        [HttpPost("template/send")]
        public async Task<IActionResult> SendTemplate([FromBody] TemplateRequestDTO req)
        {
            if (req.UserId == Guid.Empty)
                return BadRequest("UserId cannot be empty.");

            await _service.SendEmailByTemplateAsync(
                req.UserId,
                req.TemplateName,
                req.Model
            );

            return Ok("Template email sent successfully.");
        }

        // ---------------- User Notifications ----------------

        [HttpGet("{userId:guid}")]
        public async Task<IActionResult> GetUserNotifications(Guid userId)
        {
            if (userId == Guid.Empty)
                return BadRequest("UserId cannot be empty.");

            return Ok(await _service.GetUserNotificationsAsync(userId));
        }

        // ---------------- Triggered Notification ----------------

        [HttpPost("trigger")]
        [Consumes("application/json")]
        public async Task<IActionResult> TriggerNotification(
            [FromBody] TriggerNotificationDto request)
        {
            await _service.HandleTriggeredNotificationAsync(request);
            return Ok("Notification processed successfully.");
        }

        // ---------------- Test Endpoint (Demo only) ----------------
        // NOTE: Keep for development/demo purposes only

        [HttpGet("test")]
        public async Task<IActionResult> Test()
        {
            await _service.SendEmailDirectAsync(
                "tirths331@outlook.com",
                "Test Email",
                "This is a successful test email from Notification Service!"
            );

            return Ok("Test email sent.");
        }
    }
}
