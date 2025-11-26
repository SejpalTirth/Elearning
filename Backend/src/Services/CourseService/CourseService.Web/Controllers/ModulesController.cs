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
        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetByCourse(int courseId)
        {
            var modules = await _moduleService.GetModulesByCourseAsync(courseId);
            return Ok(modules);
        }

        // GET /api/modules/10
        [HttpGet("{moduleId}")]
        public async Task<IActionResult> GetContent(int moduleId)
        {
            var module = await _moduleService.GetModuleContentAsync(moduleId);

            if (module == null)
                return NotFound();

            return Ok(module);
        }

        [HttpGet("course-id/{moduleId}")]
        public async Task<IActionResult> GetCourseId(int moduleId)
        {
            var data = await _moduleService.GetModuleAndCourseIdAsync(moduleId);

            if (data == null)
                return NotFound(new { message = $"Module {moduleId} not found." });

            return Ok(data);
        }

    }
}
