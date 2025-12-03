using Microsoft.EntityFrameworkCore;
using ProgresService.DAL.Data;
using ProgresService.DAL.Models;
using ProgressService.DAL.Repo;

namespace ProgressService.Tests
{
    public class ProgressRepositoryTests
    {
        private ProgressDbContext CreateInMemoryDb(string dbName)
        {
            var options = new DbContextOptionsBuilder<ProgressDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new ProgressDbContext(options);
        }

        // ---------------------------------------------------------
        // TEST 1: GetUserProgressAsync
        // ---------------------------------------------------------
        [Fact]
        public async Task GetUserProgressAsync_ShouldReturnOnlyUserSpecificRecords()
        {
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();

            var ctx = CreateInMemoryDb("GetUserProgressAsyncTest");
            ctx.ProgressTrackings.AddRange(
                new ProgressTracking { UserId = userId, CourseId = 1, ModuleId = 10 },
                new ProgressTracking { UserId = userId, CourseId = 1, ModuleId = 11 },
                new ProgressTracking { UserId = otherUserId, CourseId = 2, ModuleId = 20 }
            );
            await ctx.SaveChangesAsync();

            var repo = new ProgressRepository(ctx);

            var result = await repo.GetUserProgressAsync(userId);

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

            var ctx = CreateInMemoryDb("GetCompletedCourseCountAsyncTest");
            ctx.CourseCompletions.AddRange(
                new CourseCompletion { UserId = userId, CompletedAt = DateTime.UtcNow },
                new CourseCompletion { UserId = userId, CompletedAt = DateTime.UtcNow },
                new CourseCompletion { UserId = userId, CompletedAt = null } // Not completed
            );
            await ctx.SaveChangesAsync();

            var repo = new ProgressRepository(ctx);

            var count = await repo.GetCompletedCourseCountAsync(userId);

            Assert.Equal(2, count);
        }

        // ---------------------------------------------------------
        // TEST 3: DeleteByModuleIdsAsync
        // ---------------------------------------------------------
        [Fact]
        public async Task DeleteByModuleIdsAsync_ShouldDeleteOnlySpecifiedModules()
        {
            var ctx = CreateInMemoryDb("DeleteByModuleIdsAsyncTest");
            ctx.ProgressTrackings.AddRange(
                new ProgressTracking { ModuleId = 1 },
                new ProgressTracking { ModuleId = 2 },
                new ProgressTracking { ModuleId = 3 }
            );
            await ctx.SaveChangesAsync();

            var repo = new ProgressRepository(ctx);

            await repo.DeleteByModuleIdsAsync(new List<int> { 2, 3 });
            await ctx.SaveChangesAsync(); // important!

            var remaining = await ctx.ProgressTrackings.ToListAsync();

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

            var ctx = CreateInMemoryDb("MarkModuleComplete_Insert");
            var repo = new ProgressRepository(ctx);

            await repo.MarkModuleCompleteAsync(userId, 1, 10);

            var entry = await ctx.ProgressTrackings
                .FirstOrDefaultAsync(p => p.UserId == userId && p.ModuleId == 10);

            Assert.NotNull(entry);
            Assert.True(entry.IsCompleted);
            Assert.Equal(100, entry.ProgressPercent);
        }

        // ---------------------------------------------------------
        // TEST 5: MarkModuleCompleteAsync (Update existing record)
        // ---------------------------------------------------------
        [Fact]
        public async Task MarkModuleCompleteAsync_ShouldUpdateExistingRecord_WhenRecordExists()
        {
            var userId = Guid.NewGuid();

            var ctx = CreateInMemoryDb("MarkModuleComplete_Update");
            ctx.ProgressTrackings.Add(new ProgressTracking
            {
                UserId = userId,
                ModuleId = 10,
                ProgressPercent = 40,
                IsCompleted = false
            });

            await ctx.SaveChangesAsync();

            var repo = new ProgressRepository(ctx);

            await repo.MarkModuleCompleteAsync(userId, 1, 10);

            var entry = await ctx.ProgressTrackings
                .FirstOrDefaultAsync(p => p.UserId == userId && p.ModuleId == 10);

            Assert.NotNull(entry);
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

            var ctx = CreateInMemoryDb("GetCompletedModuleCountAsyncTest");
            ctx.ProgressTrackings.AddRange(
                new ProgressTracking { UserId = userId, CourseId = 1, IsCompleted = true },
                new ProgressTracking { UserId = userId, CourseId = 1, IsCompleted = false },
                new ProgressTracking { UserId = userId, CourseId = 1, IsCompleted = true }
            );
            await ctx.SaveChangesAsync();

            var repo = new ProgressRepository(ctx);

            var count = await repo.GetCompletedModuleCountAsync(userId, 1);

            Assert.Equal(2, count);
        }

        // ---------------------------------------------------------
        // TEST 7: IsCourseFullyCompletedAsync
        // Placeholder logic: returns true if any completed module exists
        // ---------------------------------------------------------
        [Fact]
        public async Task IsCourseFullyCompletedAsync_ShouldReturnTrue_IfAnyCompletedModuleExists()
        {
            var userId = Guid.NewGuid();

            var ctx = CreateInMemoryDb("IsCourseFullyCompletedAsyncTest");
            ctx.ProgressTrackings.Add(new ProgressTracking
            {
                UserId = userId,
                CourseId = 1,
                IsCompleted = true
            });
            await ctx.SaveChangesAsync();

            var repo = new ProgressRepository(ctx);

            var result = await repo.IsCourseFullyCompletedAsync(userId, 1);

            Assert.True(result);
        }

        [Fact]
        public async Task IsCourseFullyCompletedAsync_ShouldReturnFalse_IfNoCompletedModulesExist()
        {
            var userId = Guid.NewGuid();

            var ctx = CreateInMemoryDb("IsCourseFullyCompletedAsync_NoCompleted");
            ctx.ProgressTrackings.Add(new ProgressTracking
            {
                UserId = userId,
                CourseId = 1,
                IsCompleted = false
            });
            await ctx.SaveChangesAsync();

            var repo = new ProgressRepository(ctx);

            var result = await repo.IsCourseFullyCompletedAsync(userId, 1);

            Assert.False(result);
        }
    }
}
