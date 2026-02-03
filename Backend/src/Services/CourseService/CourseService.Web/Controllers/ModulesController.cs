using CourseService.BLL.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DTOs._2CourseService;

namespace CourseService.Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/modules")]
    public class ModulesController : ControllerBase
    {
        private readonly IModuleService _moduleService;

        public ModulesController(IModuleService moduleService)
        {
            _moduleService = moduleService;
        }

        // ---------------- MODULES BY COURSE ----------------

        [HttpPost("by-course")]
        [ProducesResponseType(typeof(IEnumerable<ModuleSummaryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByCourse(
            [FromBody] CourseIdRequestDto dto)
        {
            var modules = await _moduleService
                .GetModulesByCourseAsync(dto.CourseId);

            return Ok(modules);
        }

        // ---------------- MODULE CONTENT ----------------

        [HttpPost("content")]
        [ProducesResponseType(typeof(ModuleContentResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetContent(
            [FromBody] ModuleIdRequestDto dto)
        {
            var module = await _moduleService
                .GetModuleContentAsync(dto.ModuleId);

            return module == null ? NotFound() : Ok(module);
        }

        // ---------------- MODULE + COURSE ID ----------------

        [HttpPost("course-id")]
        public async Task<IActionResult> GetCourseId(
            [FromBody] ModuleIdRequestDto dto)
        {
            var data = await _moduleService
                .GetModuleAndCourseIdAsync(dto.ModuleId);

            return data == null
                ? NotFound(new { message = $"Module {dto.ModuleId} not found." })
                : Ok(data);
        }
    }
}
