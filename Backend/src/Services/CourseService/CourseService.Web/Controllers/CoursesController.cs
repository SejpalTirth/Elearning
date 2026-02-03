using CourseService.BLL.Interface;
using CourseService.BLL.UserContext;
using CourseService.DAL.Models;
using DTOs._2CourseService;
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

        public CoursesController(
            ICourseService courseService,
            IModuleService moduleService,
            IUserContextAccessor userContext)
        {
            _courseService = courseService;
            _moduleService = moduleService;
            _userContext = userContext;
        }

        // ---------------- COURSES BY INSTRUCTOR ----------------

        [HttpPost("instructor")]
        [ProducesResponseType(typeof(IEnumerable<Course>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByInstructor()
        {
            var userId = _userContext.Current?.UserId;

            if (userId == null || userId == Guid.Empty)
                return Unauthorized("Invalid user context.");

            var courses = await _courseService
                .GetCoursesByInstructorAsync(userId.Value);

            if(courses == null || !courses.Any())
                return NotFound("No courses found for the instructor.");

            return Ok(courses);
        }

        // ---------------- GET ALL COURSES ----------------

        [HttpPost("all")]
        [ProducesResponseType(typeof(IEnumerable<CourseResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _courseService.GetAllAsync();
            return Ok(result);
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
        [ProducesResponseType(typeof(Course), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CourseDto>> Create(
            [FromBody] CourseDto dto)
        {
            var created = await _courseService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = created.Id },
                created
            );
        }

        // ---------------- UPDATE COURSE ----------------

        [HttpPost("update")]
        [ProducesResponseType(typeof(Course), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(
            [FromBody] UpdateCourseRequestDto request)
        {
            var updated = await _courseService
                .UpdateAsync(request.CourseId, request.Course);

            return updated == null ? NotFound() : Ok(updated);
        }


        // ---------------- DELETE COURSE ----------------

        [HttpPost("delete")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(
            [FromBody] CourseIdRequestDto dto)
        {
            var deleted = await _courseService.DeleteAsync(dto.CourseId);
            return deleted ? Ok() : NotFound();
        }

        // ---------------- ENROLL ----------------

        [HttpPost("enroll")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Enroll([FromBody] EnrollRequestDto dto)
        {
            var user = _userContext.Current;

            if (user == null || user.UserId == Guid.Empty)
                return Unauthorized("Invalid user context.");

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
        [ProducesResponseType(typeof(IEnumerable<EnrolledCourseResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<EnrolledCourseResponseDto>>> GetEnrolledCourses()
        {
            var userId = _userContext.Current?.UserId;

            if (userId == null || userId == Guid.Empty)
                return Unauthorized("Invalid user context.");

            var courses = await _courseService
                .GetUserEnrolledCoursesAsync(userId.ToString());

            if (courses == null || !courses.Any())
                return NotFound();

            var response = courses.Select(c => new EnrolledCourseResponseDto
            {
                Id = c.Id,
                Title = c.Title
            });

            return Ok(response);
        }


        // ---------------- PUBLISH COURSE ----------------

        [HttpPost("publish")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
        [ProducesResponseType(typeof(IEnumerable<Course>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUnfinishedCourses()
        {
            var instructorId = _userContext.Current?.UserId;

            if (instructorId == null || instructorId == Guid.Empty)
                return Unauthorized("Invalid user context.");

            var result = await _courseService
                .GetAllUnfinishedCoursesAsync(instructorId.Value);
            
                return Ok(result);
        }

        // ---------------- CONTINUE COURSE ----------------

        [HttpPost("continue")]
        [ProducesResponseType(typeof(IEnumerable<Course>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ContinueCourse(
            [FromBody] ContinueCourseRequestDto dto)
        {
            var result = await _courseService
                .GetByIdAsync(dto.CourseId);
            if(result == null)
            {
                return NotFound("Course not found.");
            }
            
            return Ok(result);
        }

        // ---------------- RESTORE COURSE ----------------

        [HttpPost("restore")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Restore(
            [FromBody] RestoreCourseRequestDto dto)
        {
            var restored = await _courseService.RestoreAsync(dto.CourseId);
            return restored ? Ok() : NotFound();
        }
    }
}
