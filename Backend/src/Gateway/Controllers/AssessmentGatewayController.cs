using Gateway.Contracts.Assessment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/assessment")]
    public class AssessmentGatewayController : BaseGatewayController
    {
        private const string BASE = "api/assessment";

        public AssessmentGatewayController(IHttpClientFactory factory)
            : base(factory.CreateClient("AssessmentService"))
        {
        }

        // ------------------- QUIZ FOR MODULE -------------------

        [HttpPost("quiz/module")]
        public Task<IActionResult> GetQuizForModule(
            [FromBody] GetQuizForModuleRequest request) =>
            ForwardPost($"{BASE}/quiz/module", request);

        // ------------------- QUIZ CREATION -------------------

        [HttpPost("quiz")]
        public Task<IActionResult> CreateQuiz(
            [FromBody] CreateQuiz dto) =>
            ForwardPost($"{BASE}/quiz", dto);

        [HttpPost("quiz/questions")]
        public Task<IActionResult> AddQuestion(
            [FromBody] AddQuestionRequest request) =>
            ForwardPost($"{BASE}/quiz/questions", request);

        // ------------------- QUIZ SUBMISSION -------------------

        [HttpPost("quiz/submit")]
        public Task<IActionResult> Submit(
            [FromBody] SubmitQuiz dto) =>
            ForwardPost($"{BASE}/quiz/submit", dto);

        [HttpPost("quiz/result")]
        public Task<IActionResult> Result(
            [FromBody] ResultRequest request) =>
            ForwardPost($"{BASE}/quiz/result", request);

        // ------------------- COURSE QUIZ STATUS -------------------

        [HttpPost("course/quiz-status")]
        public Task<IActionResult> QuizStatus(
            [FromBody] CourseQuizStatusRequest request) =>
            ForwardPost($"{BASE}/course/quiz-status", request);

        // ------------------- UNQUIZZED MODULES -------------------

        [HttpPost("course/unquizzed-modules")]
        public Task<IActionResult> GetUnquizzed(
            [FromBody] GetUnquizzedRequest request) =>
            ForwardPost($"{BASE}/course/unquizzed-modules", request);
    }
}
