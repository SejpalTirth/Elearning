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

            // FIX CIRCULAR REFERENCES
            _fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));

            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        // -------------------------------------------------------
        // FACTORY: CLEAN SUBMISSION (NO NAVIGATION PROPERTIES)
        // -------------------------------------------------------
        private QuizSubmission CreateSubmission(Guid? userId = null, int? quizId = null, int? score = null)
        {
            var submission = _fixture.Build<QuizSubmission>()
                .Without(s => s.Quiz)
                .Create();

            if (userId != null) submission.UserId = userId.Value;
            if (quizId != null) submission.QuizId = quizId.Value;
            if (score != null) submission.Score = score.Value;

            return submission;
        }

        // -------------------------------------------------------
        // ADD SUBMISSION
        // -------------------------------------------------------
        [Fact]
        public async Task AddAsync_ShouldInsertSubmission()
        {
            var submission = CreateSubmission(score: 45);

            await _repo.AddAsync(submission);
            await _repo.SaveChangesAsync();

            var stored = AssessmentContext.QuizSubmissions.First();
            Assert.Equal(45, stored.Score);
        }

        // -------------------------------------------------------
        // BEST SUBMISSION — RETURNS HIGHEST SCORE
        // -------------------------------------------------------
        [Fact]
        public async Task GetBestSubmissionAsync_ShouldReturnHighestScore()
        {
            var userId = Guid.NewGuid();

            var s1 = CreateSubmission(userId, quizId: 5, score: 20);
            var s2 = CreateSubmission(userId, quizId: 5, score: 90);
            var s3 = CreateSubmission(userId, quizId: 5, score: 60);

            await _repo.AddAsync(s1);
            await _repo.AddAsync(s2);
            await _repo.AddAsync(s3);
            await _repo.SaveChangesAsync();

            var best = await _repo.GetBestSubmissionAsync(5, userId);

            Assert.NotNull(best);
            Assert.Equal(90, best!.Score);
        }

        // -------------------------------------------------------
        // BEST SUBMISSION — RETURNS NULL WHEN NO DATA
        // -------------------------------------------------------
        [Fact]
        public async Task GetBestSubmissionAsync_ShouldReturnNull_WhenNoSubmissions()
        {
            var result = await _repo.GetBestSubmissionAsync(10, Guid.NewGuid());
            Assert.Null(result);
        }

        // -------------------------------------------------------
        // GET BY ID — RETURNS CORRECT SUBMISSION
        // -------------------------------------------------------
        [Fact]
        public async Task GetByIdAsync_ShouldReturnSubmission_WhenExists()
        {
            var submission = CreateSubmission(score: 33);

            await _repo.AddAsync(submission);
            await _repo.SaveChangesAsync();

            var found = await _repo.GetByIdAsync(submission.Id);

            Assert.NotNull(found);
            Assert.Equal(33, found!.Score);
        }

        // -------------------------------------------------------
        // GET BY ID — RETURN NULL WHEN NOT FOUND
        // -------------------------------------------------------
        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenMissing()
        {
            var result = await _repo.GetByIdAsync(Guid.NewGuid());
            Assert.Null(result);
        }
    }
}
