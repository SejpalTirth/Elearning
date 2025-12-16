using Microsoft.AspNetCore.Mvc;
using CourseService.BLL.Interface;

namespace CourseService.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ModulesController : ControllerBase
    {
        private readonly IModuleService _moduleService;

        public ModulesController(IModuleService moduleService)
        {
            _moduleService = moduleService;
        }

        // GET /api/modules/course/5
        [HttpGet("course/{courseId : int}")]
        public async Task<IActionResult> GetByCourse(int courseId)
        {
            if (courseId <= 0)
                return BadRequest("courseId must be greater than zero.");

            var modules = await _moduleService.GetModulesByCourseAsync(courseId);
            return Ok(modules);
        }

        // GET /api/modules/10
        [HttpGet("{moduleId : int}")]
        public async Task<IActionResult> GetContent(int moduleId)
        {
            if (moduleId <= 0)
                return BadRequest("courseId must be greater than zero.");

            var module = await _moduleService.GetModuleContentAsync(moduleId);

            if (module == null)
                return NotFound();

            return Ok(module);
        }

        [HttpGet("course-id/{moduleId : int}")]
        public async Task<IActionResult> GetCourseId(int moduleId)
        {
            if (moduleId <= 0)
                return BadRequest("courseId must be greater than zero.");

            var data = await _moduleService.GetModuleAndCourseIdAsync(moduleId);

            if (data == null)
                return NotFound(new { message = $"Module {moduleId} not found." });

            return Ok(data);
        }

    }
}
