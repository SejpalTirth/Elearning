using AssessmentService.BLL.DTOs;
using AssessmentService.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace AssessmentService.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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

        [HttpPost("quiz/{quizId:int}/questions")]
        public async Task<IActionResult> AddQuestion(int quizId, [FromBody] CreateQuestionDto dto)
        {
            if (quizId <= 0)
                return BadRequest("quizId must be greater than zero.");

            var result = await _assessmentService.AddQuestionAsync(quizId, dto);
            return Ok(result);
        }

        // ================== Student Endpoints ==================

        [HttpGet("module/{moduleId:int}")]
        public async Task<IActionResult> GetQuizForModule(int moduleId)
        {
            if (moduleId <= 0)
                return BadRequest("moduleId must be greater than zero.");

            if (!Request.Headers.TryGetValue("Authorization", out var tokenHeader))
                return Unauthorized("Missing access token.");

            if (!TryExtractUserId(tokenHeader.ToString(), out var userId))
                return Unauthorized("Invalid or missing user identifier in token.");

            var quiz = await _assessmentService.GetQuizForModuleAsync(moduleId, userId);
            return Ok(quiz);
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitQuiz([FromBody] SubmitQuizDto dto)
        {
            var result = await _assessmentService.SubmitQuizAsync(dto);
            return Ok(result);
        }

        [HttpGet("result/{submissionId:guid}")]
        public async Task<IActionResult> GetSubmissionResult(Guid submissionId)
        {
            if (submissionId == Guid.Empty)
                return BadRequest("submissionId cannot be empty.");

            var result = await _assessmentService.GetSubmissionResultAsync(submissionId);
            return result == null ? NotFound("Result not found.") : Ok(result);
        }

        [HttpGet("course/{courseId:int}/quiz-status")]
        public async Task<IActionResult> GetQuizStatus(int courseId)
        {
            if (courseId <= 0)
                return BadRequest("courseId must be greater than zero.");

            var result = await _assessmentService.GetQuizStatusForCourseAsync(courseId);
            return Ok(result);
        }

        [HttpGet("unquizzed-modules/{courseId:int}")]
        public async Task<IActionResult> GetModulesWithoutQuiz(int courseId)
        {
            if (courseId <= 0)
                return BadRequest("courseId must be greater than zero.");

            var result = await _assessmentService.GetModulesWithoutQuizByCourseAsync(courseId);
            return Ok(result);
        }

        [HttpGet("course-status/{courseId:int}")]
        public async Task<IActionResult> GetCourseQuizStatus(int courseId)
        {
            if (courseId <= 0)
                return BadRequest("courseId must be greater than zero.");

            var result = await _assessmentService.GetQuizStatusForCourseAsync(courseId);
            return Ok(result);
        }

        // ================== Helper ==================

        private bool TryExtractUserId(string authorizationHeader, out Guid userId)
        {
            userId = Guid.Empty;

            try
            {
                var token = authorizationHeader.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);

                var subClaim = jwt.Claims.FirstOrDefault(c => c.Type == "sub");
                if (subClaim == null)
                    return false;

                return Guid.TryParse(subClaim.Value, out userId);
            }
            catch
            {
                return false;
            }
        }
    }
}
