using AutoFixture;
using AssessmentService.DAL.Models;
using AssessmentService.DAL.Repo;

namespace LMS.Tests.AssessmentService
{
    public class SubmissionRepositoryTests : BaseTest
    {
        private readonly SubmissionRepository _repo;
        private readonly IFixture _fixture;

        public SubmissionRepositoryTests()
        {
            _repo = new SubmissionRepository(AssessmentContext);

            _fixture = new Fixture();

            // ---- FIX AUTO-FIXTURE RECURSION ----
            _fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));

            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        // -------------------------------------------------------
        // HELPERS
        // -------------------------------------------------------
        private QuizSubmission CreateSubmission() =>
            _fixture.Build<QuizSubmission>()
                .Without(s => s.Quiz) // avoid navigation properties
                .Create();

        // -------------------------------------------------------
        // ADD SUBMISSION
        // -------------------------------------------------------
        [Fact]
        public async Task AddAsync_ShouldAddSubmission()
        {
            var submission = CreateSubmission();
            submission.Score = 50;

            await _repo.AddAsync(submission);
            await _repo.SaveChangesAsync();

            Assert.Single(AssessmentContext.QuizSubmissions);
            Assert.Equal(50, AssessmentContext.QuizSubmissions.First().Score);
        }

        // -------------------------------------------------------
        // GET BEST SUBMISSION
        // -------------------------------------------------------
        [Fact]
        public async Task GetBestSubmissionAsync_ShouldReturnHighestScore()
        {
            var userId = Guid.NewGuid();

            var s1 = CreateSubmission();
            s1.QuizId = 2;
            s1.UserId = userId;
            s1.Score = 20;

            var s2 = CreateSubmission();
            s2.QuizId = 2;
            s2.UserId = userId;
            s2.Score = 80;

            var s3 = CreateSubmission();
            s3.QuizId = 2;
            s3.UserId = userId;
            s3.Score = 50;

            await _repo.AddAsync(s1);
            await _repo.AddAsync(s2);
            await _repo.AddAsync(s3);
            await _repo.SaveChangesAsync();

            var result = await _repo.GetBestSubmissionAsync(2, userId);

            Assert.NotNull(result);
            Assert.Equal(80, result!.Score);
        }

        // -------------------------------------------------------
        // GET BY ID
        // -------------------------------------------------------
        [Fact]
        public async Task GetByIdAsync_ShouldReturnCorrectSubmission()
        {
            var submission = CreateSubmission();
            submission.Score = 15;

            await _repo.AddAsync(submission);
            await _repo.SaveChangesAsync();

            var result = await _repo.GetByIdAsync(submission.Id);

            Assert.NotNull(result);
            Assert.Equal(15, result!.Score);
        }
    }
}
