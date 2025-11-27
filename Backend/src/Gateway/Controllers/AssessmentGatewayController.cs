using Microsoft.AspNetCore.Mvc;

namespace Gateway.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssessmentGatewayController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private const string BaseUrl = "api/assessment";

        public AssessmentGatewayController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private void ForwardAuth(HttpRequestMessage request)
        {
            if (Request.Headers.TryGetValue("Authorization", out var auth))
            {
                request.Headers.TryAddWithoutValidation("Authorization", auth.ToString());
            }
        }

        private async Task<IActionResult> Forward(HttpRequestMessage request)
        {
            var client = _httpClientFactory.CreateClient("AssessmentService");
            ForwardAuth(request);

            var response = await client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!content.StartsWith("{") && !content.StartsWith("["))
                return StatusCode((int)response.StatusCode, new { message = content });

            return Content(content, "application/json");
        }


        // ========================
        // GET: Quiz for module
        // ========================
        [HttpGet("quiz/module/{moduleId:int}")]
        public async Task<IActionResult> GetQuizForModule(int moduleId)
        {
            var client = _httpClientFactory.CreateClient("AssessmentService");

            if (!Request.Headers.TryGetValue("Authorization", out var token))
                return Unauthorized("Missing access token.");

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",
                token.ToString().Replace("Bearer ", ""));

            var response = await client.GetAsync($"api/assessment/module/{moduleId}");
            var raw = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, raw);

            return Content(raw, "application/json");
        }


        // ========================
        // Submit Quiz
        // ========================
        [HttpPost("submit")]
        public Task<IActionResult> SubmitQuiz([FromBody] object dto) =>
            Forward(new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/submit")
            {
                Content = JsonContent.Create(dto)
            });


        // ========================
        // Get Result
        // ========================
        [HttpGet("result/{submissionId:guid}")]
        public async Task<IActionResult> GetSubmissionResult(Guid submissionId)
        {
            var client = _httpClientFactory.CreateClient("AssessmentService");

            var response = await client.GetAsync($"api/assessment/result/{submissionId}");
            var result = await response.Content.ReadAsStringAsync();

            return Content(result, "application/json");
        }

        // ---------------------------
        // CREATE QUIZ
        // ---------------------------
        [HttpPost("quiz")]
        public Task<IActionResult> CreateQuiz([FromBody] object dto) =>
            Forward(new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/quiz")
            {
                Content = JsonContent.Create(dto)
            });

        // ---------------------------
        // ADD QUESTION TO QUIZ
        // ---------------------------
        [HttpPost("quiz/{quizId:int}/questions")]
        public Task<IActionResult> AddQuestion(int quizId, [FromBody] object dto)
        {
            return Forward(new HttpRequestMessage(
                HttpMethod.Post, $"{BaseUrl}/quiz/{quizId}/questions")
            {
                Content = JsonContent.Create(dto)
            });
        }


    }
}
