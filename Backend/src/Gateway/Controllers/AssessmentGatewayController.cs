using DTOs._4AssessmentService;
using Gateway.Contracts.Assessment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;

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
        [ProducesResponseType(typeof(QuizForModuleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetQuizForModule(
            [FromBody] GetQuizForModuleRequest request)
        {
            try
            {
                var result = await ForwardPost($"{BASE}/quiz/module", request);
                return result;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Gateway error",
                    details = ex.Message
                });
            }
        }


        // ------------------- QUIZ CREATION -------------------

        [HttpPost("quiz")]
        [ProducesResponseType(typeof(CreateQuizResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CreateQuizResponseDto>> CreateQuiz(
     [FromBody] CreateQuiz dto)
        {
            try
            {
                var result = await ForwardPost($"{BASE}/quiz", dto);

                if (result is OkObjectResult ok)
                {
                    return Ok(ok.Value);
                }

                return result as ActionResult;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Gateway error",
                    details = ex.Message
                });
            }
        }

        [HttpPost("quiz/questions")]
        [ProducesResponseType(typeof(AddQuestionResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AddQuestionResponseDto>> AddQuestion(
     [FromBody] AddQuestionRequest request)
        {
            try
            {
                var result = await ForwardPost($"{BASE}/quiz/questions", request);

                if (result is OkObjectResult ok)
                {
                    return Ok(ok.Value);
                }

                return result as ActionResult;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Gateway error",
                    details = ex.Message
                });
            }
        }
        // ------------------- QUIZ SUBMISSION -------------------

        [HttpPost("quiz/submit")]
        [ProducesResponseType(typeof(QuizResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Submit(
            [FromBody] SubmitQuiz dto)
        {
            try
            {
                var result = await ForwardPost($"{BASE}/quiz/submit", dto);
                return result;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Gateway error",
                    details = ex.Message
                });
            }
        }
            

        [HttpPost("quiz/result")]
        [ProducesResponseType(typeof(QuizResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<QuizResultDto>> Result(
            [FromBody] ResultRequest request)
        {
            try
            {
                var result = await ForwardPost($"{BASE}/quiz/result", request);
                if(result == null)
                {
                    return NotFound();
                }
                return result as ActionResult;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Gateway error",
                    details = ex.Message
                });
            }
        }

        // ------------------- COURSE QUIZ STATUS -------------------

        [HttpPost("course/quiz-status")]
        [ProducesResponseType(typeof(CourseQuizStatusResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CourseQuizStatusResponseDto>> QuizStatus(
    [FromBody] CourseQuizStatusRequest request)
        {
            try
            {
                var result = await ForwardPost($"{BASE}/course/quiz-status", request);

                if (result is OkObjectResult ok)
                {
                    return Ok(ok.Value);
                }

                return result as ActionResult;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Gateway error",
                    details = ex.Message
                });
            }
        }

        // ------------------- UNQUIZZED MODULES -------------------

        [HttpPost("course/unquizzed-modules")]
        [ProducesResponseType(typeof(List<int>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<int>>> GetUnquizzed(
            [FromBody] GetUnquizzedRequest request)
        {
            try
            {
                var result = await ForwardPost($"{BASE}/course/unquizzed-modules", request);
                if (result is OkObjectResult ok)
                {
                    return Ok(ok.Value);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Gateway error",
                    details = ex.Message
                });
            }
        }
    }
}
