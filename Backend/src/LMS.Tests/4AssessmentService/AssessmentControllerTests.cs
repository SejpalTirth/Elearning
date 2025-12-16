using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AssessmentService.BLL.DTOs;
using AssessmentService.BLL.Interfaces;
using AssessmentService.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace LMS.Tests.AssessmentService
{
    public class AssessmentControllerTests
    {
        private readonly Mock<IAssessmentService> _serviceMock;
        private readonly AssessmentController _controller;

        public AssessmentControllerTests()
        {
            _serviceMock = new Mock<IAssessmentService>(MockBehavior.Strict);

            _controller = new AssessmentController(_serviceMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        // -------------------------------------------------
        // CREATE QUIZ
        // -------------------------------------------------
        [Fact]
        public async Task CreateQuiz_ReturnsOk_WithResult()
        {
            var dto = new CreateQuizDto { ModuleId = 1, Title = "Test Quiz" };

            _serviceMock
                .Setup(s => s.CreateQuizAsync(dto))
                .ReturnsAsync(new { quizId = 99 });

            var result = await _controller.CreateQuiz(dto) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(99, result!.Value!.GetType().GetProperty("quizId")!.GetValue(result.Value));
        }

        // -------------------------------------------------
        // ADD QUESTION
        // -------------------------------------------------
        [Fact]
        public async Task AddQuestion_ReturnsOk()
        {
            var dto = new CreateQuestionDto
            {
                Question = "Q1?",
                Marks = 1,
                Options = new() { "A", "B" },
                CorrectAnswerIndex = 0
            };

            _serviceMock
                .Setup(s => s.AddQuestionAsync(10, dto))
                .ReturnsAsync(new { success = true });

            var result = await _controller.AddQuestion(10, dto) as OkObjectResult;

            Assert.NotNull(result);
        }

        // -------------------------------------------------
        // GET QUIZ FOR MODULE — INVALID AUTH
        // -------------------------------------------------
        [Fact]
        public async Task GetQuizForModule_ShouldReturnUnauthorized_WhenMissingAuthorization()
        {
            var result = await _controller.GetQuizForModule(20);

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Missing access token.", unauthorized.Value);
        }

        [Fact]
        public async Task GetQuizForModule_ShouldReturnUnauthorized_WhenTokenInvalid()
        {
            _controller.HttpContext.Request.Headers["Authorization"] = "Bearer BAD_TOKEN";

            var result = await _controller.GetQuizForModule(20);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task GetQuizForModule_ShouldReturnUnauthorized_WhenSubMissing()
        {
            var token = new JwtSecurityToken(claims: new[] { new Claim("name", "test") });
            string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            _controller.HttpContext.Request.Headers["Authorization"] = $"Bearer {tokenString}";

            var result = await _controller.GetQuizForModule(20);

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Token missing required 'sub' claim.", unauthorized.Value);
        }

        [Fact]
        public async Task GetQuizForModule_ShouldReturnOk_WhenTokenValid()
        {
            Guid userId = Guid.NewGuid();
            var token = new JwtSecurityToken(claims: new[] { new Claim("sub", userId.ToString()) });
            string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            _controller.HttpContext.Request.Headers["Authorization"] = $"Bearer {tokenString}";

            var quizDto = new QuizForModuleDto
            {
                QuizId = 1,
                ModuleId = 20,
                Title = "Test",
                TotalMarks = 10,
                AlreadyPassed = false
            };

            _serviceMock
                .Setup(s => s.GetQuizForModuleAsync(20, userId))
                .ReturnsAsync(quizDto);

            var result = await _controller.GetQuizForModule(20);

            Assert.IsType<OkObjectResult>(result);
        }

        // -------------------------------------------------
        // SUBMIT QUIZ
        // -------------------------------------------------
        [Fact]
        public async Task SubmitQuiz_ReturnsOk()
        {
            var dto = new SubmitQuizDto { QuizId = 1, UserId = Guid.NewGuid(), Answers = new() };

            var resultDto = new QuizResultDto
            {
                TotalMarks = 10,
                ObtainedMarks = 5,
                Percentage = 50,
                Passed = false,
                AlreadyPassed = false,
                StatusMessage = "Try again."
            };

            _serviceMock
                .Setup(s => s.SubmitQuizAsync(dto))
                .ReturnsAsync(resultDto);

            var result = await _controller.SubmitQuiz(dto);

            Assert.IsType<OkObjectResult>(result);
        }

        // -------------------------------------------------
        // SUBMISSION RESULT
        // -------------------------------------------------
        [Fact]
        public async Task GetSubmissionResult_ShouldReturnNotFound_WhenMissing()
        {
            _serviceMock
                .Setup(s => s.GetSubmissionResultAsync(It.IsAny<Guid>()))
                .ReturnsAsync((QuizResultDto?)null);

            var result = await _controller.GetSubmissionResult(Guid.NewGuid());

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetSubmissionResult_ShouldReturnOk_WhenExists()
        {
            var resultDto = new QuizResultDto
            {
                TotalMarks = 10,
                ObtainedMarks = 8,
                Percentage = 80,
                Passed = true,
                AlreadyPassed = false,
                StatusMessage = "Passed!"
            };

            _serviceMock
                .Setup(s => s.GetSubmissionResultAsync(It.IsAny<Guid>()))
                .ReturnsAsync(resultDto);

            var result = await _controller.GetSubmissionResult(Guid.NewGuid());

            Assert.IsType<OkObjectResult>(result);
        }

        // -------------------------------------------------
        // ⭐ ADDED TESTS (MISSING BEFORE)
        // -------------------------------------------------

        [Fact]
        public async Task GetQuizStatus_ShouldReturnOk()
        {
            _serviceMock
                .Setup(s => s.GetQuizStatusForCourseAsync(5))
                .ReturnsAsync(new { ok = true });

            var result = await _controller.GetQuizStatus(5);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetModulesWithoutQuiz_ShouldReturnOk()
        {
            _serviceMock
                .Setup(s => s.GetModulesWithoutQuizByCourseAsync(3))
                .ReturnsAsync(new List<int> { 1, 2 });

            var result = await _controller.GetModulesWithoutQuiz(3);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetCourseQuizStatus_ShouldReturnOk()
        {
            _serviceMock
                .Setup(s => s.GetQuizStatusForCourseAsync(9))
                .ReturnsAsync(new { status = "complete" });

            var result = await _controller.GetCourseQuizStatus(9);

            Assert.IsType<OkObjectResult>(result);
        }
    }
}
