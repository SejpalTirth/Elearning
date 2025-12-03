using AssessmentService.DAL;
using AssessmentService.DAL.Models;
using AssessmentService.DAL.Repo;
using Microsoft.EntityFrameworkCore;

namespace LMS.Tests.AssessmentService
{
    public class QuizRepositoryTests
    {
        private AssessmentDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AssessmentDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AssessmentDbContext(options);
        }

        [Fact]
        public async Task AddAsync_ShouldAddQuizToDatabase()
        {
            var db = GetDbContext();
            var repo = new QuizRepository(db);

            var quiz = new Quiz { ModuleId = 10, Title = "C# Basics" };

            await repo.AddAsync(quiz);
            await repo.SaveChangesAsync();

            Assert.Single(db.Quizzes);
            Assert.Equal("C# Basics", db.Quizzes.First().Title);
        }

        [Fact]
        public async Task GetByModuleIdAsync_ShouldReturnQuizWithQuestionsAndAnswers()
        {
            var db = GetDbContext();
            var repo = new QuizRepository(db);

            var quiz = new Quiz
            {
                ModuleId = 20,
                Title = "Math Quiz",
                Questions = new List<Question>
                {
                    new Question
                    {
                        QuestionText = "1 + 1?",
                        QuestionType = "MCQ",
                        Marks = 2,
                        Answers = new List<Answer>
                        {
                            new Answer { AnswerText = "2", IsCorrect = true }
                        }
                    }
                }
            };

            await repo.AddAsync(quiz);
            await repo.SaveChangesAsync();

            var result = await repo.GetByModuleIdAsync(20);

            Assert.NotNull(result);
            Assert.Single(result!.Questions);
            Assert.Single(result.Questions.First().Answers);
        }

        [Fact]
        public async Task GetByIdWithDetailsAsync_ShouldReturnQuizWithQuestions()
        {
            var db = GetDbContext();
            var repo = new QuizRepository(db);

            var quiz = new Quiz
            {
                ModuleId = 30,
                Title = "Physics",
                Questions = new List<Question>
                {
                    new Question { QuestionText = "Speed formula?",     QuestionType = "MCQ", Marks = 1 }
                }
            };

            await repo.AddAsync(quiz);
            await repo.SaveChangesAsync();

            var result = await repo.GetByIdWithDetailsAsync(quiz.Id);

            Assert.NotNull(result);
            Assert.Single(result!.Questions);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllQuizzes()
        {
            var db = GetDbContext();
            var repo = new QuizRepository(db);

            await repo.AddAsync(new Quiz { Title = "Q1" });
            await repo.AddAsync(new Quiz { Title = "Q2" });
            await repo.SaveChangesAsync();

            var result = (await repo.GetAllAsync()).ToList();

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnCorrectQuiz()
        {
            var db = GetDbContext();
            var repo = new QuizRepository(db);

            var quiz = new Quiz { Title = "Special Quiz" };

            await repo.AddAsync(quiz);
            await repo.SaveChangesAsync();

            var result = await repo.GetByIdAsync(quiz.Id);

            Assert.NotNull(result);
            Assert.Equal("Special Quiz", result!.Title);
        }
    }
}
