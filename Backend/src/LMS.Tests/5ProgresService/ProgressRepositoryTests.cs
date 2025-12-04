using AutoFixture;
using Microsoft.EntityFrameworkCore;
using ProgressService.DAL.Repo;
using ProgresService.DAL.Models;

namespace LMS.Tests.ProgressService
{
    public class ProgressRepositoryTests : BaseTest
    {
        private readonly ProgressRepository _repo;
        private readonly IFixture _fixture;

        public ProgressRepositoryTests()
        {
            _repo = new ProgressRepository(ProgressContext);

            _fixture = new Fixture();

            // ---- FIX AUTO-FIXTURE RECURSION ISSUES ----
            _fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));

            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        private ProgressTracking CreateProgressTracking(Guid? userId = null, int? courseId = null, int? moduleId = null) =>
            _fixture.Build<ProgressTracking>()
                .With(p => p.UserId, userId ?? Guid.NewGuid())
                .With(p => p.CourseId, courseId ?? 1)
                .With(p => p.ModuleId, moduleId ?? 1)
                .Create();


        private CourseCompletion CreateCourseCompletion(Guid? userId = null, bool completed = true) =>
            _fixture.Build<CourseCompletion>()
                .With(c => c.UserId, userId ?? Guid.NewGuid())
                .With(c => c.CompletedAt, completed ? DateTime.UtcNow : (DateTime?)null)
                .Create();

        // ---------------------------------------------------------
        // TEST 1: GetUserProgressAsync
        // ---------------------------------------------------------
        [Fact]
        public async Task GetUserProgressAsync_ShouldReturnOnlyUserSpecificRecords()
        {
            var userId = Guid.NewGuid();
            var otherUser = Guid.NewGuid();

            ProgressContext.ProgressTrackings.AddRange(
                CreateProgressTracking(userId, 1, 10),
                CreateProgressTracking(userId, 1, 11),
                CreateProgressTracking(otherUser, 2, 99)
            );

            await ProgressContext.SaveChangesAsync();

            var result = await _repo.GetUserProgressAsync(userId);

            Assert.Equal(2, result.Count);
            Assert.All(result, r => Assert.Equal(userId, r.UserId));
        }

        // ---------------------------------------------------------
        // TEST 2: GetCompletedCourseCountAsync
        // ---------------------------------------------------------
        [Fact]
        public async Task GetCompletedCourseCountAsync_ShouldReturnCountOfCompletedCourses()
        {
            var userId = Guid.NewGuid();

            ProgressContext.CourseCompletions.AddRange(
                CreateCourseCompletion(userId, completed: true),
                CreateCourseCompletion(userId, completed: true),
                CreateCourseCompletion(userId, completed: false)
            );

            await ProgressContext.SaveChangesAsync();

            var count = await _repo.GetCompletedCourseCountAsync(userId);

            Assert.Equal(2, count);
        }

        // ---------------------------------------------------------
        // TEST 3: DeleteByModuleIdsAsync
        // ---------------------------------------------------------
        [Fact]
        public async Task DeleteByModuleIdsAsync_ShouldDeleteOnlySpecifiedModules()
        {
            ProgressContext.ProgressTrackings.AddRange(
                CreateProgressTracking(moduleId: 1),
                CreateProgressTracking(moduleId: 2),
                CreateProgressTracking(moduleId: 3)
            );

            await ProgressContext.SaveChangesAsync();

            await _repo.DeleteByModuleIdsAsync(new List<int> { 2, 3 });
            await ProgressContext.SaveChangesAsync();

            var remaining = await ProgressContext.ProgressTrackings.ToListAsync();

            Assert.Single(remaining);
            Assert.Equal(1, remaining[0].ModuleId);
        }

        // ---------------------------------------------------------
        // TEST 4: MarkModuleCompleteAsync (Insert new record)
        // ---------------------------------------------------------
        [Fact]
        public async Task MarkModuleCompleteAsync_ShouldInsertNewRecord_WhenRecordDoesNotExist()
        {
            var userId = Guid.NewGuid();

            await _repo.MarkModuleCompleteAsync(userId, 1, 10);

            var entry = await ProgressContext.ProgressTrackings
                .FirstOrDefaultAsync(p => p.UserId == userId && p.ModuleId == 10);

            Assert.NotNull(entry);
            Assert.True(entry.IsCompleted);
            Assert.Equal(100, entry.ProgressPercent);
        }

        // ---------------------------------------------------------
        // TEST 5: MarkModuleCompleteAsync (Update existing)
        // ---------------------------------------------------------
        [Fact]
        public async Task MarkModuleCompleteAsync_ShouldUpdateExistingRecord_WhenRecordExists()
        {
            var userId = Guid.NewGuid();

            ProgressContext.ProgressTrackings.Add(new ProgressTracking
            {
                UserId = userId,
                ModuleId = 10,
                CourseId = 1,
                ProgressPercent = 40,
                IsCompleted = false
            });

            await ProgressContext.SaveChangesAsync();

            await _repo.MarkModuleCompleteAsync(userId, 1, 10);

            var entry = await ProgressContext.ProgressTrackings
                .FirstAsync(p => p.UserId == userId && p.ModuleId == 10);

            Assert.True(entry.IsCompleted);
            Assert.Equal(100, entry.ProgressPercent);
        }

        // ---------------------------------------------------------
        // TEST 6: GetCompletedModuleCountAsync
        // ---------------------------------------------------------
        [Fact]
        public async Task GetCompletedModuleCountAsync_ShouldCountOnlyCompletedModules()
        {
            var userId = Guid.NewGuid();

            ProgressContext.ProgressTrackings.AddRange(
                new ProgressTracking { UserId = userId, CourseId = 1, IsCompleted = true },
                new ProgressTracking { UserId = userId, CourseId = 1, IsCompleted = false },
                new ProgressTracking { UserId = userId, CourseId = 1, IsCompleted = true }
            );

            await ProgressContext.SaveChangesAsync();

            var count = await _repo.GetCompletedModuleCountAsync(userId, 1);

            Assert.Equal(2, count);
        }

        // ---------------------------------------------------------
        // TEST 7: IsCourseFullyCompletedAsync
        // ---------------------------------------------------------
        [Fact]
        public async Task IsCourseFullyCompletedAsync_ShouldReturnTrue_IfAnyCompletedModuleExists()
        {
            var userId = Guid.NewGuid();

            ProgressContext.ProgressTrackings.Add(new ProgressTracking
            {
                UserId = userId,
                CourseId = 1,
                IsCompleted = true
            });

            await ProgressContext.SaveChangesAsync();

            var result = await _repo.IsCourseFullyCompletedAsync(userId, 1);

            Assert.True(result);
        }

        [Fact]
        public async Task IsCourseFullyCompletedAsync_ShouldReturnFalse_IfNoCompletedModulesExist()
        {
            var userId = Guid.NewGuid();

            ProgressContext.ProgressTrackings.Add(new ProgressTracking
            {
                UserId = userId,
                CourseId = 1,
                IsCompleted = false
            });

            await ProgressContext.SaveChangesAsync();

            var result = await _repo.IsCourseFullyCompletedAsync(userId, 1);

            Assert.False(result);
        }
    }
}
