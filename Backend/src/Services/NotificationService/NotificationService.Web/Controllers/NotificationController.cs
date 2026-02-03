using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DTOs._6NotificationService;
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Send(
            [FromBody] EmailRequestDTO req)
        {
            await _service.SendEmailAsync(
                req.UserId,
                req.Subject,
                req.Body);

            return Ok("Email sent successfully.");
        }

        // ---------------- Template Email ----------------

        [HttpPost("template/send")]
        [ProducesResponseType(StatusCodes.Status200OK)]
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
        [ProducesResponseType(typeof(NotificationDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<NotificationDto>> GetUserNotifications(
            [FromBody] UserIdRequestDto dto)
        {
            var result = await _service.GetUserNotificationsAsync(dto.UserId);
            return Ok(result);
        }

        // ---------------- Triggered Notification ----------------

        [HttpPost("trigger")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> TriggerNotification(
            [FromBody] TriggerNotificationDto request)
        {
            await _service.HandleTriggeredNotificationAsync(request);
            return Ok("Notification processed successfully.");
        }
    }
}
