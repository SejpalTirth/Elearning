using AssessmentService.DAL;
using AssessmentService.DAL.Models;
using AssessmentService.DAL.Repo;
using Microsoft.EntityFrameworkCore;

namespace LMS.Tests.AssessmentService
{
    public class SubmissionRepositoryTests
    {
        private AssessmentDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AssessmentDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AssessmentDbContext(options);
        }

        [Fact]
        public async Task AddAsync_ShouldAddSubmission()
        {
            var db = GetDbContext();
            var repo = new SubmissionRepository(db);

            var sub = new QuizSubmission
            {
                QuizId = 1,
                UserId = Guid.NewGuid(),
                Score = 50
            };

            await repo.AddAsync(sub);
            await repo.SaveChangesAsync();

            Assert.Single(db.QuizSubmissions);
            Assert.Equal(50, db.QuizSubmissions.First().Score);
        }

        [Fact]
        public async Task GetBestSubmissionAsync_ShouldReturnHighestScore()
        {
            var db = GetDbContext();
            var repo = new SubmissionRepository(db);
            var userId = Guid.NewGuid();

            await repo.AddAsync(new QuizSubmission { QuizId = 2, UserId = userId, Score = 20 });
            await repo.AddAsync(new QuizSubmission { QuizId = 2, UserId = userId, Score = 80 });
            await repo.AddAsync(new QuizSubmission { QuizId = 2, UserId = userId, Score = 50 });
            await repo.SaveChangesAsync();

            var result = await repo.GetBestSubmissionAsync(2, userId);

            Assert.NotNull(result);
            Assert.Equal(80, result!.Score);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnCorrectSubmission()
        {
            var db = GetDbContext();
            var repo = new SubmissionRepository(db);

            var sub = new QuizSubmission
            {
                QuizId = 3,
                UserId = Guid.NewGuid(),
                Score = 15
            };

            await repo.AddAsync(sub);
            await repo.SaveChangesAsync();

            var result = await repo.GetByIdAsync(sub.Id);

            Assert.NotNull(result);
            Assert.Equal(15, result!.Score);
        }
    }
}
