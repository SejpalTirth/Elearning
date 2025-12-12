using AutoFixture;
using System.Text.Json;
using AssessmentService.BLL.DTOs;
using AssessmentService.BLL.Services;
using AssessmentService.DAL.Models;
using AssessmentService.DAL.Repo;
using Moq;

namespace LMS.Tests.AssessmentService
{
    // Helper class for mocking HTTP responses
    internal class FakeHttpHandler : HttpMessageHandler
    {
        public object? ResponseToSend { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var json = JsonSerializer.Serialize(ResponseToSend);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = content };
            return Task.FromResult(response);
        }
    }

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

        // -----------------------------------------------------------
        // GET QUIZ FOR MODULE — SUCCESS
        // -----------------------------------------------------------
        [Fact]
        public async Task GetQuizForModule_ReturnsQuizWithQuestions()
        {
            var quiz = new Quiz
            {
                Id = 10,
                ModuleId = 5,
                Title = "Sample Quiz",
                TimeLimitMinutes = 20,
                Questions = new List<Question>
        {
            new Question
            {
                Id = 1,
                QuestionText = "Q1",
                Marks = 2,
                Answers = new List<Answer>
                {
                    new Answer { Id = 100, AnswerText = "A1", IsCorrect = true },
                    new Answer { Id = 101, AnswerText = "A2", IsCorrect = false }
                }
            }
        }
            };

            _quizRepoMock.Setup(r => r.GetByModuleIdAsync(5))
                         .ReturnsAsync(quiz);

            _submissionRepoMock.Setup(r => r.GetBestSubmissionAsync(10, It.IsAny<Guid>()))
                               .ReturnsAsync((QuizSubmission?)null);

            var result = await _service.GetQuizForModuleAsync(5, Guid.NewGuid());

            Assert.NotNull(result);

            var json = JsonSerializer.Serialize(result);
            Assert.Contains("Sample Quiz", json);
            Assert.Contains("\"TotalMarks\":2", json);
        }


        // -----------------------------------------------------------
        // SUBMIT QUIZ — FIRST SUBMISSION (No previous passed)
        // -----------------------------------------------------------
        [Fact]
        public async Task SubmitQuiz_FirstAttempt_ComputesScoreCorrectly()
        {
            var quiz = new Quiz
            {
                Id = 1,
                Questions = new List<Question>
        {
            new Question
            {
                Id = 10, Marks = 2,
                Answers = new List<Answer>
                {
                    new Answer { Id = 100, IsCorrect = true },
                    new Answer { Id = 101, IsCorrect = false },
                }
            }
        }
            };

            _quizRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(quiz);
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
                Answers = new List<SubmitAnswerDto>
        {
            new SubmitAnswerDto { QuestionId = 10, SelectedAnswerId = 100 }
        }
            };

            var result = await _service.SubmitQuizAsync(dto);

            var json = JsonSerializer.Serialize(result);
            Assert.Contains("\"Passed\":true", json);
            Assert.Contains("\"CorrectAnswers\":1", json);
        }


        // -----------------------------------------------------------
        // SUBMIT QUIZ — ALREADY PASSED EARLIER
        // -----------------------------------------------------------
        [Fact]
        public async Task SubmitQuiz_AlreadyPassed_PreventsNewSubmission()
        {
            var quiz = new Quiz
            {
                Id = 1,
                Questions = new List<Question>
        {
            new Question { Id = 10, Marks = 2, Answers = new List<Answer> { new Answer { Id = 100, IsCorrect = true } } }
        }
            };

            var previous = new QuizSubmission
            {
                Id = Guid.NewGuid(),
                QuizId = 1,
                UserId = Guid.NewGuid(),
                Score = 2
            };

            _quizRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(quiz);
            _submissionRepoMock.Setup(r => r.GetBestSubmissionAsync(1, It.IsAny<Guid>()))
                               .ReturnsAsync(previous);

            var result = await _service.SubmitQuizAsync(new SubmitQuizDto
            {
                QuizId = 1,
                UserId = previous.UserId,
                Answers = new List<SubmitAnswerDto>()
            });

            var json = JsonSerializer.Serialize(result);
            Assert.Contains("already passed", json.ToLower());
        }


        // -----------------------------------------------------------
        // GetSubmissionResultAsync — NOT FOUND
        // -----------------------------------------------------------
        [Fact]
        public async Task GetSubmissionResultAsync_WhenMissing_ReturnsNull()
        {
            _submissionRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                               .ReturnsAsync((QuizSubmission?)null);

            var result = await _service.GetSubmissionResultAsync(Guid.NewGuid());

            Assert.Null(result);
        }


        // -----------------------------------------------------------
        // GetSubmissionResultAsync — SUCCESS
        // -----------------------------------------------------------
        [Fact]
        public async Task GetSubmissionResultAsync_ReturnsComputedResult()
        {
            var submission = new QuizSubmission
            {
                Id = Guid.NewGuid(),
                QuizId = 5,
                Score = 7
            };

            var quiz = new Quiz
            {
                Id = 5,
                TotalMarks = 10,
                Questions = new List<Question>()
            };

            _submissionRepoMock.Setup(r => r.GetByIdAsync(submission.Id))
                               .ReturnsAsync(submission);

            _quizRepoMock.Setup(r => r.GetByIdAsync(5))
                         .ReturnsAsync(quiz);

            var result = await _service.GetSubmissionResultAsync(submission.Id);

            Assert.NotNull(result);
            var json = JsonSerializer.Serialize(result);
            Assert.Contains("\"Percentage\":70", json);
            Assert.Contains("\"Passed\":true", json);
        }


        // -----------------------------------------------------------
        // GetModulesWithoutQuizAsync
        // -----------------------------------------------------------
        [Fact]
        public async Task GetModulesWithoutQuizAsync_ReturnsOnlyModulesWithNoQuiz()
        {
            _quizRepoMock.Setup(r => r.GetByModuleIdAsync(1)).ReturnsAsync(new Quiz());
            _quizRepoMock.Setup(r => r.GetByModuleIdAsync(2)).ReturnsAsync((Quiz?)null);
            _quizRepoMock.Setup(r => r.GetByModuleIdAsync(3)).ReturnsAsync((Quiz?)null);

            var result = await _service.GetModulesWithoutQuizAsync(new List<int> { 1, 2, 3 });

            Assert.Equal(new List<int> { 2, 3 }, result);
        }


        // -----------------------------------------------------------
        // GetModulesWithoutQuizByCourseAsync
        // -----------------------------------------------------------
        [Fact]
        public async Task GetModulesWithoutQuizByCourseAsync_FiltersMissingCorrectly()
        {
            // Mock CourseService client
            var handler = new FakeHttpHandler
            {
                ResponseToSend = new[]
                {
            new { Id = 10, Title = "M1" },
            new { Id = 11, Title = "M2" }
        }
            };

            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://fake-course")
            };

            _httpFactoryMock.Setup(f => f.CreateClient("CourseService"))
                            .Returns(client);

            _quizRepoMock.Setup(r => r.GetByModuleIdAsync(10)).ReturnsAsync(new Quiz());
            _quizRepoMock.Setup(r => r.GetByModuleIdAsync(11)).ReturnsAsync((Quiz?)null);

            var result = await _service.GetModulesWithoutQuizByCourseAsync(99);

            Assert.Single(result);
            Assert.Equal(11, result[0]);
        }


        // -----------------------------------------------------------
        // GetQuizStatusForCourseAsync
        // -----------------------------------------------------------
        [Fact]
        public async Task GetQuizStatusForCourseAsync_ReturnsModuleStatus()
        {
            var handler = new FakeHttpHandler
            {
                ResponseToSend = new[]
                {
            new { Id = 100, Title = "M1" },
            new { Id = 101, Title = "M2" }
        }
            };

            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://fake-course")
            };

            _httpFactoryMock.Setup(f => f.CreateClient("CourseService"))
                            .Returns(client);

            _quizRepoMock.Setup(r => r.GetByModuleIdAsync(100)).ReturnsAsync(new Quiz());
            _quizRepoMock.Setup(r => r.GetByModuleIdAsync(101)).ReturnsAsync((Quiz?)null);

            var result = await _service.GetQuizStatusForCourseAsync(77);

            var json = JsonSerializer.Serialize(result);

            Assert.Contains("\"allQuizzesCreated\":false", json);
            Assert.Contains("\"nextPendingModuleId\":101", json);
        }

    }
}