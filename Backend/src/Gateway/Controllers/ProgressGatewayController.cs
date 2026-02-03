using Gateway.Contracts.Progress;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/progress")]
    public class ProgressGatewayController : BaseGatewayController
    {
        private const string BASE = "api/progress";

        public ProgressGatewayController(IHttpClientFactory factory)
            : base(factory.CreateClient("ProgressService"))
        {
        }

        [HttpPost("user")]
        [ProducesResponseType(typeof(List<ProgressRecordDto>), 200)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserProgress()
        {
            try
            {
                var result = await ForwardPost($"{BASE}/user", new { });
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
            

        [HttpPost("complete-module")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CompleteModule(
            [FromBody] ModuleCompleteRequest payload)
        {
            try
            {
                var result = await ForwardPost($"{BASE}/complete-module", payload);

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

        public class ProgressRecordDto
        {
            public int CourseId { get; set; }
            public int? ModuleId { get; set; }
            public bool IsCompleted { get; set; }
            public double ProgressPercent { get; set; }
        }

    }

}