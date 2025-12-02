// Gateway/Controllers/GatewayCourseController.cs
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace Gateway.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GatewayCourseController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "api/courses";

        public GatewayCourseController(IHttpClientFactory factory)
        {
            _httpClient = factory.CreateClient("CourseService");
        }

        private void ForwardAuth(HttpRequestMessage req)
        {
            if (Request.Headers.TryGetValue("Authorization", out var auth))
                req.Headers.TryAddWithoutValidation("Authorization", auth.ToString());
        }

        private async Task<IActionResult> Forward(HttpRequestMessage req)
        {
            ForwardAuth(req);

            var response = await _httpClient.SendAsync(req);
            var raw = await response.Content.ReadAsStringAsync();

            if (!raw.Trim().StartsWith("{") && !raw.Trim().StartsWith("["))
                return StatusCode((int)response.StatusCode, new { message = raw });

            return Content(raw, "application/json");
        }

        // ---------------- CRUD ----------------
        [HttpGet]
        public Task<IActionResult> GetAll() =>
            Forward(new HttpRequestMessage(HttpMethod.Get, BaseUrl));

        [HttpGet("{id:int}")]
        public Task<IActionResult> Get(int id) =>
            Forward(new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/{id}"));

        [HttpPost]
        public Task<IActionResult> Create([FromBody] object dto) =>
            Forward(new HttpRequestMessage(HttpMethod.Post, BaseUrl) { Content = JsonContent.Create(dto) });

        [HttpPut("{id:int}")]
        public Task<IActionResult> Update(int id, [FromBody] object dto) =>
            Forward(new HttpRequestMessage(HttpMethod.Put, $"{BaseUrl}/{id}") { Content = JsonContent.Create(dto) });

        [HttpDelete("{id:int}")]
        public Task<IActionResult> Delete(int id) =>
            Forward(new HttpRequestMessage(HttpMethod.Delete, $"{BaseUrl}/{id}"));

        // ---------------- Instructor Filter ----------------
        [HttpGet("instructor/{id:guid}")]
        public Task<IActionResult> GetByInstructor(Guid id) =>
            Forward(new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/instructor/{id}"));

        // ---------------- Enrollment ----------------
        [HttpPost("enroll")]
        public Task<IActionResult> Enroll([FromBody] object dto) =>
            Forward(new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/enroll") { Content = JsonContent.Create(dto) });

        // ---------------- My Learning (GET Enrolled Courses) ----------------
        [HttpGet("enrolled/{userId:guid}")]
        public Task<IActionResult> GetEnrolledCourses(Guid userId) =>
            Forward(new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/enrolled/{userId}"));

        // ---------------- Modules ----------------
        [HttpGet("{courseId:int}/modules")]
        public Task<IActionResult> GetModules(int courseId) =>
            Forward(new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/{courseId}/modules"));

        [HttpGet("module/{id:int}")]
        public Task<IActionResult> GetModule(int id) =>
            Forward(new HttpRequestMessage(HttpMethod.Get, $"api/modules/{id}"));

        // ---------------- Categories ----------------
        [HttpGet("categories")]
        public Task<IActionResult> GetCategories() =>
            Forward(new HttpRequestMessage(HttpMethod.Get, $"api/categories"));

        // ---------------- Publish + Unfinished workflow ----------------
        [HttpPut("publish/{courseId:int}")]
        public Task<IActionResult> PublishCourse(int courseId) =>
            Forward(new HttpRequestMessage(HttpMethod.Put, $"{BaseUrl}/publish/{courseId}"));

        [HttpPost("{courseId:int}/publish")]
        public Task<IActionResult> TryPublishCourse(int courseId) =>
            Forward(new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/{courseId}/publish"));

        [HttpGet("unfinished/{instructorId:guid}")]
        public Task<IActionResult> GetUnfinished(Guid instructorId) =>
            Forward(new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/unfinished/{instructorId}"));

        [HttpPost("continue/{courseId:int}")]
        public Task<IActionResult> Continue(int courseId) =>
            Forward(new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/continue/{courseId}"));
    }
}
