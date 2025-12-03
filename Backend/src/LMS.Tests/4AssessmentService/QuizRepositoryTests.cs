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

            // Setup AutoFixture instance
            _fixture = new Fixture();

            // --- FIX CIRCULAR REFERENCES ---
            _fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));

            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        // ------------------------------------------------------
        // HELPERS
        // ------------------------------------------------------
        private Quiz CreateQuiz() =>
            _fixture.Build<Quiz>()
                .Without(q => q.Questions)
                .Create();

        private Question CreateQuestion() =>
            _fixture.Build<Question>()
                .Without(q => q.Answers)
                .Without(q => q.Quiz)
                .Create();

        private Answer CreateAnswer() =>
            _fixture.Build<Answer>()
                .Without(a => a.Question)
                .Create();

        // ---------------------------------------------------------
        // ADD QUIZ
        // ---------------------------------------------------------
        [Fact]
        public async Task AddAsync_ShouldAddQuizToDatabase()
        {
            var quiz = CreateQuiz();
            quiz.Title = "C# Basics";

            await _repo.AddAsync(quiz);
            await _repo.SaveChangesAsync();

            Assert.Single(AssessmentContext.Quizzes);
            Assert.Equal("C# Basics", AssessmentContext.Quizzes.First().Title);
        }

        // ---------------------------------------------------------
        // GET BY MODULE ID
        // ---------------------------------------------------------
        [Fact]
        public async Task GetByModuleIdAsync_ShouldReturnQuizWithQuestionsAndAnswers()
        {
            var quiz = CreateQuiz();
            quiz.ModuleId = 20;
            quiz.Title = "Math Quiz";

            var question = CreateQuestion();
            var answer = CreateAnswer();

            // Attach children manually
            question.Answers = new List<Answer> { answer };
            quiz.Questions = new List<Question> { question };

            await _repo.AddAsync(quiz);
            await _repo.SaveChangesAsync();

            var result = await _repo.GetByModuleIdAsync(20);

            Assert.NotNull(result);
            Assert.Single(result!.Questions);
            Assert.Single(result.Questions.First().Answers);
        }

        // ---------------------------------------------------------
        // GET BY ID WITH DETAILS
        // ---------------------------------------------------------
        [Fact]
        public async Task GetByIdWithDetailsAsync_ShouldReturnQuizWithQuestions()
        {
            var quiz = CreateQuiz();
            quiz.ModuleId = 30;
            quiz.Title = "Physics";

            quiz.Questions = new List<Question> { CreateQuestion() };

            await _repo.AddAsync(quiz);
            await _repo.SaveChangesAsync();

            var result = await _repo.GetByIdWithDetailsAsync(quiz.Id);

            Assert.NotNull(result);
            Assert.Single(result!.Questions);
        }

        // ---------------------------------------------------------
        // GET ALL
        // ---------------------------------------------------------
        [Fact]
        public async Task GetAllAsync_ShouldReturnAllQuizzes()
        {
            await _repo.AddAsync(CreateQuiz());
            await _repo.AddAsync(CreateQuiz());
            await _repo.SaveChangesAsync();

            var list = (await _repo.GetAllAsync()).ToList();

            Assert.Equal(2, list.Count);
        }

        // ---------------------------------------------------------
        // GET BY ID
        // ---------------------------------------------------------
        [Fact]
        public async Task GetByIdAsync_ShouldReturnCorrectQuiz()
        {
            var quiz = CreateQuiz();
            quiz.Title = "Special Quiz";

            await _repo.AddAsync(quiz);
            await _repo.SaveChangesAsync();

            var result = await _repo.GetByIdAsync(quiz.Id);

            Assert.NotNull(result);
            Assert.Equal("Special Quiz", result!.Title);
        }
    }
}
