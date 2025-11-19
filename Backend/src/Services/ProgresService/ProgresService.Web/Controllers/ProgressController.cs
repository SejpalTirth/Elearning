using Microsoft.AspNetCore.Mvc;
using ProgressService.BLL.Interface;

namespace ProgressService.Web.Controllers
{
    [ApiController]
    [Route("api/progress")]
    public class ProgressController : ControllerBase
    {
        private readonly IProgressService _service;

        public ProgressController(IProgressService service)
        {
            _service = service;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserProgress(Guid userId)
        {
            var data = await _service.GetUserProgressAsync(userId);
            return Ok(data);
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary([FromQuery] Guid userId)
        {
            var summary = await _service.GetSummaryAsync(userId);
            return Ok(summary);
        }
    }
}
