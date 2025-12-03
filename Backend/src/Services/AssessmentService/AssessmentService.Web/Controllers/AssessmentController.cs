using AssessmentService.BLL.DTOs;
using AssessmentService.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

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

        // ================== Instructor Endpoints ==================

        [HttpPost("quiz")]
        public async Task<IActionResult> CreateQuiz([FromBody] CreateQuizDto dto)
        {
            var result = await _assessmentService.CreateQuizAsync(dto);
            return Ok(result);
        }

        [HttpPost("quiz/{quizId}/questions")]
        public async Task<IActionResult> AddQuestion(int quizId, [FromBody] CreateQuestionDto dto)
        {
            var result = await _assessmentService.AddQuestionAsync(quizId, dto);
            return Ok(result);
        }

        // ================== Student Endpoints ==================

        [HttpGet("module/{moduleId:int}")]
        public async Task<IActionResult> GetQuizForModule(int moduleId)
        {
            if (!Request.Headers.TryGetValue("Authorization", out var tokenHeader))
                return Unauthorized("Missing access token.");

            try
            {
                var token = tokenHeader.ToString().Replace("Bearer ", "");
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);

                var userClaim = jwt.Claims.FirstOrDefault(c => c.Type == "sub");
                if (userClaim == null)
                    return Unauthorized("Token missing required 'sub' claim.");

                Guid userId = Guid.Parse(userClaim.Value);

                var quiz = await _assessmentService.GetQuizForModuleAsync(moduleId, userId);
                return Ok(quiz);
            }
            catch
            {
                return Unauthorized("Invalid token format");
            }
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitQuiz([FromBody] SubmitQuizDto dto)
        {
            var result = await _assessmentService.SubmitQuizAsync(dto);
            return Ok(result);
        }

        // Keep only ONE version of this endpoint
        [HttpGet("result/{submissionId:guid}")]
        public async Task<IActionResult> GetSubmissionResult(Guid submissionId)
        {
            var result = await _assessmentService.GetSubmissionResultAsync(submissionId);

            if (result == null)
                return NotFound("Result not found.");

            return Ok(result);
        }

        [HttpGet("course/{courseId}/quiz-status")]
        public async Task<IActionResult> GetQuizStatus(int courseId)
        {
            var result = await _assessmentService.GetQuizStatusForCourseAsync(courseId);
            return Ok(result);
        }

        [HttpGet("unquizzed-modules/{courseId:int}")]
        public async Task<IActionResult> GetModulesWithoutQuiz(int courseId)
        {
            var result = await _assessmentService.GetModulesWithoutQuizByCourseAsync(courseId);
            return Ok(result);
        }

        [HttpGet("course-status/{courseId:int}")]
        public async Task<IActionResult> GetCourseQuizStatus(int courseId)
        {
            var result = await _assessmentService.GetQuizStatusForCourseAsync(courseId);
            return Ok(result);
        }


    }
}
