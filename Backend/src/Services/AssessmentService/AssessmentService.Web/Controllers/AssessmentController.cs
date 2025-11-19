using AssessmentService.BLL.DTOs;
using AssessmentService.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AssessmentService.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssessmentController : ControllerBase
    {
        private readonly IAssessmentService _assessmentService;

        public AssessmentController(IAssessmentService assessmentService)
        {
            _assessmentService = assessmentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllQuizzes()
        {
            var quizzes = await _assessmentService.GetAllQuizzesAsync();
            return Ok(quizzes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetQuizById(int id)
        {
            var quiz = await _assessmentService.GetQuizByIdAsync(id);
            if (quiz == null)
                return NotFound("Quiz not found");

            return Ok(quiz);
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitQuiz([FromBody] SubmitQuizDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid submission data");

            var result = await _assessmentService.SubmitQuizAsync(dto);
            return Ok(result);
        }
    }
}
