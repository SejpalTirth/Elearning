using DTOs._4AssessmentService;
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
        [ProducesResponseType(typeof(CreateQuizResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CreateQuizResponseDto>> CreateQuiz(
            [FromBody] CreateQuizDto dto)
        {
            var result = await _assessmentService.CreateQuizAsync(dto);
            return Ok(result);
        }


        [HttpPost("quiz/questions")]
        [ProducesResponseType(typeof(AddQuestionResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AddQuestionResponseDto>> AddQuestion(
    [FromBody] AddQuestionDto dto)
        {
            var result = await _assessmentService.AddQuestionAsync(dto);
            return Ok(result);
        }


        // ================== Student ==================
        [HttpPost("quiz/module")]
        [ProducesResponseType(typeof(QuizForModuleDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<QuizForModuleDto>> GetQuizForModule(
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
        [ProducesResponseType(typeof(QuizResultDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> SubmitQuiz(
            [FromBody] SubmitQuizDto dto)
        {
            var result = await _assessmentService.SubmitQuizAsync(dto);
            return Ok(result);
        }

        [HttpPost("quiz/result")]
        [ProducesResponseType(typeof(QuizResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        [ProducesResponseType(typeof(CourseQuizStatusResponseDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<CourseQuizStatusResponseDto>> GetQuizStatus(
    [FromBody] CourseQuizStatusDto dto)
        {
            var result = await _assessmentService
                .GetQuizStatusForCourseAsync(dto.CourseId);

            return Ok(result);
        }

        [HttpPost("course/unquizzed-modules")]
        [ProducesResponseType(typeof(List<int>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<int>>> GetUnquizzedModules(
            [FromBody] GetUnquizzedModulesDto dto)
        {
            var result = await _assessmentService
                .GetModulesWithoutQuizByCourseAsync(dto.CourseId);

            return Ok(result);
        }
    }
}
