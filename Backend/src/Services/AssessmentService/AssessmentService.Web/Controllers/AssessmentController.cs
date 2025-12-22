using AssessmentService.BLL.DTOs;
using AssessmentService.BLL.Interfaces;
using AssessmentService.BLL.UserContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssessmentService.Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/assessment")]
    public class AssessmentController : ControllerBase
    {
        private readonly IAssessmentService _assessmentService;
        private readonly IUserContextAccessor _userContext;

        public AssessmentController(
            IAssessmentService assessmentService,
            IUserContextAccessor userContext)
        {
            _assessmentService = assessmentService;
            _userContext = userContext;
        }

        [HttpPost("debug/claims")]
        public IActionResult DebugClaims()
        {
            return Ok(User.Claims.Select(c => new
            {
                c.Type,
                c.Value
            }));
        }



        // ================== Instructor ==================

        [HttpPost("quiz")]
        public async Task<IActionResult> CreateQuiz(
            [FromBody] CreateQuizDto dto)
        {
            var result = await _assessmentService.CreateQuizAsync(dto);
            return Ok(result);
        }

        [HttpPost("quiz/questions")]
        public async Task<IActionResult> AddQuestion(
            [FromBody] AddQuestionDto dto)
        {
            var result = await _assessmentService.AddQuestionAsync(dto);
            return Ok(result);
        }

        // ================== Student ==================
        [HttpPost("quiz/module")]
        public async Task<IActionResult> GetQuizForModule(
            [FromBody] GetQuizForModuleDto dto)
        {
            var userId = _userContext.Current?.UserId;

            if (userId == null || userId == Guid.Empty)
                return Unauthorized("Invalid user context.");

            var quiz = await _assessmentService
                .GetQuizForModuleAsync(dto.ModuleId, userId.Value);

            return Ok(quiz);
        }


        [HttpPost("quiz/submit")]
        public async Task<IActionResult> SubmitQuiz(
            [FromBody] SubmitQuizDto dto)
        {
            var result = await _assessmentService.SubmitQuizAsync(dto);
            return Ok(result);
        }

        [HttpPost("quiz/result")]
        public async Task<IActionResult> GetSubmissionResult(
            [FromBody] SubmissionResultRequestDto dto)
        {
            var result = await _assessmentService
                .GetSubmissionResultAsync(dto.SubmissionId);

            return result == null
                ? NotFound("Result not found.")
                : Ok(result);
        }

        [HttpPost("course/quiz-status")]
        public async Task<IActionResult> GetQuizStatus(
            [FromBody] CourseQuizStatusDto dto)
        {
            var result = await _assessmentService
                .GetQuizStatusForCourseAsync(dto.CourseId);

            return Ok(result);
        }

        [HttpPost("course/unquizzed-modules")]
        public async Task<IActionResult> GetUnquizzedModules(
            [FromBody] GetUnquizzedModulesDto dto)
        {
            var result = await _assessmentService
                .GetModulesWithoutQuizByCourseAsync(dto.CourseId);

            return Ok(result);
        }
    }
}
