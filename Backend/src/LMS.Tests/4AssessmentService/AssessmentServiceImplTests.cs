using AutoFixture;
using System.Text.Json;
using AssessmentService.BLL.DTOs;
using AssessmentService.BLL.Services;
using AssessmentService.DAL.Models;
using AssessmentService.DAL.Repo;
using Moq;

namespace LMS.Tests.AssessmentService
{
    public class AssessmentServiceImplTests
    {
        private readonly Mock<IQuizRepository> _quizRepoMock;
        private readonly Mock<ISubmissionRepository> _submissionRepoMock;
        private readonly Mock<IQuestionRepository> _questionRepoMock;
        private readonly Mock<IHttpClientFactory> _httpFactoryMock;

        private readonly AssessmentServiceImpl _service;
        private readonly Fixture _fixture;

        public AssessmentServiceImplTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.Clear();
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _quizRepoMock = new Mock<IQuizRepository>();
            _submissionRepoMock = new Mock<ISubmissionRepository>();
            _questionRepoMock = new Mock<IQuestionRepository>();
            _httpFactoryMock = new Mock<IHttpClientFactory>();

            _service = new AssessmentServiceImpl(
                _quizRepoMock.Object,
                _submissionRepoMock.Object,
                _questionRepoMock.Object,
                _httpFactoryMock.Object
            );
        }

        private static JsonDocument ToJson(object obj)
        {
            return JsonDocument.Parse(JsonSerializer.Serialize(obj));
        }

        // -----------------------------------------------------------
        // CREATE QUIZ
        // -----------------------------------------------------------
        [Fact]
        public async Task CreateQuizAsync_WhenQuizExists_ReturnsMessageAndId()
        {
            var dto = _fixture.Build<CreateQuizDto>()
                              .With(x => x.ModuleId, 7)
                              .Create();

            var existing = new Quiz { Id = 42, ModuleId = 7 };

            _quizRepoMock.Setup(r => r.GetByModuleIdAsync(7))
                         .ReturnsAsync(existing);

            var result = await _service.CreateQuizAsync(dto);

            using var doc = ToJson(result);

            Assert.Equal("Quiz already exists for this module.",
                         doc.RootElement.GetProperty("message").GetString());
            Assert.Equal(42,
                         doc.RootElement.GetProperty("quizId").GetInt32());

            _quizRepoMock.Verify(r => r.AddAsync(It.IsAny<Quiz>()), Times.Never);
        }

        [Fact]
        public async Task CreateQuizAsync_WhenNotExists_CreatesQuiz()
        {
            var dto = _fixture.Build<CreateQuizDto>()
                              .With(x => x.ModuleId, 9)
                              .Create();

            _quizRepoMock.Setup(r => r.GetByModuleIdAsync(9))
                         .ReturnsAsync((Quiz?)null);

            _quizRepoMock.Setup(r => r.AddAsync(It.IsAny<Quiz>()))
                         .Callback<Quiz>(q => q.Id = 100)
                         .Returns(Task.CompletedTask);

            _quizRepoMock.Setup(r => r.SaveChangesAsync())
                         .Returns(Task.CompletedTask);

            var result = await _service.CreateQuizAsync(dto);

            using var doc = ToJson(result);
            var root = doc.RootElement;

            // Extract Id (Id or id or QuizId)
            int id =
                root.TryGetProperty("Id", out var idProp)
                ? idProp.GetInt32()
                : root.TryGetProperty("id", out idProp)
                ? idProp.GetInt32()
                : root.GetProperty("quizId").GetInt32(); // final fallback

            // Extract Title
            string? title =
                root.TryGetProperty("Title", out var tProp)
                ? tProp.GetString()
                : root.TryGetProperty("title", out tProp)
                ? tProp.GetString()
                : root.GetProperty("quizTitle").GetString(); // fallback if exists

            // Extract ModuleId
            int moduleId =
                root.TryGetProperty("ModuleId", out var mProp)
                ? mProp.GetInt32()
                : root.TryGetProperty("moduleId", out mProp)
                ? mProp.GetInt32()
                : root.GetProperty("moduleID").GetInt32(); // fallback

            // Assertions
            Assert.Equal(100, id);
            Assert.Equal(dto.Title, title);
            Assert.Equal(9, moduleId);
        }


