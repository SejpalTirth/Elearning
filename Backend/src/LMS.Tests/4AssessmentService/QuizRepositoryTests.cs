using AutoFixture;
using AssessmentService.DAL.Models;
using AssessmentService.DAL.Repo;

namespace LMS.Tests.AssessmentService
{
    public class QuizRepositoryTests : BaseTest
    {
        private readonly QuizRepository _repo;
        private readonly IFixture _fixture;

        public QuizRepositoryTests()
        {
            _repo = new QuizRepository(AssessmentContext);

            _fixture = new Fixture();

            // Remove recursion behavior for EF nav properties
            _fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));

            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        // ------------------------------------------------------
        // FACTORY HELPERS
        // ------------------------------------------------------
        private Quiz CreateQuiz(int? moduleId = null)
        {
            return _fixture.Build<Quiz>()
                .With(q => q.ModuleId, moduleId)
                .Without(q => q.Questions)
                .Create();
        }

        private Question CreateQuestion()
        {
            return _fixture.Build<Question>()
                .Without(q => q.Quiz)
                .Without(q => q.Answers)
                .Create();
        }

        private Answer CreateAnswer()
        {
            return _fixture.Build<Answer>()
                .Without(a => a.Question)
                .Create();
        }

        // ------------------------------------------------------
        // ADD
        // ------------------------------------------------------
        [Fact]
        public async Task AddAsync_ShouldAddQuizToDB()
        {
            var quiz = CreateQuiz();
            quiz.Title = "C# Basics";

            await _repo.AddAsync(quiz);
            await _repo.SaveChangesAsync();

            Assert.Single(AssessmentContext.Quizzes);
            Assert.Equal("C# Basics", AssessmentContext.Quizzes.First().Title);
        }

        // ------------------------------------------------------
        // GET BY MODULE ID
        // ------------------------------------------------------
        [Fact]
        public async Task GetByModuleIdAsync_ShouldReturnQuizWithQuestionsAndAnswers()
        {
            var quiz = CreateQuiz(moduleId: 20);

            var q1 = CreateQuestion();
            var ans = CreateAnswer();

            q1.Answers = new List<Answer> { ans };
            quiz.Questions = new List<Question> { q1 };

            await _repo.AddAsync(quiz);
            await _repo.SaveChangesAsync();

            var result = await _repo.GetByModuleIdAsync(20);

            Assert.NotNull(result);
            Assert.Single(result!.Questions);
            Assert.Single(result.Questions.First().Answers);
        }

        // ------------------------------------------------------
        // GET BY MODULE ID — MISSING RETURNS NULL
        // ------------------------------------------------------
        [Fact]
        public async Task GetByModuleIdAsync_WhenMissing_ShouldReturnNull()
        {
            var result = await _repo.GetByModuleIdAsync(999);
            Assert.Null(result);
        }

        // ------------------------------------------------------
        // GET BY ID WITH DETAILS
        // ------------------------------------------------------
        [Fact]
        public async Task GetByIdWithDetailsAsync_ShouldReturnQuizWithChildren()
        {
            var quiz = CreateQuiz(moduleId: 30);

            quiz.Questions = new List<Question>
            {
                CreateQuestion(),
                CreateQuestion()
            };

            await _repo.AddAsync(quiz);
            await _repo.SaveChangesAsync();

            var result = await _repo.GetByIdWithDetailsAsync(quiz.Id);

            Assert.NotNull(result);
            Assert.Equal(2, result!.Questions.Count);
        }

        // ------------------------------------------------------
        // GET BY ID WITH DETAILS — MISSING RETURNS NULL
        // ------------------------------------------------------
        [Fact]
        public async Task GetByIdWithDetailsAsync_WhenMissing_ShouldReturnNull()
        {
            var result = await _repo.GetByIdWithDetailsAsync(999);
            Assert.Null(result);
        }

        // ------------------------------------------------------
        // GET ALL
        // ------------------------------------------------------
        [Fact]
        public async Task GetAllAsync_ShouldReturnAllQuizzes()
        {
            await _repo.AddAsync(CreateQuiz(1));
            await _repo.AddAsync(CreateQuiz(2));
            await _repo.SaveChangesAsync();

            var list = (await _repo.GetAllAsync()).ToList();

            Assert.Equal(2, list.Count);
        }

        // ------------------------------------------------------
        // GET BY ID
        // ------------------------------------------------------
        [Fact]
        public async Task GetByIdAsync_ShouldReturnQuiz()
        {
            var quiz = CreateQuiz(100);
            quiz.Title = "Special Quiz";

            await _repo.AddAsync(quiz);
            await _repo.SaveChangesAsync();

            var result = await _repo.GetByIdAsync(quiz.Id);

            Assert.NotNull(result);
            Assert.Equal("Special Quiz", result!.Title);
        }

        // ------------------------------------------------------
        // GET BY ID — RETURNS NULL
        // ------------------------------------------------------
        [Fact]
        public async Task GetByIdAsync_WhenMissing_ShouldReturnNull()
        {
            var result = await _repo.GetByIdAsync(555);
            Assert.Null(result);
        }

        // ------------------------------------------------------
        // GET MODULE IDs WITH QUIZ (MISSING IN YOUR FILE)
        // ------------------------------------------------------
        [Fact]
        public async Task GetModuleIdsWithQuizAsync_ShouldReturnOnlyModulesThatHaveQuiz()
        {
            // moduleId = 10 → has quiz
            var quiz1 = CreateQuiz(10);

            // moduleId = 20 → has quiz
            var quiz2 = CreateQuiz(20);

            await _repo.AddAsync(quiz1);
            await _repo.AddAsync(quiz2);
            await _repo.SaveChangesAsync();

            var inputModules = new List<int> { 10, 20, 30 }; // 30 has no quiz

            var result = await _repo.GetModuleIdsWithQuizAsync(inputModules);

            Assert.Equal(2, result.Count);
            Assert.Contains(10, result);
            Assert.Contains(20, result);
            Assert.DoesNotContain(30, result);
        }

        // ------------------------------------------------------
        // EF ID GENERATED CHECK
        // ------------------------------------------------------
        [Fact]
        public async Task AddAsync_ShouldAssignDatabaseId()
        {
            var quiz = CreateQuiz(200);

            await _repo.AddAsync(quiz);
            await _repo.SaveChangesAsync();

            Assert.True(quiz.Id > 0);
        }
    }
}
