using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.BLL.DTOs;
using NotificationService.BLL.Interface;

namespace NotificationService.Web.Controllers
{
    [Authorize]
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
        public async Task<IActionResult> Send(
            [FromBody] EmailRequest req)
        {
            await _service.SendEmailAsync(
                req.UserId,
                req.Subject,
                req.Body);

            return Ok("Email sent successfully.");
        }

        // ---------------- Template Email ----------------

        [HttpPost("template/send")]
        public async Task<IActionResult> SendTemplate(
            [FromBody] TemplateRequestDTO req)
        {
            await _service.SendEmailByTemplateAsync(
                req.UserId,
                req.TemplateName,
                req.Model);

            return Ok("Template email sent successfully.");
        }

        // ---------------- User Notifications ----------------

        [HttpPost("user")]
        public async Task<IActionResult> GetUserNotifications(
            [FromBody] UserIdRequestDto dto)
        {
            return Ok(await _service
                .GetUserNotificationsAsync(dto.UserId));
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
    }
}
