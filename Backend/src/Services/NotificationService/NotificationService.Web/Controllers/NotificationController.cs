using Microsoft.AspNetCore.Mvc;
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

        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] EmailRequest req)
        {
            await _service.SendEmailAsync(req.UserId, req.Subject, req.Body);
            return Ok("Sent");
        }

        [HttpPost("template/send")]
        public async Task<IActionResult> SendTemplate([FromBody] TemplateRequest req)
        {
            await _service.SendEmailByTemplateAsync(req.UserId, req.TemplateName, req.Model);
            return Ok("Sent using template");
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserNotifications(Guid userId)
        {
            return Ok(await _service.GetUserNotificationsAsync(userId));
        }

        [HttpGet("test")]
        public async Task<IActionResult> Test()
        {
            await _service.SendEmailDirectAsync(
                "tirths331@outlook.com",
                "Test Email",
                "This is a successful test email from Notification Service!"
            );

            return Ok("Test email sent!");
        }


    }

    public class EmailRequest
    {
        public Guid UserId { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }

    public class TemplateRequest
    {
        public Guid UserId { get; set; }
        public string TemplateName { get; set; }
        public object Model { get; set; }
    }
}
