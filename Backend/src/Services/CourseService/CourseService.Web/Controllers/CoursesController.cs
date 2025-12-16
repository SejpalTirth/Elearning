using CourseService.BLL.DTOs;
using CourseService.BLL.Interface;
using Microsoft.AspNetCore.Mvc;

namespace CourseService.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseService _courseService;
        private readonly IModuleService _moduleService;
        private readonly IHttpClientFactory _httpClientFactory;

        public CoursesController(
            ICourseService courseService,
            IModuleService moduleService,
            IHttpClientFactory httpClientFactory)
        {
            _courseService = courseService;
            _moduleService = moduleService;
            _httpClientFactory = httpClientFactory;
        }

        // ---------------- GET COURSES BY INSTRUCTOR ----------------

        [HttpGet("instructor/{instructorId:guid}")]
        public async Task<IActionResult> GetByInstructor(Guid instructorId)
        {
            if (instructorId == Guid.Empty)
                return BadRequest("userId cannot be empty.");

            var courses = await _courseService.GetCoursesByInstructorAsync(instructorId);
            return Ok(courses);
        }

        // ---------------- GET ALL COURSES ----------------

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _courseService.GetAllAsync());
        }

        // ---------------- GET COURSE BY ID ----------------

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest("courseId must be greater than zero.");

            var course = await _courseService.GetByIdAsync(id);
            return course == null ? NotFound() : Ok(course);
        }

        // ---------------- CREATE COURSE ----------------

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CourseDto dto)
        {
            var created = await _courseService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // ---------------- UPDATE COURSE ----------------

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCourseDto dto)
        {
            if (id <= 0)
                return BadRequest("courseId must be greater than zero.");

            var updated = await _courseService.UpdateAsync(id, dto);
            return updated == null ? NotFound() : Ok(updated);
        }

        // ---------------- DELETE COURSE ----------------

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest("courseId must be greater than zero.");

            var deleted = await _courseService.DeleteAsync(id);
            return deleted ? Ok() : NotFound("The course cannot be found.");
        }

        // ---------------- ENROLL USER ----------------

        [HttpPost("enroll")]
        public async Task<IActionResult> Enroll([FromBody] EnrollRequestDto dto)
        {
            var success = await _courseService.EnrollUserAsync(dto);
            if (!success)
                return BadRequest("User already enrolled.");

            var course = await _courseService.GetByIdAsync(dto.CourseId);
            if (course == null)
                return Ok("Enrollment successful (course not found for email).");

            try
            {
                var userClient = _httpClientFactory.CreateClient("UserService");
                var notificationClient = _httpClientFactory.CreateClient("NotificationService");

                var user = await userClient
                    .GetFromJsonAsync<UserInfoDto>($"/api/users/{dto.UserId}");

                if (user == null || string.IsNullOrWhiteSpace(user.Email))
                    return Ok("Enrollment successful (user email not found).");

                var displayName = string.IsNullOrWhiteSpace(user.Name)
                    ? user.Email.Split('@')[0]
                    : user.Name;

                var triggerPayload = new
                {
                    userId = user.Id,
                    email = user.Email,
                    type = "Enrollment",
                    data = new Dictionary<string, string>
                    {
                        ["UserName"] = displayName,
                        ["CourseName"] = course.Title
                    }
                };

                await notificationClient
                    .PostAsJsonAsync("/api/notification/trigger", triggerPayload);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Enrollment Email Error] {ex.Message}");
            }

            return Ok("Enrollment successful.");
        }

        // ---------------- GET ENROLLED COURSES ----------------

        [HttpGet("enrolled/{userId:guid}")]
        public async Task<IActionResult> GetEnrolledCourses(Guid userId)
        {
            if (userId == Guid.Empty)
                return BadRequest("userId cannot be empty.");

            return Ok(await _courseService.GetUserEnrolledCoursesAsync(userId.ToString()));
        }

        // ---------------- MODULE ROUTES ----------------

        [HttpGet("{courseId:int}/modules")]
        public async Task<IActionResult> GetModulesForCourse(int courseId)
        {
            if (courseId <= 0)
                return BadRequest("courseId must be greater than zero.");

            return Ok(await _moduleService.GetModulesByCourseAsync(courseId));
        }

        // ---------------- COURSE PUBLISH ----------------

        [HttpPost("{courseId:int}/publish")]
        public async Task<IActionResult> PublishCourse(int courseId)
        {
            if (courseId <= 0)
                return BadRequest("courseId must be greater than zero.");

            var success = await _courseService.PublishCourseIfReadyAsync(courseId);
            if (!success)
                return BadRequest("All modules must have a quiz before publishing.");

            return Ok("Course published successfully.");
        }

        // ---------------- UNFINISHED COURSES ----------------

        [HttpGet("unfinished/{instructorId:guid}")]
        public async Task<IActionResult> GetUnfinishedCourses(Guid instructorId)
        {
            if (instructorId == Guid.Empty)
                return BadRequest("userId cannot be empty.");

            return Ok(await _courseService.GetAllUnfinishedCoursesAsync(instructorId));
        }

        // ---------------- CONTINUE COURSE ----------------

        [HttpPost("continue/{courseId:int}")]
        public async Task<IActionResult> ContinueCourse(int courseId)
        {
            if (courseId <= 0)
                return BadRequest("courseId must be greater than zero.");

            return Ok(await _courseService.ContinueUnfinishedCourseAsync(courseId));
        }

        // ---------------- RESTORE COURSE ----------------

        [HttpPut("{id:int}/restore")]
        public async Task<IActionResult> Restore(int id)
        {
            if (id <= 0)
                return BadRequest("courseId must be greater than zero.");

            var restored = await _courseService.RestoreAsync(id);
            return restored
                ? Ok("Course restored successfully.")
                : NotFound("Course not found.");
        }
    }
}
