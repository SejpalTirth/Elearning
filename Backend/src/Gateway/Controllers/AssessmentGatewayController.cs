using Gateway.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace Gateway.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssessmentGatewayController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AssessmentGatewayController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("quizzes")]
        public async Task<IActionResult> GetAllQuizzes()
        {
            var client = _httpClientFactory.CreateClient("AssessmentService");

            var response = await client.GetAsync("api/assessment");

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());

            var quizzes = await response.Content.ReadFromJsonAsync<List<QuizDto>>();
            return Ok(quizzes);
        }

        [HttpGet("quizzes/{id}")]
        public async Task<IActionResult> GetQuizById(int id)
        {
            var client = _httpClientFactory.CreateClient("AssessmentService");

            var response = await client.GetAsync($"api/assessment/{id}");

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());

            var quiz = await response.Content.ReadFromJsonAsync<QuizDto>();
            return Ok(quiz);
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitQuiz([FromBody] SubmitQuizDto dto)
        {
            var client = _httpClientFactory.CreateClient("AssessmentService");

            var response = await client.PostAsJsonAsync("api/assessment/submit", dto);

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());

            var result = await response.Content.ReadFromJsonAsync<SubmissionResultDto>();
            return Ok(result);
        }
    }
}
