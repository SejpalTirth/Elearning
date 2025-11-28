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
        private readonly AssessmentServiceImpl _service;

        public AssessmentServiceImplTests()
        {
            _quizRepoMock = new Mock<IQuizRepository>();
            _submissionRepoMock = new Mock<ISubmissionRepository>();
            _questionRepoMock = new Mock<IQuestionRepository>();

            _service = new AssessmentServiceImpl(
                _quizRepoMock.Object,
                _submissionRepoMock.Object,
                _questionRepoMock.Object
            );
        }

        // ---------- Helpers ----------
        private static JsonDocument ToJsonDoc(object obj)
        {
            var json = JsonSerializer.Serialize(obj);
            return JsonDocument.Parse(json);
        }

        // ---------- CreateQuiz ----------
        [Fact]
        public async Task CreateQuizAsync_WhenQuizExists_ReturnsMessageAndQuizId()
        {
            var dto = new CreateQuizDto { ModuleId = 7, Title = "X", TimeLimitMinutes = 10 };
            var existing = new Quiz { Id = 42, ModuleId = 7, Title = "Existing" };

            _quizRepoMock.Setup(x => x.GetByModuleIdAsync(dto.ModuleId))
                         .ReturnsAsync(existing);

            var result = await _service.CreateQuizAsync(dto);

            using var doc = ToJsonDoc(result);
            Assert.True(doc.RootElement.TryGetProperty("message", out var msg));
            Assert.True(doc.RootElement.TryGetProperty("quizId", out var qid));
            Assert.Equal("Quiz already exists for this module.", msg.GetString());
            Assert.Equal(42, qid.GetInt32());
            _quizRepoMock.Verify(r => r.AddAsync(It.IsAny<Quiz>()), Times.Never);
        }

        [Fact]
        public async Task CreateQuizAsync_WhenNotExists_CreatesQuizAndReturnsInfo()
        {
            var dto = new CreateQuizDto { ModuleId = 9, Title = "New Quiz", TimeLimitMinutes = 5 };

            _quizRepoMock.Setup(x => x.GetByModuleIdAsync(dto.ModuleId))
                         .ReturnsAsync((Quiz?)null);

            _quizRepoMock.Setup(x => x.AddAsync(It.IsAny<Quiz>())).Returns(Task.CompletedTask)
                         .Callback<Quiz>(q => q.Id = 100); // simulate DB assign

            _quizRepoMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

            var result = await _service.CreateQuizAsync(dto);

            using var doc = ToJsonDoc(result);

            Assert.True(doc.RootElement.TryGetProperty("id", out var idProp) || doc.RootElement.TryGetProperty("Id", out idProp));
            Assert.True(doc.RootElement.TryGetProperty("title", out var titleProp) || doc.RootElement.TryGetProperty("Title", out titleProp));
            Assert.True(doc.RootElement.TryGetProperty("moduleId", out var mProp) || doc.RootElement.TryGetProperty("ModuleId", out mProp));

            // check values
            Assert.Equal(100, idProp.GetInt32());
            Assert.Equal("New Quiz", titleProp.GetString());
            Assert.Equal(9, mProp.GetInt32());

            _quizRepoMock.Verify(r => r.AddAsync(It.IsAny<Quiz>()), Times.Once);
            _quizRepoMock.Verify(r => r.SaveChangesAsync(), Times.AtLeastOnce);
        }

        // ---------- AddQuestion ----------
        [Fact]
        public async Task AddQuestionAsync_WhenQuizNotFound_ReturnsMessage()
        {
            _quizRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Quiz?)null);

            var dto = new CreateQuestionDto { Text = "Q", Marks = 1, Options = new List<string> { "a", "b" }, CorrectAnswerIndex = 0 };
            var res = await _service.AddQuestionAsync(5, dto);

            using var doc = ToJsonDoc(res);
            Assert.True(doc.RootElement.TryGetProperty("message", out var msg));
            Assert.Equal("Quiz not found.", msg.GetString());

            _questionRepoMock.Verify(r => r.AddAsync(It.IsAny<Question>()), Times.Never);
        }

        [Fact]
        public async Task AddQuestionAsync_WhenLessThanTwoOptions_ReturnsMessage()
        {
            var quiz = new Quiz { Id = 10, Questions = new List<Question>() };
            _quizRepoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(quiz);

            var dto = new CreateQuestionDto { Text = "Q", Marks = 1, Options = new List<string> { "only1" }, CorrectAnswerIndex = 0 };

            var res = await _service.AddQuestionAsync(10, dto);

            using var doc = ToJsonDoc(res);
            Assert.True(doc.RootElement.TryGetProperty("message", out var msg));
            Assert.Equal("A question must have at least 2 options.", msg.GetString());

            _questionRepoMock.Verify(r => r.AddAsync(It.IsAny<Question>()), Times.Never);
        }

        [Fact]
        public async Task AddQuestionAsync_WhenCorrectIndexOutOfRange_ReturnsMessage()
        {
            var quiz = new Quiz { Id = 11, Questions = new List<Question>() };
            _quizRepoMock.Setup(r => r.GetByIdAsync(11)).ReturnsAsync(quiz);

            var dto = new CreateQuestionDto { Text = "Q", Marks = 2, Options = new List<string> { "a", "b" }, CorrectAnswerIndex = 5 };

            var res = await _service.AddQuestionAsync(11, dto);

            using var doc = ToJsonDoc(res);
            Assert.True(doc.RootElement.TryGetProperty("message", out var msg));
            Assert.Equal("CorrectAnswerIndex is out of range.", msg.GetString());

            _questionRepoMock.Verify(r => r.AddAsync(It.IsAny<Question>()), Times.Never);
        }

        [Fact]
        public async Task AddQuestionAsync_OnSuccess_AddsQuestionAndUpdatesQuizTotalMarks()
        {
            var quiz = new Quiz { Id = 20, Questions = new List<Question>(), TotalMarks = 3 };
            _quizRepoMock.Setup(r => r.GetByIdAsync(20)).ReturnsAsync(quiz);

            _questionRepoMock.Setup(q => q.AddAsync(It.IsAny<Question>())).Returns(Task.CompletedTask)
                .Callback<Question>(question => question.Id = 555);
            _questionRepoMock.Setup(q => q.SaveChangesAsync()).Returns(Task.CompletedTask);

            _quizRepoMock.Setup(q => q.SaveChangesAsync()).Returns(Task.CompletedTask);

            var dto = new CreateQuestionDto
            {
                Text = "New Q",
                Marks = 4,
                Options = new List<string> { "opt1", "opt2", "opt3" },
                CorrectAnswerIndex = 1
            };

            var res = await _service.AddQuestionAsync(20, dto);

            using var doc = ToJsonDoc(res);

            Assert.True(doc.RootElement.TryGetProperty("message", out var msg));
            Assert.Equal("Question added successfully.", msg.GetString());
            Assert.True(doc.RootElement.TryGetProperty("questionId", out var qid));
            Assert.Equal(555, qid.GetInt32());

            // quiz total marks updated
            Assert.Equal(7, quiz.TotalMarks); // 3 + 4

            _questionRepoMock.Verify(q => q.AddAsync(It.IsAny<Question>()), Times.Once);
            _questionRepoMock.Verify(q => q.SaveChangesAsync(), Times.Once);
            _quizRepoMock.Verify(q => q.SaveChangesAsync(), Times.AtLeastOnce);
        }

        // ---------- GetQuizForModule ----------
        [Fact]
        public async Task GetQuizForModuleAsync_WhenNoQuiz_ReturnsNull()
        {
            _quizRepoMock.Setup(q => q.GetByModuleIdAsync(It.IsAny<int>())).ReturnsAsync((Quiz?)null);

            var res = await _service.GetQuizForModuleAsync(123, Guid.NewGuid());

            Assert.Null(res);
        }

        [Fact]
        public async Task GetQuizForModuleAsync_WhenPreviousBestPassed_ReturnsAlreadyPassedTrue()
        {
            var userId = Guid.NewGuid();
            var quiz = new Quiz
            {
                Id = 300,
                ModuleId = 12,
                Title = "T",
                TimeLimitMinutes = 10,
                Questions = new List<Question>
                {
                    new Question { Id = 1, Marks = 10, Answers = new List<Answer> { new Answer { Id = 11, IsCorrect = true } } },
                    new Question { Id = 2, Marks = 10, Answers = new List<Answer> { new Answer { Id = 21, IsCorrect = true } } }
                }
            };

            _quizRepoMock.Setup(q => q.GetByModuleIdAsync(12)).ReturnsAsync(quiz);

            var previous = new QuizSubmission { Id = Guid.NewGuid(), QuizId = quiz.Id, UserId = userId, Score = 18 }; // 18/20 -> 90%
            _submissionRepoMock.Setup(s => s.GetBestSubmissionAsync(quiz.Id, userId)).ReturnsAsync(previous);

            var res = await _service.GetQuizForModuleAsync(12, userId);

            // result is anonymous object; serialize & inspect
            using var doc = ToJsonDoc(res);
            Assert.True(doc.RootElement.TryGetProperty("AlreadyPassed", out var ap));
            Assert.True(ap.GetBoolean());
            Assert.Equal(20, doc.RootElement.GetProperty("TotalMarks").GetInt32());
        }

        [Fact]
        public async Task GetQuizForModuleAsync_WhenPreviousBestNotPassed_ReturnsAlreadyPassedFalse()
        {
            var userId = Guid.NewGuid();
            var quiz = new Quiz
            {
                Id = 301,
                ModuleId = 13,
                Title = "T2",
                TimeLimitMinutes = 15,
                Questions = new List<Question>
                {
                    new Question { Id = 1, Marks = 5, Answers = new List<Answer> { new Answer { Id = 11, IsCorrect = true } } },
                    new Question { Id = 2, Marks = 5, Answers = new List<Answer> { new Answer { Id = 21, IsCorrect = true } } }
                }
            };

            _quizRepoMock.Setup(q => q.GetByModuleIdAsync(13)).ReturnsAsync(quiz);

            var previous = new QuizSubmission { Id = Guid.NewGuid(), QuizId = quiz.Id, UserId = userId, Score = 5 }; // 5/10 -> 50%
            _submissionRepoMock.Setup(s => s.GetBestSubmissionAsync(quiz.Id, userId)).ReturnsAsync(previous);

            var res = await _service.GetQuizForModuleAsync(13, userId);

            using var doc = ToJsonDoc(res);
            Assert.True(doc.RootElement.TryGetProperty("AlreadyPassed", out var ap));
            Assert.False(ap.GetBoolean());
            Assert.Equal(10, doc.RootElement.GetProperty("TotalMarks").GetInt32());
        }

        // ---------- SubmitQuiz ----------
        [Fact]
        public async Task SubmitQuizAsync_WhenQuizNotFound_ReturnsMessage()
        {
            _quizRepoMock.Setup(q => q.GetByIdWithDetailsAsync(It.IsAny<int>())).ReturnsAsync((Quiz?)null);

            var dto = new SubmitQuizDto { QuizId = 11, UserId = Guid.NewGuid(), Answers = new List<SubmitAnswerDto>() };

            var res = await _service.SubmitQuizAsync(dto);

            Assert.False(res is QuizResultDto);
            using var doc = ToJsonDoc(res);
            Assert.True(doc.RootElement.TryGetProperty("message", out var msg));
            Assert.Equal("Quiz not found.", msg.GetString());
        }

        [Fact]
        public async Task SubmitQuizAsync_WhenPreviousAlreadyPassed_ReturnsQuizResultDtoWithAlreadyPassedTrue()
        {
            var quiz = new Quiz
            {
                Id = 400,
                Questions = new List<Question>
                {
                    new Question { Id = 1, Marks = 5, Answers = new List<Answer> { new Answer { Id = 101, IsCorrect = true } } },
                    new Question { Id = 2, Marks = 5, Answers = new List<Answer> { new Answer { Id = 201, IsCorrect = true } } }
                }
            };

            _quizRepoMock.Setup(q => q.GetByIdWithDetailsAsync(400)).ReturnsAsync(quiz);

            var previous = new QuizSubmission { Id = Guid.NewGuid(), QuizId = 400, UserId = Guid.NewGuid(), Score = 10 };
            _submissionRepoMock.Setup(s => s.GetBestSubmissionAsync(400, It.IsAny<Guid>())).ReturnsAsync(previous);

            var dto = new SubmitQuizDto { QuizId = 400, UserId = Guid.NewGuid(), Answers = new List<SubmitAnswerDto>() };

            var res = await _service.SubmitQuizAsync(dto);

            var quizResult = Assert.IsType<QuizResultDto>(res);
            Assert.True(quizResult.AlreadyPassed);
            Assert.True(quizResult.Passed);
            Assert.Equal(previous.Id, quizResult.SubmissionId);
            Assert.Equal(10, quizResult.ObtainedMarks);
        }

        [Fact]
        public async Task SubmitQuizAsync_NormalFlow_CalculatesScoreAndSavesSubmission()
        {
            var quiz = new Quiz
            {
                Id = 500,
                Questions = new List<Question>
                {
                    new Question
                    {
                        Id = 1,
                        Marks = 3,
                        Answers = new List<Answer>
                        {
                            new Answer { Id = 1001, IsCorrect = true },
                            new Answer { Id = 1002, IsCorrect = false }
                        }
                    },
                    new Question
                    {
                        Id = 2,
                        Marks = 2,
                        Answers = new List<Answer>
                        {
                            new Answer { Id = 2001, IsCorrect = true },
                            new Answer { Id = 2002, IsCorrect = false }
                        }
                    }
                }
            };

            _quizRepoMock.Setup(q => q.GetByIdWithDetailsAsync(500)).ReturnsAsync(quiz);

            _submissionRepoMock.Setup(s => s.GetBestSubmissionAsync(500, It.IsAny<Guid>())).ReturnsAsync((QuizSubmission?)null);

            // intercept AddAsync to set Id
            _submissionRepoMock.Setup(s => s.AddAsync(It.IsAny<QuizSubmission>()))
                .Returns(Task.CompletedTask)
                .Callback<QuizSubmission>(s => s.Id = Guid.NewGuid());

            _submissionRepoMock.Setup(s => s.SaveChangesAsync()).Returns(Task.CompletedTask);

            var dto = new SubmitQuizDto
            {
                QuizId = 500,
                UserId = Guid.NewGuid(),
                Answers = new List<SubmitAnswerDto>
                {
                    new SubmitAnswerDto { QuestionId = 1, SelectedAnswerId = 1001 }, // correct => +3
                    new SubmitAnswerDto { QuestionId = 2, SelectedAnswerId = 2002 } // wrong => +0
                }
            };

            var res = await _service.SubmitQuizAsync(dto);

            var resultDto = Assert.IsType<QuizResultDto>(res);

            Assert.Equal(2, resultDto.TotalQuestions);
            Assert.Equal(5, resultDto.TotalMarks);
            Assert.Equal(3, resultDto.ObtainedMarks);
            Assert.Equal(1, resultDto.CorrectAnswers);
            Assert.False(resultDto.AlreadyPassed);

            _submissionRepoMock.Verify(s => s.AddAsync(It.IsAny<QuizSubmission>()), Times.Once);
            _submissionRepoMock.Verify(s => s.SaveChangesAsync(), Times.Once);
        }

        // ---------- GetSubmissionResult ----------
        [Fact]
        public async Task GetSubmissionResultAsync_WhenNoSubmission_ReturnsNull()
        {
            _submissionRepoMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((QuizSubmission?)null);

            var res = await _service.GetSubmissionResultAsync(Guid.NewGuid());

            Assert.Null(res);
        }

        [Fact]
        public async Task GetSubmissionResultAsync_ReturnsQuizResultDto_WithCorrectCalculation()
        {
            var submissionId = Guid.NewGuid();
            var submission = new QuizSubmission
            {
                Id = submissionId,
                QuizId = 700,
                UserId = Guid.NewGuid(),
                Score = 8
            };

            var quiz = new Quiz
            {
                Id = 700,
                TotalMarks = 10,
                Questions = new List<Question>()
            };

            _submissionRepoMock.Setup(s => s.GetByIdAsync(submissionId)).ReturnsAsync(submission);
            _quizRepoMock.Setup(q => q.GetByIdAsync(700)).ReturnsAsync(quiz);

            var res = await _service.GetSubmissionResultAsync(submissionId);

            var dto = Assert.IsType<QuizResultDto>(res);
            Assert.Equal(10, dto.TotalMarks);
            Assert.Equal(8, dto.ObtainedMarks);
            Assert.Equal(Math.Round((decimal)8 / 10 * 100m, 2), dto.Percentage);
            Assert.True(dto.AlreadyPassed);
        }

        // ---------- GetAllQuizzes ----------
        [Fact]
        public async Task GetAllQuizzesAsync_ReturnsList()
        {
            var list = new List<Quiz>
            {
                new Quiz { Id = 1, Title = "A" },
                new Quiz { Id = 2, Title = "B" }
            };

            _quizRepoMock.Setup(q => q.GetAllAsync()).ReturnsAsync(list);

            var res = await _service.GetAllQuizzesAsync();

            Assert.NotNull(res);
            var arr = res.ToList();
            Assert.Equal(2, arr.Count);
        }
    }
}
