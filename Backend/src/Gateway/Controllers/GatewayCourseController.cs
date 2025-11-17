using Microsoft.AspNetCore.Mvc;

namespace Gateway.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GatewayCourseController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public GatewayCourseController(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        private const string BaseUrl = "http://localhost:5257/api/courses";

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            var response = await _httpClient.GetAsync(BaseUrl);

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Error calling the CourseService");

            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetById(int id)
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/{id}");

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Error calling the CourseService");

            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] object dto)
        {
            var response = await _httpClient.PostAsJsonAsync(BaseUrl, dto);

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Error calling the CourseService");

            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] object dto)
        {
            var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{id}", dto);

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Error calling the CourseService");

            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }

        [HttpPost("enroll")]
        public async Task<ActionResult> Enroll([FromBody] object dto)
        {
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/enroll", dto);

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Error calling the CourseService");

            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }
    }
}
