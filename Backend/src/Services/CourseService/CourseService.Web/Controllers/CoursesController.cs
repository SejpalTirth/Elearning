using CourseService.BLL.DTOs;
using CourseService.BLL.Interface;
using Microsoft.AspNetCore.Mvc;

namespace CourseService.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseService _courseService;
        private readonly IModuleService _moduleService;

        public CoursesController(ICourseService courseService, IModuleService moduleService)
        {
            _courseService = courseService;
            _moduleService = moduleService;
        }

        // ---------------- GET COURSES BY INSTRUCTOR ----------------
        [HttpGet("instructor/{instructorId:guid}")]
        public async Task<IActionResult> GetByInstructor(Guid instructorId)
        {
            var courses = await _courseService.GetCoursesByInstructorAsync(instructorId);
            return Ok(courses);
        }

        // ---------------- GET ALL ----------------
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _courseService.GetAllAsync());
        }

        // ---------------- GET COURSE BY ID ----------------
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var course = await _courseService.GetByIdAsync(id);
            return course == null ? NotFound() : Ok(course);
        }

        // ---------------- CREATE COURSE ----------------
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CourseDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(dto.InstructorUserId))
                return BadRequest(new { message = "InstructorUserId is required." });

            var created = await _courseService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // ---------------- UPDATE COURSE ----------------
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCourseDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _courseService.UpdateAsync(id, dto);
            return updated == null ? NotFound() : Ok(updated);
        }

        // ---------------- ENROLL (with email trigger) ----------------
        [HttpPost("enroll")]
        public async Task<IActionResult> Enroll([FromBody] EnrollRequestDto dto)
        {
            // 1) Enroll user in course (existing logic)
            bool success = await _courseService.EnrollUserAsync(dto);
            if (!success)
                return BadRequest("User already enrolled.");

            // 2) Load course details
            var course = await _courseService.GetByIdAsync(dto.CourseId);
            if (course == null)
                return Ok("Enrollment successful (course not found for email).");

            try
            {
                // 3) Call UserService to get user info
                using var userClient = new HttpClient
                {
                    BaseAddress = new Uri("https://localhost:7130")
                };

                var user = await userClient.GetFromJsonAsync<UserInfo>($"/api/users/{dto.UserId}");
                if (user == null || string.IsNullOrWhiteSpace(user.Email))
                {
                    return Ok("Enrollment successful (user email not found).");
                }

                var displayName = string.IsNullOrWhiteSpace(user.Name)
                    ? user.Email.Split('@')[0]
                    : user.Name;

                // 4) Build notification trigger payload
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

                // 5) Call NotificationService
                using var notificationClient = new HttpClient
                {
                    BaseAddress = new Uri("https://localhost:7245")
                };

                await notificationClient.PostAsJsonAsync("/api/notification/trigger", triggerPayload);
            }
            catch (Exception ex)
            {
                // Do not fail enrollment if email fails
                Console.WriteLine($"[Enrollment Email Error] {ex.Message}");
            }

            return Ok("Enrollment successful.");
        }

        // ---------------- GET ENROLLED COURSES ----------------
        [HttpGet("enrolled/{userId}")]
        public async Task<IActionResult> GetEnrolledCourses(string userId)
        {
            return Ok(await _courseService.GetUserEnrolledCoursesAsync(userId));
        }

        // ---------------- MODULE ROUTES ----------------
        [HttpGet("{courseId:int}/modules")]
        public async Task<IActionResult> GetModulesForCourse(int courseId)
        {
            return Ok(await _moduleService.GetModulesByCourseAsync(courseId));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _courseService.DeleteAsync(id);
            return deleted ? NoContent() : NotFound();
        }

        [HttpPut("publish/{courseId:int}")]
        public async Task<IActionResult> PublishCourse(int courseId)
        {
            bool success = await _courseService.PublishCourseIfReadyAsync(courseId);
            if (!success)
                return BadRequest("All modules must have a quiz before publishing.");

            return Ok("Course published successfully.");
        }

        // POST /api/courses/{id}/publish
        [HttpPost("{id:int}/publish")]
        public async Task<IActionResult> TryPublishCourse(int id)
        {
            var result = await _courseService.PublishCourseIfReadyAsync(id);
            return Ok(new { published = result });
        }

        // GET: api/courses/instructor/unpublished/{instructorUserId}
        [HttpGet("instructor/unpublished/{instructorUserId}")]
        public async Task<IActionResult> GetUnpublishedCourse(Guid instructorUserId)
        {
            var course = await _courseService.GetUnpublishedCourseAsync(instructorUserId);
            if (course == null)
                return Ok(null);

            return Ok(new { courseId = course.Id });
        }

        // ================== GET UNFINISHED COURSE FOR INSTRUCTOR ==================
        [HttpGet("unfinished/{instructorId:guid}")]
        public async Task<IActionResult> GetUnfinishedCourse(Guid instructorId)
        {
            var course = await _courseService.GetUnfinishedCourseAsync(instructorId);
            if (course == null)
                return NotFound();

            return Ok(course);
        }

        // ================== CONTINUE COURSE (UNDELETE) ==================
        [HttpPost("continue/{courseId:int}")]
        public async Task<IActionResult> ContinueCourse(int courseId)
        {
            var result = await _courseService.ContinueUnfinishedCourseAsync(courseId);
            return Ok(result);
        }


        // Local helper type for reading UserService response
        private class UserInfo
        {
            public Guid Id { get; set; }
            public string Email { get; set; } = string.Empty;
            public string? Name { get; set; }
        }
    }
}
