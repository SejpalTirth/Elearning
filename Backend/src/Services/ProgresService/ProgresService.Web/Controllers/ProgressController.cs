using Microsoft.AspNetCore.Mvc;
using ProgressService.BLL.Interface;
using ProgresService.BLL.DTOs;

namespace ProgressService.Web.Controllers
{
    [ApiController]
    [Route("api/progress")]
    public class ProgressController : ControllerBase
    {
        private readonly IProgressService _service;
        private readonly IHttpClientFactory _httpClientFactory;

        public ProgressController(
            IProgressService service,
            IHttpClientFactory httpClientFactory)
        {
            _service = service;
            _httpClientFactory = httpClientFactory;
        }

        // GET → user progress
        [HttpGet("{userId:guid}")]
        public async Task<IActionResult> GetUserProgress(Guid userId)
        {
            if (userId == Guid.Empty)
                return BadRequest("userId cannot be empty.");

            var data = await _service.GetUserProgressAsync(userId);
            return Ok(data);
        }

        // POST → Complete Module
        [HttpPost("complete-module")]
        public async Task<IActionResult> CompleteModule(
            [FromBody] ModuleCompleteRequest request)
        {
            if (request.ModuleId <= 0)
                return BadRequest("moduleId must be greater than zero.");

            if (request.UserId == Guid.Empty)
                return BadRequest("userId cannot be empty.");

            var client = _httpClientFactory.CreateClient("CourseService");

            CourseIdResponseDTO? courseInfo;

            try
            {
                courseInfo = await client.GetFromJsonAsync<CourseIdResponseDTO>(
                    $"api/Modules/course-id/{request.ModuleId}");
            }
            catch
            {
                return BadRequest(new
                {
                    message = "Failed to fetch course info from CourseService."
                });
            }

            if (courseInfo == null)
            {
                return NotFound(new
                {
                    message = $"Module {request.ModuleId} not found in CourseService."
                });
            }

            await _service.MarkModuleCompletedAsync(
                request.UserId,
                courseInfo.CourseId,
                request.ModuleId
            );

            return Ok(new
            {
                message = "Module marked as completed successfully.",
                moduleId = request.ModuleId,
                courseId = courseInfo.CourseId
            });
        }
    }
}
