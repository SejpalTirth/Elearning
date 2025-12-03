using AssessmentService.DAL;
using AssessmentService.DAL.Models;
using AssessmentService.DAL.Repo;
using Microsoft.EntityFrameworkCore;

namespace LMS.Tests.AssessmentService
{
    public class QuestionRepositoryTests
    {
        private AssessmentDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AssessmentDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AssessmentDbContext(options);
        }

        [Fact]
        public async Task AddAsync_ShouldAddQuestionToDatabase()
        {
            var db = GetDbContext();
            var repo = new QuestionRepository(db);

            var question = new Question
            {
                QuestionText = "What is 2+2?",
                QuestionType = "MCQ",
                Marks = 4
            };

            await repo.AddAsync(question);
            await repo.SaveChangesAsync();

            Assert.Equal(1, db.Questions.Count());
            Assert.Equal("What is 2+2?", db.Questions.First().QuestionText);
            Assert.Equal(4, db.Questions.First().Marks);
        }
    }
}
