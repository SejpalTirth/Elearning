using AutoFixture;
using System.Text.Json;
using DTOs._4AssessmentService;
using AssessmentService.BLL.Services;
using AssessmentService.DAL.Models;
using AssessmentService.DAL.Repo;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Moq;
using static AssessmentService.BLL.Services.AssessmentServiceImpl;

namespace LMS.Tests.AssessmentService
{
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

        // ---------------- CREATE QUIZ ----------------
        [Fact]
        public async Task CreateQuizAsync_WhenQuizExists_ReturnsExistingInfo()
        {
            var dto = new CreateQuizDto { ModuleId = 7, Title = "Quiz" };
            var quiz = new Quiz { Id = 42, ModuleId = 7, Title = "Existing Quiz" };

            _quizRepoMock.Setup(r => r.GetByModuleIdAsync(7))
                         .ReturnsAsync(quiz);

            var result = await _service.CreateQuizAsync(dto);

            Assert.True(result.AlreadyExists);
            Assert.Equal(42, result.QuizId);
            Assert.Equal("Existing Quiz", result.Title);
            Assert.Equal(7, result.ModuleId);
            Assert.Equal("Quiz already exists.", result.Message);
        }

        [Fact]
        public async Task CreateQuizAsync_WhenNewQuiz_CreatesSuccessfully()
        {
            var dto = new CreateQuizDto { ModuleId = 8, Title = "New Quiz", TimeLimitMinutes = 30 };

            _quizRepoMock.Setup(r => r.GetByModuleIdAsync(8))
                         .ReturnsAsync((Quiz?)null);

            _quizRepoMock.Setup(r => r.AddAsync(It.IsAny<Quiz>()))
                         .Returns(Task.CompletedTask);

            _quizRepoMock.Setup(r => r.SaveChangesAsync())
                         .Returns(Task.CompletedTask);

            var result = await _service.CreateQuizAsync(dto);

            Assert.False(result.AlreadyExists);
            Assert.Equal("New Quiz", result.Title);
            Assert.Equal(8, result.ModuleId);
            Assert.Equal("Quiz created successfully.", result.Message);
        }

        // ---------------- ADD QUESTION ----------------
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