        // -----------------------------------------------------------
        // ADD QUESTION
        // -----------------------------------------------------------
        [Fact]
        public async Task AddQuestion_WhenQuizNotFound_ReturnsMessage()
        {
            _quizRepoMock.Setup(r => r.GetByIdAsync(10))
                         .ReturnsAsync((Quiz?)null);

            var dto = _fixture.Build<CreateQuestionDto>()
                              .With(x => x.Options, new List<string> { "A", "B" })
                              .With(x => x.CorrectAnswerIndex, 0)
                              .Create();

            var result = await _service.AddQuestionAsync(10, dto);

            using var doc = ToJson(result);

            Assert.Equal("Quiz not found.", doc.RootElement.GetProperty("message").GetString());
            _questionRepoMock.Verify(r => r.AddAsync(It.IsAny<Question>()), Times.Never);
        }

        [Fact]
        public async Task AddQuestion_WhenInvalidOptions_ReturnsMessage()
        {
            var quiz = new Quiz { Id = 20, Questions = new List<Question>() };

            _quizRepoMock.Setup(r => r.GetByIdAsync(20))
                         .ReturnsAsync(quiz);

            var dto = _fixture.Build<CreateQuestionDto>()
                              .With(x => x.Options, new List<string> { "OnlyOne" })
                              .Create();

            var result = await _service.AddQuestionAsync(20, dto);

            using var doc = ToJson(result);

            Assert.Equal("A question must have minimum 2 options.",
                doc.RootElement.GetProperty("message").GetString());
        }

        [Fact]
        public async Task AddQuestion_WhenCorrectIndexOutOfRange_ReturnsMessage()
        {
            var quiz = new Quiz { Id = 30, Questions = new List<Question>() };

            _quizRepoMock.Setup(r => r.GetByIdAsync(30))
                         .ReturnsAsync(quiz);

            var dto = new CreateQuestionDto
            {
                Text = "Q",
                Marks = 1,
                Options = new List<string> { "A", "B" },
                CorrectAnswerIndex = 99
            };

            var result = await _service.AddQuestionAsync(30, dto);

            using var doc = ToJson(result);

            Assert.Equal("CorrectAnswerIndex is out of range.",
                doc.RootElement.GetProperty("message").GetString());
        }

        [Fact]
        public async Task AddQuestion_Success_AddsAndUpdatesQuiz()
        {
            var quiz = new Quiz { Id = 40, TotalMarks = 5, Questions = new List<Question>() };

            _quizRepoMock.Setup(r => r.GetByIdAsync(40))
                         .ReturnsAsync(quiz);

            _questionRepoMock.Setup(r => r.AddAsync(It.IsAny<Question>()))
                             .Callback<Question>(q => q.Id = 555)
                             .Returns(Task.CompletedTask);

            _questionRepoMock.Setup(r => r.SaveChangesAsync())
                             .Returns(Task.CompletedTask);

            _quizRepoMock.Setup(r => r.SaveChangesAsync())
                         .Returns(Task.CompletedTask);

            var dto = new CreateQuestionDto
            {
                Text = "New Q",
                Marks = 3,
                Options = new List<string> { "A", "B", "C" },
                CorrectAnswerIndex = 1
            };

            var result = await _service.AddQuestionAsync(40, dto);

            using var doc = ToJson(result);

            Assert.Equal("Question added successfully.",
                doc.RootElement.GetProperty("message").GetString());
            Assert.Equal(555, doc.RootElement.GetProperty("questionId").GetInt32());
            Assert.Equal(8, quiz.TotalMarks); // 5 + 3
        }

        // -----------------------------------------------------------
        // GET QUIZ FOR MODULE (partial tests rewritten)
        // -----------------------------------------------------------
        [Fact]
        public async Task GetQuizForModule_NoQuiz_ReturnsNull()
        {
            _quizRepoMock.Setup(r => r.GetByModuleIdAsync(77))
                         .ReturnsAsync((Quiz?)null);

            var result = await _service.GetQuizForModuleAsync(77, Guid.NewGuid());

            Assert.Null(result);
        }
    }
}