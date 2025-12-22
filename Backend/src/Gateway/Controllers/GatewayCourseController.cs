using Gateway.Contracts.Course;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/course")]
    public class GatewayCourseController : BaseGatewayController
    {
        private const string BASE = "api/courses";

        public GatewayCourseController(IHttpClientFactory factory)
            : base(factory.CreateClient("CourseService"))
        {
        }

        // ------------------- COURSES -------------------

        [AllowAnonymous]
        [HttpPost("all")]
        public Task<IActionResult> GetAll() =>
            ForwardPost($"{BASE}/all", new { });

        [HttpPost("by-id")]
        public Task<IActionResult> GetById([FromBody] CourseIdRequest request) =>
            ForwardPost($"{BASE}/by-id", request);

        [HttpPost("create")]
        public Task<IActionResult> Create([FromBody] Course dto) =>
            ForwardPost($"{BASE}/create", dto);

        [HttpPost("update")]
        public Task<IActionResult> Update([FromBody] UpdateCourseRequest request) =>
            ForwardPost($"{BASE}/update", request);

        [HttpPost("delete")]
        public Task<IActionResult> Delete([FromBody] CourseIdRequest request) =>
            ForwardPost($"{BASE}/delete", request);

        // ------------------- INSTRUCTOR -------------------

        [HttpPost("instructor")]
        public Task<IActionResult> GetByInstructor() =>
            ForwardPost($"{BASE}/instructor", new { });

        [HttpPost("unfinished")]
        public Task<IActionResult> GetUnfinishedCourses() =>
            ForwardPost($"{BASE}/unfinished", new { });

        [HttpPost("continue")]
        public Task<IActionResult> Continue(
            [FromBody] ContinueCourseRequest request) =>
            ForwardPost($"{BASE}/continue", request);

        // ------------------- ENROLLMENT -------------------

        [HttpPost("enroll")]
        public Task<IActionResult> Enroll([FromBody] EnrollRequest dto) =>
            ForwardPost($"{BASE}/enroll", dto);

        [HttpPost("enrolled")]
        public Task<IActionResult> GetEnrolled() =>
            ForwardPost($"{BASE}/enrolled", new { });

        // ------------------- MODULES -------------------

        [HttpPost("modules")]
        public Task<IActionResult> GetModules(
            [FromBody] CourseIdRequest request) =>
            ForwardPost("api/modules/by-course", request);

        [HttpPost("module")]
        public Task<IActionResult> GetModule(
            [FromBody] ModuleIdRequest request) =>
            ForwardPost("api/modules/content", request);

        // ------------------- CATEGORIES -------------------

        [AllowAnonymous]
        [HttpPost("categories")]
        public Task<IActionResult> GetCategories() =>
            ForwardPost("api/categories", new { });

        // ------------------- PUBLISH / RESTORE -------------------

        [HttpPost("publish")]
        public Task<IActionResult> PublishCourse(
            [FromBody] CourseIdRequest request) =>
            ForwardPost($"{BASE}/publish", request);

        [HttpPost("restore")]
        public Task<IActionResult> Restore(
            [FromBody] CourseIdRequest request) =>
            ForwardPost($"{BASE}/restore", request);
    }
}
