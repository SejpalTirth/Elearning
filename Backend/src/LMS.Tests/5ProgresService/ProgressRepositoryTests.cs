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

        private ProgressTracking CreateProgressTracking(
            Guid? userId = null,
            int? courseId = null,
            int? moduleId = null,
            bool isCompleted = false) =>
            _fixture.Build<ProgressTracking>()
                .With(p => p.UserId, userId ?? Guid.NewGuid())
                .With(p => p.CourseId, courseId ?? 1)
                .With(p => p.ModuleId, moduleId)
                .With(p => p.IsCompleted, isCompleted)
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
        // TEST 2: DeleteByModuleIdsAsync
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

            var remaining = await ProgressContext.ProgressTrackings.ToListAsync();

            Assert.Single(remaining);
            Assert.Equal(1, remaining[0].ModuleId);
        }

        // ---------------------------------------------------------
        // TEST 3: MarkModuleCompleteAsync (Insert)
        // ---------------------------------------------------------
        [Fact]
        public async Task MarkModuleCompleteAsync_ShouldInsertNewRecord_WhenRecordDoesNotExist()
        {
            var userId = Guid.NewGuid();

            await _repo.MarkModuleCompleteAsync(userId, 1, 10);

            var entry = await ProgressContext.ProgressTrackings
                .FirstOrDefaultAsync(p => p.UserId == userId && p.ModuleId == 10);

            Assert.NotNull(entry);
            Assert.True(entry!.IsCompleted);
            Assert.Equal(100, entry.ProgressPercent);
        }

        // ---------------------------------------------------------
        // TEST 4: MarkModuleCompleteAsync (Update)
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
        // TEST 5: GetCompletedModuleCountAsync
        // ---------------------------------------------------------
        [Fact]
        public async Task GetCompletedModuleCountAsync_ShouldCountOnlyCompletedModules()
        {
            var userId = Guid.NewGuid();

            ProgressContext.ProgressTrackings.AddRange(
                CreateProgressTracking(userId, 1, 1, true),
                CreateProgressTracking(userId, 1, 2, false),
                CreateProgressTracking(userId, 1, 3, true)
            );

            await ProgressContext.SaveChangesAsync();

            var count = await _repo.GetCompletedModuleCountAsync(userId, 1);

            Assert.Equal(2, count);
        }

        // ---------------------------------------------------------
        // TEST 6: IsCourseFullyCompletedAsync
        // ---------------------------------------------------------
        [Fact]
        public async Task IsCourseFullyCompletedAsync_ShouldReturnTrue_WhenCompletedModulesMeetTotal()
        {
            var userId = Guid.NewGuid();

            ProgressContext.ProgressTrackings.AddRange(
                CreateProgressTracking(userId, 1, 1, true),
                CreateProgressTracking(userId, 1, 2, true)
            );

            await ProgressContext.SaveChangesAsync();

            var result = await _repo.IsCourseFullyCompletedAsync(
                userId,
                courseId: 1,
                totalModuleCount: 2);

            Assert.True(result);
        }

        [Fact]
        public async Task IsCourseFullyCompletedAsync_ShouldReturnFalse_WhenCompletedModulesAreLessThanTotal()
        {
            var userId = Guid.NewGuid();

            ProgressContext.ProgressTrackings.Add(
                CreateProgressTracking(userId, 1, 1, true)
            );

            await ProgressContext.SaveChangesAsync();

            var result = await _repo.IsCourseFullyCompletedAsync(
                userId,
                courseId: 1,
                totalModuleCount: 2);

            Assert.False(result);
        }

        [Fact]
        public async Task IsCourseFullyCompletedAsync_ShouldReturnFalse_WhenTotalModuleCountIsZero()
        {
            var result = await _repo.IsCourseFullyCompletedAsync(
                Guid.NewGuid(),
                courseId: 1,
                totalModuleCount: 0);

            Assert.False(result);
        }
    }
}
