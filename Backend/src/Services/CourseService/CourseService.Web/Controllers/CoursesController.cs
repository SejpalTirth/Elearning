using CourseService.BLL.DTOs;
using CourseService.BLL.Interface;
using CourseService.BLL.UserContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseService.Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/courses")]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseService _courseService;
        private readonly IModuleService _moduleService;
        private readonly IUserContextAccessor _userContext;
        private readonly string invalid_user_context = "Invalid user context.";

        public CoursesController(
            ICourseService courseService,
            IModuleService moduleService,
            IHttpClientFactory httpClientFactory,
            IUserContextAccessor userContext)
        {
            _courseService = courseService;
            _moduleService = moduleService;
            _userContext = userContext;
        }

        // ---------------- COURSES BY INSTRUCTOR ----------------

        [HttpPost("instructor")]
        public async Task<IActionResult> GetByInstructor()
        {
            var userId = _userContext.Current?.UserId;

            if (userId == null || userId == Guid.Empty)
                return Unauthorized(invalid_user_context);

            return Ok(await _courseService
                .GetCoursesByInstructorAsync(userId.Value));
        }

        // ---------------- GET ALL COURSES ----------------

        [HttpPost("all")]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _courseService.GetAllAsync());
        }

        // ---------------- COURSE BY ID ----------------

        [HttpPost("by-id")]
        public async Task<IActionResult> GetById(
            [FromBody] CourseIdRequestDto dto)
        {
            var course = await _courseService.GetByIdAsync(dto.CourseId);
            return course == null ? NotFound() : Ok(course);
        }

        // ---------------- CREATE COURSE ----------------

        [HttpPost("create")]
        public async Task<IActionResult> Create(
            [FromBody] CourseDto dto)
        {
            var created = await _courseService.CreateAsync(dto);
            return Ok(created);
        }

        // ---------------- UPDATE COURSE ----------------

        [HttpPost("update")]
        public async Task<IActionResult> Update(
            [FromBody] UpdateCourseRequestDto request)
        {
            var updated = await _courseService
                .UpdateAsync(request.CourseId, request.Course);

            return updated == null ? NotFound() : Ok(updated);
        }


        // ---------------- DELETE COURSE ----------------

        [HttpPost("delete")]
        public async Task<IActionResult> Delete(
            [FromBody] CourseIdRequestDto dto)
        {
            var deleted = await _courseService.DeleteAsync(dto.CourseId);
            return deleted ? Ok() : NotFound();
        }

        // ---------------- ENROLL ----------------

        [HttpPost("enroll")]
        public async Task<IActionResult> Enroll([FromBody] EnrollRequestDto dto)
        {
            var user = _userContext.Current;

            if (user == null || user.UserId == Guid.Empty)
                return Unauthorized(invalid_user_context);

            var authHeader = HttpContext.Request.Headers["Authorization"].ToString();

            var success = await _courseService.EnrollUserAsync(
                user.UserId,
                dto.CourseId,
                user.Email,
                authHeader
            );

            return success
                ? Ok(new { success = true })
                : BadRequest(new { success = false });
        }



        // ---------------- ENROLLED COURSES ----------------

        [HttpPost("enrolled")]
        public async Task<IActionResult> GetEnrolledCourses()
        {
            var userId = _userContext.Current?.UserId;

            if (userId == null || userId == Guid.Empty)
                return Unauthorized(invalid_user_context);

            return Ok(await _courseService
                .GetUserEnrolledCoursesAsync(userId.ToString()));
        }

        // ---------------- MODULES ----------------

        [HttpPost("modules")]
        public async Task<IActionResult> GetModulesForCourse(
            [FromBody] CourseIdRequestDto dto)
        {
            return Ok(await _moduleService
                .GetModulesByCourseAsync(dto.CourseId));
        }

        // ---------------- PUBLISH COURSE ----------------

        [HttpPost("publish")]
        public async Task<IActionResult> PublishCourse(
            [FromBody] CourseIdRequestDto dto)
        {
            var authHeader = HttpContext.Request.Headers["Authorization"].ToString();

            var success = await _courseService
                .PublishCourseIfReadyAsync(dto.CourseId, authHeader);

            return success
                ? Ok(new {message =  "Course published successfully."})
                : BadRequest(new { message = "All modules must have quizzes." });
        }

        // ---------------- UNFINISHED COURSES ----------------

        [HttpPost("unfinished")]
        public async Task<IActionResult> GetUnfinishedCourses()
        {
            var instructorId = _userContext.Current?.UserId;

            if (instructorId == null || instructorId == Guid.Empty)
                return Unauthorized(invalid_user_context);

            return Ok(await _courseService
                .GetAllUnfinishedCoursesAsync(instructorId.Value));
        }

        // ---------------- CONTINUE COURSE ----------------

        [HttpPost("continue")]
        public async Task<IActionResult> ContinueCourse(
            [FromBody] ContinueCourseRequestDto dto)
        {
            return Ok(await _courseService
                .ContinueUnfinishedCourseAsync(dto.CourseId));
        }

        // ---------------- RESTORE COURSE ----------------

        [HttpPost("restore")]
        public async Task<IActionResult> Restore(
            [FromBody] RestoreCourseRequestDto dto)
        {
            var restored = await _courseService.RestoreAsync(dto.CourseId);
            return restored ? Ok() : NotFound();
        }
    }
}
