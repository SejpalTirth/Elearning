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
            _serviceMock = new Mock<IAssessmentService>();
            _controller = new AssessmentController(_serviceMock.Object);

            // Allow controller to modify HttpContext
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
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
            Assert.Equal(200, result!.StatusCode);
        }

        // -------------------------------------------------
        // ADD QUESTION
        // -------------------------------------------------
        [Fact]
        public async Task AddQuestion_ReturnsOk()
        {
            var dto = new CreateQuestionDto
            {
                Text = "Q1?",
                Marks = 1,
                Options = new() { "A", "B" },
                CorrectAnswerIndex = 0
            };

            _serviceMock
                .Setup(s => s.AddQuestionAsync(10, dto))
                .ReturnsAsync(new { success = true });

            var result = await _controller.AddQuestion(10, dto) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result!.StatusCode);
        }

        // -------------------------------------------------
        // GET QUIZ FOR MODULE
        // -------------------------------------------------

        [Fact]
        public async Task GetQuizForModule_ShouldReturnUnauthorized_WhenMissingAuthorization()
        {
            // No "Authorization" header added

            var result = await _controller.GetQuizForModule(20);

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Missing access token.", unauthorized.Value);
        }

        [Fact]
        public async Task GetQuizForModule_ShouldReturnUnauthorized_WhenTokenInvalid()
        {
            _controller.HttpContext.Request.Headers["Authorization"] = "Bearer INVALID_TOKEN";

            var result = await _controller.GetQuizForModule(20);

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Invalid token format", unauthorized.Value);
        }

        [Fact]
        public async Task GetQuizForModule_ShouldReturnUnauthorized_WhenSubClaimMissing()
        {
            // Create JWT with NO "sub" claim
            var token = new JwtSecurityToken(
                claims: new[] { new Claim("name", "test") }
            );

            string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            _controller.HttpContext.Request.Headers["Authorization"] = $"Bearer {tokenString}";

            var result = await _controller.GetQuizForModule(20);

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Token missing required 'sub' claim.", unauthorized.Value);
        }

        [Fact]
        public async Task GetQuizForModule_ShouldReturnOk_WhenTokenAndDataValid()
        {
            Guid userId = Guid.NewGuid();

            var token = new JwtSecurityToken(
                claims: new[] { new Claim("sub", userId.ToString()) }
            );
            string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            _controller.HttpContext.Request.Headers["Authorization"] = $"Bearer {tokenString}";

            _serviceMock
                .Setup(s => s.GetQuizForModuleAsync(20, userId))
                .ReturnsAsync(new { QuizId = 1 });

            var result = await _controller.GetQuizForModule(20) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result!.StatusCode);
        }

        // -------------------------------------------------
        // SUBMIT QUIZ
        // -------------------------------------------------
        [Fact]
        public async Task SubmitQuiz_ReturnsOk()
        {
            var dto = new SubmitQuizDto
            {
                QuizId = 1,
                UserId = Guid.NewGuid(),
                Answers = new()
            };

            _serviceMock
                .Setup(s => s.SubmitQuizAsync(dto))
                .ReturnsAsync(new { score = 5 });

            var result = await _controller.SubmitQuiz(dto) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result!.StatusCode);
        }

        // -------------------------------------------------
        // GET SUBMISSION RESULT
        // -------------------------------------------------
        [Fact]
        public async Task GetSubmissionResult_ShouldReturnNotFound_WhenNull()
        {
            _serviceMock
                .Setup(s => s.GetSubmissionResultAsync(It.IsAny<Guid>()))
                .ReturnsAsync((object?)null);

            var result = await _controller.GetSubmissionResult(Guid.NewGuid());

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetSubmissionResult_ShouldReturnOk_WhenExists()
        {
            _serviceMock
                .Setup(s => s.GetSubmissionResultAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new { result = "OK" });

            var result = await _controller.GetSubmissionResult(Guid.NewGuid()) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result!.StatusCode);
        }
    }
}
