using Microsoft.AspNetCore.Mvc;
using ProgressService.BLL.Interface;
using ProgresService.BLL.DTOs;
using System.Net.Http;
using System.Net.Http.Json;

namespace ProgressService.Web.Controllers
{
    [ApiController]
    [Route("api/progress")]
    public class ProgressController : ControllerBase
    {
        private readonly IProgressService _service;
        private readonly IHttpClientFactory _httpClientFactory;

        public ProgressController(IProgressService service, IHttpClientFactory httpClientFactory)
        {
            _service = service;
            _httpClientFactory = httpClientFactory;
        }

        // GET → user progress
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserProgress(Guid userId)
        {
            var data = await _service.GetUserProgressAsync(userId);
            return Ok(data);
        }

        // POST → Complete Module (Only userId + moduleId)
        [HttpPost("complete-module")]
        public async Task<IActionResult> CompleteModule([FromBody] ModuleCompleteRequest request)
        {
            if (request == null)
                return BadRequest("Invalid request body.");

            // 1️⃣ Call CourseService to get the correct courseId for this module
            var client = _httpClientFactory.CreateClient("CourseService");

            CourseIdResponse? courseInfo = null;

            try
            {
                courseInfo = await client.GetFromJsonAsync<CourseIdResponse>(
                    $"api/Modules/course-id/{request.ModuleId}");
            }
            catch
            {
                return BadRequest(new { message = "Failed to fetch course info from CourseService." });
            }

            if (courseInfo == null)
                return NotFound(new { message = $"Module {request.ModuleId} not found in CourseService." });

            // 2️⃣ Now call existing service with correct courseId
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

    // DTO returned by CourseService
    public class CourseIdResponse
    {
        public int CourseId { get; set; }
        public int ModuleId { get; set; }
    }
}