            Assert.False(result.Success);
            Assert.Equal("Quiz not found.", result.Message);
        }

        [Fact]
        public async Task AddQuestion_WhenOptionsLessThanTwo_Throws()
        {
            _quizRepoMock.Setup(r => r.GetByIdAsync(20))
                         .ReturnsAsync(new Quiz { Id = 20, Questions = new List<Question>() });

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
                         .ReturnsAsync(new Quiz { Id = 30, Questions = new List<Question>() });

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
            var quiz = new Quiz
            {
                Id = 40,
                TotalMarks = 5,
                Questions = new List<Question>()
            };

            _quizRepoMock.Setup(r => r.GetByIdAsync(40))
                         .ReturnsAsync(quiz);

            _mapperMock.Setup(m => m.Map<Question>(It.IsAny<CreateQuestionDto>()))
                       .Returns<CreateQuestionDto>(dto => new Question
                       {
                           QuestionText = dto.Question,
                           Marks = dto.Marks,
                           Answers = new List<Answer>()
                       });

            _questionRepoMock.Setup(r => r.AddAsync(It.IsAny<Question>()))
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

            Assert.True(result.Success);
            Assert.Equal("Question added successfully.", result.Message);
            Assert.Equal(555, result.QuestionId);
            Assert.Equal(8, quiz.TotalMarks);
        }

        // ---------------- GET QUIZ FOR MODULE ----------------
        [Fact]
        public async Task GetQuizForModule_NoQuiz_ReturnsNull()
        {
            _quizRepoMock.Setup(r => r.GetByModuleIdAsync(99))
                         .ReturnsAsync((Quiz?)null);

            var result = await _service.GetQuizForModuleAsync(99, Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task GetQuizForModule_WithQuiz_ReturnsDto()
        {
            var quiz = new Quiz { Id = 1, ModuleId = 100, Title = "Test Quiz", TotalMarks = 10 };
            _quizRepoMock.Setup(r => r.GetByModuleIdAsync(100))
                         .ReturnsAsync(quiz);

            _mapperMock.Setup(m => m.Map<QuizForModuleDto>(quiz))
                       .Returns(new QuizForModuleDto
                       {
                           QuizId = quiz.Id,
                           ModuleId = quiz.ModuleId,
                           Title = quiz.Title,
                           TotalMarks = quiz.TotalMarks ?? 0
                       });

            var result = await _service.GetQuizForModuleAsync(100, Guid.NewGuid());

            Assert.NotNull(result);
            Assert.Equal(1, result.QuizId);
            Assert.Equal(100, result.ModuleId);
            Assert.Equal("Test Quiz", result.Title);
        }

        // ---------------- SUBMIT QUIZ ----------------
        [Fact]
        public async Task SubmitQuiz_FirstAttempt_ComputesScore()
        {
            var quiz = new Quiz
            {
                Id = 1,
                Questions = new List<Question>
                {
                    new Question
                    {
                        Id = 10,
                        Marks = 2,
                        Answers = new List<Answer>
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
                Answers = new List<SubmitAnswerDto>
                {
                    new SubmitAnswerDto { QuestionId = 10, SelectedAnswerId = 100 }
                }
            };

            var result = await _service.SubmitQuizAsync(dto);

            Assert.Equal(2, result.ObtainedMarks);
            Assert.True(result.Passed);
            Assert.False(result.AlreadyPassed);
            Assert.Equal(100, result.Percentage);
        }

        // ---------------- HTTP / MODULE TESTS ----------------
        [Fact]
        public async Task CreateQuizAsync_NewQuiz_HttpClientThrows_DoesNotFail()
        {
            var dto = new CreateQuizDto { ModuleId = 1, Title = "New Quiz" };
            _quizRepoMock.Setup(r => r.GetByModuleIdAsync(1))
                         .ReturnsAsync((Quiz?)null);

            _quizRepoMock.Setup(r => r.AddAsync(It.IsAny<Quiz>()))
                         .Returns(Task.CompletedTask);

            _quizRepoMock.Setup(r => r.SaveChangesAsync())
                         .Returns(Task.CompletedTask);

            _httpFactoryMock.Setup(f => f.CreateClient("CourseService"))
                            .Throws(new Exception("HTTP client fail"));

            var result = await _service.CreateQuizAsync(dto);

            Assert.False(result.AlreadyExists);
            Assert.Equal("Quiz created successfully.", result.Message);
        }

        [Fact]
        public async Task CreateQuizAsync_ModulePublishReturnsNull_DoesNotFail()
        {
            var dto = new CreateQuizDto { ModuleId = 2, Title = "Quiz 2" };
            _quizRepoMock.Setup(r => r.GetByModuleIdAsync(2))
                         .ReturnsAsync((Quiz?)null);

            _quizRepoMock.Setup(r => r.AddAsync(It.IsAny<Quiz>())).Returns(Task.CompletedTask);
            _quizRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var fakeHandler = new FakeHttpHandler { ResponseToSend = (ModuleDetail?)null };
            var client = new HttpClient(fakeHandler);

            _httpFactoryMock.Setup(f => f.CreateClient("CourseService")).Returns(client);

            var result = await _service.CreateQuizAsync(dto);

            Assert.False(result.AlreadyExists);
            Assert.Equal("Quiz created successfully.", result.Message);
        }

        [Fact]
        public async Task GetAllQuizzesAsync_ReturnsEmptyList()
        {
            _quizRepoMock.Setup(r => r.GetAllAsync())
                         .ReturnsAsync(new List<Quiz>());

            _mapperMock.Setup(m => m.Map<IEnumerable<QuizSummaryDto>>(It.IsAny<List<Quiz>>()))
                       .Returns(new List<QuizSummaryDto>());

            var result = await _service.GetAllQuizzesAsync();

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllQuizzesAsync_ReturnsMappedQuizzes()
        {
            var quizzes = new List<Quiz> { new Quiz { Id = 1, Title = "Q1" }, new Quiz { Id = 2, Title = "Q2" } };
            _quizRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(quizzes);

            _mapperMock.Setup(m => m.Map<IEnumerable<QuizSummaryDto>>(quizzes))
                       .Returns(quizzes.Select(q => new QuizSummaryDto { Id = q.Id, Title = q.Title }));

            var result = await _service.GetAllQuizzesAsync();

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetQuizByIdAsync_ReturnsNull_WhenNotFound()
        {
            _quizRepoMock.Setup(r => r.GetByIdWithDetailsAsync(100)).ReturnsAsync((Quiz?)null);
            var result = await _service.GetQuizByIdAsync(100);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetQuizByIdAsync_ReturnsMappedQuiz()
        {
            var quiz = new Quiz { Id = 10, Title = "Test Quiz" };
            _quizRepoMock.Setup(r => r.GetByIdWithDetailsAsync(10)).ReturnsAsync(quiz);
            _mapperMock.Setup(m => m.Map<QuizDetailDto>(quiz))
                       .Returns(new QuizDetailDto { Id = quiz.Id, Title = quiz.Title });

            var result = await _service.GetQuizByIdAsync(10);

            Assert.NotNull(result);
            Assert.Equal(10, result.Id);
            Assert.Equal("Test Quiz", result.Title);
        }

        [Fact]
        public async Task SubmitQuiz_AlreadyPassed_ReturnsAlreadyPassedResult()
        {
            var quiz = new Quiz { Id = 1, Questions = new List<Question> { new Question { Id = 101, Marks = 2 } } };
            _quizRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(quiz);

            var submission = new QuizSubmission { Id = Guid.NewGuid(), Score = 67 };
            _submissionRepoMock.Setup(r => r.GetBestSubmissionAsync(1, It.IsAny<Guid>()))
                               .ReturnsAsync(submission);

            var dto = new SubmitQuizDto { QuizId = 1, UserId = Guid.NewGuid(), Answers = new List<SubmitAnswerDto>() };
            var result = await _service.SubmitQuizAsync(dto);

            Assert.True(result.AlreadyPassed);
            Assert.True(result.Passed);
            Assert.Equal("Already passed.", result.StatusMessage);
        }

        [Fact]
        public async Task GetModulesWithoutQuizAsync_ReturnsMissingModules()
        {
            _quizRepoMock.Setup(r => r.GetByModuleIdAsync(1)).ReturnsAsync((Quiz?)null);
            _quizRepoMock.Setup(r => r.GetByModuleIdAsync(2)).ReturnsAsync(new Quiz { Id = 5 });

            var result = await _service.GetModulesWithoutQuizAsync(new List<int> { 1, 2 });

            Assert.Single(result);
            Assert.Contains(1, result);
        }

        [Fact]
        public void CalculatePercentage_ReturnsZero_WhenTotalZero()
        {
            var result = typeof(AssessmentServiceImpl)
                .GetMethod("CalculatePercentage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .Invoke(null, new object[] { 5, 0 });

            Assert.Equal(0m, result);
        }

        [Fact]
        public void HasAlreadyPassed_ReturnsTrue_WhenScoreEqualsPass()
        {
            var submission = new QuizSubmission { Score = 67 };
            var result = typeof(AssessmentServiceImpl)
                .GetMethod("HasAlreadyPassed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .Invoke(null, new object[] { submission, 100 });

            Assert.True((bool)result!);
        }

        [Fact]
        public async Task GetSubmissionResultAsync_ReturnsNull_WhenSubmissionNotFound()
        {
            var submissionId = Guid.NewGuid();
            _submissionRepoMock.Setup(r => r.GetByIdAsync(submissionId))
                               .ReturnsAsync((QuizSubmission?)null);

            var result = await _service.GetSubmissionResultAsync(submissionId);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetSubmissionResultAsync_ReturnsNull_WhenQuizNotFound()
        {
            var submissionId = Guid.NewGuid();
            _submissionRepoMock.Setup(r => r.GetByIdAsync(submissionId))
                               .ReturnsAsync(new QuizSubmission { Id = submissionId, QuizId = 99 });

            _quizRepoMock.Setup(r => r.GetByIdAsync(99))
                         .ReturnsAsync((Quiz?)null);

            var result = await _service.GetSubmissionResultAsync(submissionId);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetQuizForModuleAsync_SetsAlreadyPassed_WhenBestScoreExists()
        {
            var moduleId = 10;
            var quiz = new Quiz { Id = 1, ModuleId = moduleId, Title = "Test Quiz", TotalMarks = 10 };
            _quizRepoMock.Setup(r => r.GetByModuleIdAsync(moduleId))
                         .ReturnsAsync(quiz);

            _submissionRepoMock.Setup(r => r.GetBestSubmissionAsync(quiz.Id, It.IsAny<Guid>()))
                               .ReturnsAsync(new QuizSubmission { Score = 7 });

            _mapperMock.Setup(m => m.Map<QuizForModuleDto>(quiz))
                       .Returns(new QuizForModuleDto { QuizId = quiz.Id, ModuleId = moduleId, TotalMarks = 10 });

            var result = await _service.GetQuizForModuleAsync(moduleId, Guid.NewGuid());

            Assert.NotNull(result);
            Assert.True(result.AlreadyPassed);
        }

        // ---------------- PROTECTED / OVERRIDE TESTS ----------------
        public class TestAssessmentService : AssessmentServiceImpl
        {
            public Func<int, Task<List<ModuleInfo>?>>? FetchModulesFunc { get; set; }

            public TestAssessmentService(
                IQuizRepository quizRepo,
                ISubmissionRepository submissionRepo,
                IQuestionRepository questionRepo,
                IHttpClientFactory httpFactory,
                IMapper mapper,
                IHttpContextAccessor httpContextAccessor
            ) : base(quizRepo, submissionRepo, questionRepo, httpFactory, mapper, httpContextAccessor) { }

            protected override Task<List<ModuleInfo>?> FetchModulesForCourseAsync(int courseId)
            {
                if (FetchModulesFunc != null)
                    return FetchModulesFunc(courseId);
                return base.FetchModulesForCourseAsync(courseId);
            }
        }

        [Fact]
        public async Task GetModulesWithoutQuizByCourseAsync_ReturnsEmptyList_WhenNoModules()
        {
            var testService = new TestAssessmentService(
                _quizRepoMock.Object,
                _submissionRepoMock.Object,
                _questionRepoMock.Object,
                _httpFactoryMock.Object,
                _mapperMock.Object,
                _httpContextAccessorMock.Object
            );

            testService.FetchModulesFunc = _ => Task.FromResult<List<ModuleInfo>?>(null);

            var result = await testService.GetModulesWithoutQuizByCourseAsync(1);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetQuizStatusForCourseAsync_ReturnsError_WhenModulesNull()
        {
            var testService = new TestAssessmentService(
                _quizRepoMock.Object,
                _submissionRepoMock.Object,
                _questionRepoMock.Object,
                _httpFactoryMock.Object,
                _mapperMock.Object,
                _httpContextAccessorMock.Object
            );

            testService.FetchModulesFunc = _ => Task.FromResult<List<ModuleInfo>?>(null);

            var result = await testService.GetQuizStatusForCourseAsync(100);

            Assert.False(result.AllQuizzesCreated);
            Assert.Equal("CourseService unavailable", result.Error);
        }
    }
}