using DTOs._6NotificationService;
using Gateway.Contracts.Notification;
using Gateway.Contracts.Users;
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Send(
            [FromBody] EmailRequest request)
        {
            try
            {
                var result = await ForwardPost($"{BASE}/send", request);
                return result;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Gateway error",
                    details = ex.Message
                });
            }
        }

        // ------------------- SEND TEMPLATE EMAIL -------------------

        [HttpPost("template/send")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SendTemplate(
            [FromBody] TemplateRequest request)
        {
            try
            {
                var result = await ForwardPost($"{BASE}/template/send", request);
                return result;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Gateway error",
                    details = ex.Message
                });
            }
        }

        // ------------------- TRIGGER NOTIFICATION -------------------

        [HttpPost("trigger")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Trigger(
            [FromBody] TriggerNotification request)
        {
            try
            {
                var result = await ForwardPost($"{BASE}/trigger", request);
                return result;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Gateway error",
                    details = ex.Message
                });
            }
        }

        // ------------------- USER NOTIFICATIONS -------------------

        [HttpPost("user")]
        [ProducesResponseType(typeof(NotificationDto[]), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<NotificationDto>> GetUserNotifications(
            [FromBody] UserIdRequest request)
        {
            try
            {

                var result = await ForwardPost($"{BASE}/user", request);
                if (result is OkObjectResult okResult)
                {
                    return Ok(okResult.Value);
                }
                return result as ActionResult;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Gateway error",
                    details = ex.Message
                });
            }
        }
    }
}
