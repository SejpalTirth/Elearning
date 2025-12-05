using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace Gateway.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class GatewayCourseController : ControllerBase
    {
        private readonly HttpClient _http;
        private const string BASE = "api/courses";

        public GatewayCourseController(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("CourseService");
        }

        private void ForwardAuth(HttpRequestMessage req)
        {
            if (Request.Headers.TryGetValue("Authorization", out var token))
                req.Headers.TryAddWithoutValidation("Authorization", token.ToString());
        }

        private async Task<IActionResult> Forward(HttpRequestMessage req)
        {
            ForwardAuth(req);

            var res = await _http.SendAsync(req);
            var raw = await res.Content.ReadAsStringAsync();

            if (!raw.Trim().StartsWith("{") && !raw.Trim().StartsWith("["))
                return StatusCode((int)res.StatusCode, new { message = raw });

            return Content(raw, "application/json");
        }

        // ------------------- COURSE CRUD -------------------
        [AllowAnonymous]
        [HttpGet]
        public Task<IActionResult> GetAll() =>
            Forward(new HttpRequestMessage(HttpMethod.Get, BASE));

        [HttpGet("{id:int}")]
        public Task<IActionResult> GetById(int id) =>
            Forward(new HttpRequestMessage(HttpMethod.Get, $"{BASE}/{id}"));

        [HttpPost]
        public Task<IActionResult> Create([FromBody] object dto) =>
            Forward(new HttpRequestMessage(HttpMethod.Post, BASE)
            {
                Content = JsonContent.Create(dto)
            });

        [HttpPut("{id:int}")]
        public Task<IActionResult> Update(int id, [FromBody] object dto) =>
            Forward(new HttpRequestMessage(HttpMethod.Put, $"{BASE}/{id}")
            {
                Content = JsonContent.Create(dto)
            });

        [HttpDelete("{id:int}")]
        public Task<IActionResult> Delete(int id) =>
            Forward(new HttpRequestMessage(HttpMethod.Delete, $"{BASE}/{id}"));

        // ------------------- INSTRUCTOR ROUTES -------------------

        [HttpGet("instructor/{id:guid}")]
        public Task<IActionResult> GetByInstructor(Guid id) =>
            Forward(new HttpRequestMessage(HttpMethod.Get, $"{BASE}/instructor/{id}"));

        // Unfinished course (PENDING TASK)
        [HttpGet("unfinished/{instructorId:guid}")]
        public Task<IActionResult> GetUnfinishedCourse(Guid instructorId) =>
            Forward(new HttpRequestMessage(HttpMethod.Get, $"{BASE}/unfinished/{instructorId}"));

        // Continue editing (does NOT publish)
        [HttpPost("continue/{courseId:int}")]
        public Task<IActionResult> Continue(int courseId) =>
            Forward(new HttpRequestMessage(HttpMethod.Post, $"{BASE}/continue/{courseId}"));

        // ------------------- ENROLLMENT -------------------

        [HttpPost("enroll")]
        public Task<IActionResult> Enroll([FromBody] object dto) =>
            Forward(new HttpRequestMessage(HttpMethod.Post, $"{BASE}/enroll")
            {
                Content = JsonContent.Create(dto)
            });

        [HttpGet("enrolled/{userId}")]
        public Task<IActionResult> GetEnrolled(string userId) =>
            Forward(new HttpRequestMessage(HttpMethod.Get, $"{BASE}/enrolled/{userId}"));

        // ------------------- MODULE ROUTES -------------------

        [HttpGet("{courseId:int}/modules")]
        public Task<IActionResult> GetModules(int courseId) =>
            Forward(new HttpRequestMessage(HttpMethod.Get, $"{BASE}/{courseId}/modules"));

        [HttpGet("module/{id:int}")]
        public Task<IActionResult> GetModule(int id) =>
            Forward(new HttpRequestMessage(HttpMethod.Get, $"api/modules/{id}"));

        // ------------------- CATEGORIES -------------------

        [HttpGet("categories")]
        public Task<IActionResult> GetCategories() =>
            Forward(new HttpRequestMessage(HttpMethod.Get, $"api/categories"));

        // ------------------- PUBLISHING -------------------

        [HttpPost("{courseId:int}/publish")]
        public Task<IActionResult> PublishCourse(int courseId)
        {
            return Forward(
                new HttpRequestMessage(HttpMethod.Post, $"{BASE}/{courseId}/publish")
            );
        }

    }
}
