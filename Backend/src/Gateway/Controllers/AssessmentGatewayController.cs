// Gateway/Controllers/AssessmentGatewayController.cs
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace Gateway.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssessmentGatewayController : ControllerBase
    {
        private readonly IHttpClientFactory _factory;
        private const string BASE = "api/assessment";

        public AssessmentGatewayController(IHttpClientFactory factory)
        {
            _factory = factory;
        }

        private void ForwardAuth(HttpRequestMessage req)
        {
            if (Request.Headers.TryGetValue("Authorization", out var auth))
                req.Headers.TryAddWithoutValidation("Authorization", auth.ToString());
        }

        private async Task<IActionResult> Forward(HttpRequestMessage req)
        {
            var client = _factory.CreateClient("AssessmentService");
            ForwardAuth(req);

            var response = await client.SendAsync(req);
            var raw = await response.Content.ReadAsStringAsync();

            if (!raw.Trim().StartsWith("{") && !raw.Trim().StartsWith("["))
                return StatusCode((int)response.StatusCode, new { message = raw });

            return Content(raw, "application/json");
        }

        // Student: Quiz for module (requires forwarded auth)
        [HttpGet("quiz/module/{moduleId:int}")]
        public async Task<IActionResult> GetQuizForModule(int moduleId)
        {
            var client = _factory.CreateClient("AssessmentService");

            if (!Request.Headers.TryGetValue("Authorization", out var token))
                return Unauthorized("Missing access token.");

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",
                    token.ToString().Replace("Bearer ", ""));

            var response = await client.GetAsync($"{BASE}/module/{moduleId}");
            var raw = await response.Content.ReadAsStringAsync();

            return Content(raw, "application/json");
        }

        [HttpPost("submit")]
        public Task<IActionResult> SubmitQuiz([FromBody] object dto) =>
            Forward(new HttpRequestMessage(HttpMethod.Post, $"{BASE}/submit") { Content = JsonContent.Create(dto) });

        [HttpGet("result/{submissionId:guid}")]
        public Task<IActionResult> GetResult(Guid submissionId) =>
            Forward(new HttpRequestMessage(HttpMethod.Get, $"{BASE}/result/{submissionId}"));

        [HttpPost("quiz")]
        public Task<IActionResult> CreateQuiz([FromBody] object dto) =>
            Forward(new HttpRequestMessage(HttpMethod.Post, $"{BASE}/quiz") { Content = JsonContent.Create(dto) });

        [HttpPost("quiz/{quizId:int}/questions")]
        public Task<IActionResult> AddQuestion(int quizId, [FromBody] object dto) =>
            Forward(new HttpRequestMessage(HttpMethod.Post, $"{BASE}/quiz/{quizId}/questions") { Content = JsonContent.Create(dto) });

        [HttpGet("course/{courseId}/quiz-status")]
        public Task<IActionResult> GetQuizStatus(int courseId) =>
            Forward(new HttpRequestMessage(HttpMethod.Get, $"{BASE}/course/{courseId}/quiz-status"));

        [HttpGet("unquizzed-modules/{courseId:int}")]
        public Task<IActionResult> GetUnquizzed(int courseId) =>
            Forward(new HttpRequestMessage(HttpMethod.Get, $"{BASE}/unquizzed-modules/{courseId}"));
    }
}
