using AutoFixture;
using System.Text.Json;
using AssessmentService.BLL.DTOs;
using AssessmentService.BLL.Services;
using AssessmentService.DAL.Models;
using AssessmentService.DAL.Repo;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Moq;

namespace LMS.Tests.AssessmentService
{
    // -----------------------------------------------------------
    // Helper HTTP handler
    // -----------------------------------------------------------
    internal class FakeHttpHandler : HttpMessageHandler
    {
        public object? ResponseToSend { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var json = JsonSerializer.Serialize(ResponseToSend);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = content
            });
        }
    }

    public class AssessmentServiceImplTests
    {
        private readonly Fixture _fixture;

        private readonly Mock<IQuizRepository> _quizRepoMock;
        private readonly Mock<ISubmissionRepository> _submissionRepoMock;
        private readonly Mock<IQuestionRepository> _questionRepoMock;
        private readonly Mock<IHttpClientFactory> _httpFactoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;

        private readonly AssessmentServiceImpl _service;

        public AssessmentServiceImplTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.Clear();
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _quizRepoMock = new Mock<IQuizRepository>();
            _submissionRepoMock = new Mock<ISubmissionRepository>();
            _questionRepoMock = new Mock<IQuestionRepository>();
            _httpFactoryMock = new Mock<IHttpClientFactory>();
            _mapperMock = new Mock<IMapper>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            _httpContextAccessorMock
                .Setup(x => x.HttpContext)
                .Returns(new DefaultHttpContext());

            _service = new AssessmentServiceImpl(
                _quizRepoMock.Object,
                _submissionRepoMock.Object,
                _questionRepoMock.Object,
                _httpFactoryMock.Object,
                _mapperMock.Object,
                _httpContextAccessorMock.Object
            );
        }

        private static JsonDocument ToJson(object obj) =>
            JsonDocument.Parse(JsonSerializer.Serialize(obj));

        // -----------------------------------------------------------
        // CREATE QUIZ
        // -----------------------------------------------------------
        [Fact]
        public async Task CreateQuizAsync_WhenQuizExists_ReturnsExistingInfo()
        {
            var dto = new CreateQuizDto { ModuleId = 7, Title = "Quiz" };
            var quiz = new Quiz { Id = 42, ModuleId = 7 };

            _quizRepoMock.Setup(r => r.GetByModuleIdAsync(7))
                         .ReturnsAsync(quiz);

            var result = await _service.CreateQuizAsync(dto);

            using var json = ToJson(result);

            Assert.Equal("Quiz already exists.",
                json.RootElement.GetProperty("message").GetString());
            Assert.Equal(42,
                json.RootElement.GetProperty("quizId").GetInt32());
        }

        // -----------------------------------------------------------
        // ADD QUESTION
        // -----------------------------------------------------------
        [Fact]
        public async Task AddQuestion_WhenQuizNotFound_ReturnsMessage()
        {
            _quizRepoMock.Setup(r => r.GetByIdAsync(10))
                         .ReturnsAsync((Quiz?)null);

            var dto = new AddQuestionDto
            {
                QuizId = 10,
                Question = new CreateQuestionDto
                {
                    Question = "Q",
                    Marks = 1,
                    Options = new() { "A", "B" },
                    CorrectAnswerIndex = 0
                }
            };

            var result = await _service.AddQuestionAsync(dto);

            using var json = ToJson(result);

            Assert.Equal("Quiz not found.",
                json.RootElement.GetProperty("message").GetString());
        }

        [Fact]
        public async Task AddQuestion_WhenOptionsLessThanTwo_Throws()
        {
            _quizRepoMock.Setup(r => r.GetByIdAsync(20))
                         .ReturnsAsync(new Quiz { Id = 20 });

            var dto = new AddQuestionDto
            {
                QuizId = 20,
                Question = new CreateQuestionDto
                {
                    Question = "Q",
                    Marks = 1,
                    Options = new() { "OnlyOne" },
                    CorrectAnswerIndex = 0
                }
            };

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.AddQuestionAsync(dto));
        }

        [Fact]
        public async Task AddQuestion_WhenCorrectIndexInvalid_Throws()
        {
            _quizRepoMock.Setup(r => r.GetByIdAsync(30))
                         .ReturnsAsync(new Quiz { Id = 30 });

            var dto = new AddQuestionDto
            {
                QuizId = 30,
                Question = new CreateQuestionDto
                {
                    Question = "Q",
                    Marks = 1,
                    Options = new() { "A", "B" },
                    CorrectAnswerIndex = 5
                }
            };

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.AddQuestionAsync(dto));
        }

        [Fact]
        public async Task AddQuestion_Success_AddsAndUpdatesQuiz()
        {
            var quiz = new Quiz { Id = 40, TotalMarks = 5 };

            _quizRepoMock.Setup(r => r.GetByIdAsync(40))
                         .ReturnsAsync(quiz);

            _questionRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Question>()))
                .Callback<Question>(q => q.Id = 555)
                .Returns(Task.CompletedTask);

            _quizRepoMock.Setup(r => r.SaveChangesAsync())
                         .Returns(Task.CompletedTask);

            var dto = new AddQuestionDto
            {
                QuizId = 40,
                Question = new CreateQuestionDto
                {
                    Question = "New Q",
                    Marks = 3,
                    Options = new() { "A", "B", "C" },
                    CorrectAnswerIndex = 1
                }
            };

            var result = await _service.AddQuestionAsync(dto);

            using var json = ToJson(result);

            Assert.Equal("Question added successfully.",
                json.RootElement.GetProperty("message").GetString());
            Assert.Equal(555,
                json.RootElement.GetProperty("questionId").GetInt32());
            Assert.Equal(8, quiz.TotalMarks);
        }

        // -----------------------------------------------------------
        // GET QUIZ FOR MODULE
        // -----------------------------------------------------------
        [Fact]
        public async Task GetQuizForModule_NoQuiz_ReturnsNull()
        {
            _quizRepoMock.Setup(r => r.GetByModuleIdAsync(99))
                         .ReturnsAsync((Quiz?)null);

            var result = await _service.GetQuizForModuleAsync(99, Guid.NewGuid());

            Assert.Null(result);
        }

        // -----------------------------------------------------------
        // SUBMIT QUIZ
        // -----------------------------------------------------------
        [Fact]
        public async Task SubmitQuiz_FirstAttempt_ComputesScore()
        {
            var quiz = new Quiz
            {
                Id = 1,
                Questions = new List<Question>()
                {
                    new Question
                    {
                        Id = 10,
                        Marks = 2,
                        Answers = new List<Answer>()
                        {
                            new Answer { Id = 100, IsCorrect = true }
                        }
                    }
                }
            };

            _quizRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1))
                         .ReturnsAsync(quiz);

            _submissionRepoMock.Setup(r => r.GetBestSubmissionAsync(1, It.IsAny<Guid>()))
                               .ReturnsAsync((QuizSubmission?)null);

            _submissionRepoMock.Setup(r => r.AddAsync(It.IsAny<QuizSubmission>()))
                               .Returns(Task.CompletedTask);

            _submissionRepoMock.Setup(r => r.SaveChangesAsync())
                               .Returns(Task.CompletedTask);

            var dto = new SubmitQuizDto
            {
                QuizId = 1,
                UserId = Guid.NewGuid(),
                Answers = new()
                {
                    new SubmitAnswerDto
                    {
                        QuestionId = 10,
                        SelectedAnswerId = 100
                    }
                }
            };

            var result = await _service.SubmitQuizAsync(dto);

            var json = JsonSerializer.Serialize(result);
            Assert.Contains("\"Passed\":true", json);
        }
    }
}