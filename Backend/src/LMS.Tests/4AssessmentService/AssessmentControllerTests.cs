using AssessmentService.BLL.DTOs;
using AssessmentService.BLL.Interfaces;
using AssessmentService.BLL.UserContext;
using AssessmentService.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UserService.DAL.Models;

namespace LMS.Tests.AssessmentService
{
    public class AssessmentControllerTests
    {
        private readonly Mock<IAssessmentService> _serviceMock;
        private readonly Mock<IUserContextAccessor> _userContextMock;
        private readonly AssessmentController _controller;

        public AssessmentControllerTests()
        {
            _serviceMock = new Mock<IAssessmentService>();
            _userContextMock = new Mock<IUserContextAccessor>();

            _controller = new AssessmentController(
                _serviceMock.Object,
                _userContextMock.Object);
        }

        // -------------------------------------------------
        // CREATE QUIZ
        // -------------------------------------------------
        [Fact]
        public async Task CreateQuiz_ShouldReturnOk()
        {
            var dto = new CreateQuizDto { ModuleId = 1, Title = "Test Quiz" };

            _serviceMock
                .Setup(s => s.CreateQuizAsync(dto))
                .ReturnsAsync(new { quizId = 99 });

            var result = await _controller.CreateQuiz(dto);

            Assert.IsType<OkObjectResult>(result);
        }

        // -------------------------------------------------
        // ADD QUESTION
        // -------------------------------------------------
        [Fact]
        public async Task AddQuestion_ShouldReturnOk()
        {
            var dto = new AddQuestionDto
            {
                QuizId = 10,
                Question = new CreateQuestionDto
                {
                    Question = "Q1?",
                    Marks = 1,
                    Options = new() { "A", "B" },
                    CorrectAnswerIndex = 0
                }
            };

            _serviceMock
                .Setup(s => s.AddQuestionAsync(dto))
                .ReturnsAsync(new { success = true });

            var result = await _controller.AddQuestion(dto);

            Assert.IsType<OkObjectResult>(result);
        }

        // -------------------------------------------------
        // GET QUIZ FOR MODULE
        // -------------------------------------------------

        [Fact]
        public async Task GetQuizForModule_ShouldReturnOk_WhenUserContextValid()
        {
            var userId = Guid.NewGuid();

            _userContextMock
                .Setup(x => x.Current)
                .Returns(new UserContextDto { UserId = userId });

            var dto = new GetQuizForModuleDto { ModuleId = 20 };

            _serviceMock
                .Setup(s => s.GetQuizForModuleAsync(dto.ModuleId, userId))
                .ReturnsAsync(new QuizForModuleDto
                {
                    QuizId = 1,
                    ModuleId = 20,
                    Title = "Test Quiz"
                });

            var result = await _controller.GetQuizForModule(dto);

            Assert.IsType<OkObjectResult>(result);
        }

        // -------------------------------------------------
        // SUBMIT QUIZ
        // -------------------------------------------------
        [Fact]
        public async Task SubmitQuiz_ShouldReturnOk()
        {
            var dto = new SubmitQuizDto
            {
                QuizId = 1,
                UserId = Guid.NewGuid(),
                Answers = new()
            };

            _serviceMock
                .Setup(s => s.SubmitQuizAsync(dto))
                .ReturnsAsync(new QuizResultDto());

            var result = await _controller.SubmitQuiz(dto);

            Assert.IsType<OkObjectResult>(result);
        }

        // -------------------------------------------------
        // SUBMISSION RESULT
        // -------------------------------------------------
        [Fact]
        public async Task GetSubmissionResult_ShouldReturnNotFound_WhenResultMissing()
        {
            _serviceMock
                .Setup(s => s.GetSubmissionResultAsync(It.IsAny<Guid>()))
                .ReturnsAsync((QuizResultDto?)null);

            var dto = new SubmissionResultRequestDto
            {
                SubmissionId = Guid.NewGuid()
            };

            var result = await _controller.GetSubmissionResult(dto);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetSubmissionResult_ShouldReturnOk_WhenResultExists()
        {
            _serviceMock
                .Setup(s => s.GetSubmissionResultAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new QuizResultDto());

            var dto = new SubmissionResultRequestDto
            {
                SubmissionId = Guid.NewGuid()
            };

            var result = await _controller.GetSubmissionResult(dto);

            Assert.IsType<OkObjectResult>(result);
        }

        // -------------------------------------------------
        // COURSE QUIZ STATUS
        // -------------------------------------------------
        [Fact]
        public async Task GetQuizStatus_ShouldReturnOk()
        {
            var dto = new CourseQuizStatusDto { CourseId = 5 };

            _serviceMock
                .Setup(s => s.GetQuizStatusForCourseAsync(dto.CourseId))
                .ReturnsAsync(new { ok = true });

            var result = await _controller.GetQuizStatus(dto);

            Assert.IsType<OkObjectResult>(result);
        }

        // -------------------------------------------------
        // UNQUIZZED MODULES
        // -------------------------------------------------
        [Fact]
        public async Task GetUnquizzedModules_ShouldReturnOk()
        {
            var dto = new GetUnquizzedModulesDto { CourseId = 3 };

            _serviceMock
                .Setup(s => s.GetModulesWithoutQuizByCourseAsync(dto.CourseId))
                .ReturnsAsync(new List<int> { 1, 2 });

            var result = await _controller.GetUnquizzedModules(dto);

            Assert.IsType<OkObjectResult>(result);
        }
    }
}
