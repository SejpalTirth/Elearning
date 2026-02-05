using DTOs._4AssessmentService;
using AssessmentService.BLL.Interfaces;
using AssessmentService.BLL.UserContext;
using AssessmentService.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

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
            var response = new CreateQuizResponseDto { QuizId = 99, Title = dto.Title };

            _serviceMock
                .Setup(s => s.CreateQuizAsync(dto))
                .ReturnsAsync(response);

            var actionResult = await _controller.CreateQuiz(dto);
            var okResult = actionResult.Result as OkObjectResult;

            Assert.NotNull(okResult);
            Assert.Equal(response, okResult!.Value);
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

            var response = new AddQuestionResponseDto { QuestionId = 5 };

            _serviceMock
                .Setup(s => s.AddQuestionAsync(dto))
                .ReturnsAsync(response);

            var actionResult = await _controller.AddQuestion(dto);
            var okResult = actionResult.Result as OkObjectResult;

            Assert.NotNull(okResult);
            Assert.Equal(response, okResult!.Value);
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
            var quiz = new QuizForModuleDto
            {
                QuizId = 1,
                ModuleId = 20,
                Title = "Test Quiz"
            };

            _serviceMock
                .Setup(s => s.GetQuizForModuleAsync(dto.ModuleId, userId))
                .ReturnsAsync(quiz);

            var actionResult = await _controller.GetQuizForModule(dto);
            var okResult = actionResult.Result as OkObjectResult;

            Assert.NotNull(okResult);
            var value = okResult!.Value as QuizForModuleDto;
            Assert.NotNull(value);
            Assert.Equal(dto.ModuleId, value!.ModuleId);
        }

        [Fact]
        public async Task GetQuizForModule_ShouldReturnUnauthorized_WhenUserContextInvalid()
        {
            _userContextMock
                .Setup(x => x.Current)
                .Returns((UserContextDto?)null);

            var dto = new GetQuizForModuleDto { ModuleId = 20 };
            var result = await _controller.GetQuizForModule(dto);

            Assert.IsType<UnauthorizedObjectResult>(result.Result);
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

            var response = new QuizResultDto();

            _serviceMock
                .Setup(s => s.SubmitQuizAsync(dto))
                .ReturnsAsync(response);

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
            var response = new QuizResultDto();
            _serviceMock
                .Setup(s => s.GetSubmissionResultAsync(It.IsAny<Guid>()))
                .ReturnsAsync(response);

            var dto = new SubmissionResultRequestDto
            {
                SubmissionId = Guid.NewGuid()
            };

            var result = await _controller.GetSubmissionResult(dto) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(response, result!.Value);
        }

        // -------------------------------------------------
        // COURSE QUIZ STATUS
        // -------------------------------------------------
        [Fact]
        public async Task GetQuizStatus_ShouldReturnOk()
        {
            // Arrange
            var dto = new CourseQuizStatusDto { CourseId = 5 };
            var response = new CourseQuizStatusResponseDto
            {
                CourseId = 5,
                AllQuizzesCreated = true,
                Modules = new List<QuizModuleStatusDto>(),
                NextPendingModuleId = null,
                Error = null
            };

            _serviceMock
                .Setup(s => s.GetQuizStatusForCourseAsync(dto.CourseId))
                .ReturnsAsync(response);

            // Act
            var actionResult = await _controller.GetQuizStatus(dto);
            var okResult = actionResult.Result as OkObjectResult;

            // Assert
            Assert.NotNull(okResult);
            var value = Assert.IsType<CourseQuizStatusResponseDto>(okResult!.Value);
            Assert.Equal(response.CourseId, value.CourseId);
            Assert.Equal(response.AllQuizzesCreated, value.AllQuizzesCreated);
            Assert.Equal(response.Modules, value.Modules);
            Assert.Equal(response.NextPendingModuleId, value.NextPendingModuleId);
            Assert.Equal(response.Error, value.Error);
        }

        // -------------------------------------------------
        // UNQUIZZED MODULES
        // -------------------------------------------------
        [Fact]
        public async Task GetUnquizzedModules_ShouldReturnOk()
        {
            var dto = new GetUnquizzedModulesDto { CourseId = 3 };
            var response = new List<int> { 1, 2 };

            _serviceMock
                .Setup(s => s.GetModulesWithoutQuizByCourseAsync(dto.CourseId))
                .ReturnsAsync(response);

            var actionResult = await _controller.GetUnquizzedModules(dto);
            var okResult = actionResult.Result as OkObjectResult;

            Assert.NotNull(okResult);
            Assert.Equal(response, okResult!.Value);
        }
    }
}
