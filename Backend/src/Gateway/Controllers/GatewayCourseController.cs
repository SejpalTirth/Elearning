using Microsoft.AspNetCore.Mvc;
using static System.Net.WebRequestMethods;

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
        public async Task<IActionResult> Update(int id, [FromBody] object dto)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(dto);

            var req = new HttpRequestMessage(HttpMethod.Put, $"{BaseUrl}/{id}")
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
            };

            return await Forward(req);
        }



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


        // ---------------- Categories ----------------
        [HttpGet("categories")]
        public Task<IActionResult> GetCategories() =>
            Forward(new HttpRequestMessage(HttpMethod.Get, $"api/categories"));

        [HttpDelete("{id:int}")]
        public Task<IActionResult> Delete(int id) =>
        Forward(new HttpRequestMessage(HttpMethod.Delete, $"{BaseUrl}/{id}"));


        [HttpGet("module/{id}")]
        public Task<IActionResult> GetModule(int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/modules/{id}");
            return Forward(request);
        }



    }
}
