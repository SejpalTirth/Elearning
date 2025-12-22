using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProgresService.BLL.DTOs;
using ProgresService.BLL.Interface;
using ProgresService.BLL.UserContext;

namespace ProgressService.Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/progress")]
    public class ProgressController : ControllerBase
    {
            private readonly IProgressService _service;
            private readonly IHttpClientFactory _httpClientFactory;
            private readonly IUserContextAccessor _userContext;

            public ProgressController(
                IProgressService service,
                IHttpClientFactory httpClientFactory,
                IUserContextAccessor userContext)
            {
                _service = service;
                _httpClientFactory = httpClientFactory;
                _userContext = userContext;
            }



        // ---------------- USER PROGRESS ----------------

        [HttpPost("user")]
        public async Task<IActionResult> GetUserProgress()
        {
            var userId = _userContext.Current?.UserId;

            if (userId == null || userId == Guid.Empty)
                return Unauthorized("Invalid user context.");

            var data = await _service.GetUserProgressAsync(userId.Value);
            return Ok(data);
        }

        // ---------------- COMPLETE MODULE ----------------

        [HttpPost("complete-module")]
        public async Task<IActionResult> CompleteModule(
            [FromBody] ModuleCompleteRequest request)
        {
            var userId = _userContext.Current?.UserId;

            if (userId == null || userId == Guid.Empty)
                return Unauthorized("Invalid user context.");

            var client = _httpClientFactory.CreateClient("CourseService");

            // FORWARD AUTHORIZATION HEADER
            if (Request.Headers.TryGetValue("Authorization", out var token))
            {
                client.DefaultRequestHeaders
                      .TryAddWithoutValidation("Authorization", token.ToString());
            }

            var response = await client.PostAsJsonAsync(
                "api/modules/course-id",
                new { moduleId = request.ModuleId }
            );

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return BadRequest(new
                {
                    message = "Failed to fetch course info from CourseService.",
                    status = response.StatusCode,
                    details = error
                });
            }

            var courseInfo = await response.Content
                .ReadFromJsonAsync<CourseIdResponseDTO>();

            if (courseInfo == null)
                return NotFound($"Module {request.ModuleId} not found.");

            await _service.MarkModuleCompletedAsync(
                userId.Value,
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
