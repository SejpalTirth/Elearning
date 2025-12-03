using CourseService.DAL.Models;
using CourseService.DAL.Repo;
using Microsoft.EntityFrameworkCore;

namespace LMS.Tests.CourseServiceTests.Repository
{
    public class ModuleRepositoryTests
    {
        private CourseContext CreateDb()
        {
            var options = new DbContextOptionsBuilder<CourseContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new CourseContext(options);
        }

        [Fact]
        public async Task GetByCourseIdAsync_ReturnsModules()
        {
            using var db = CreateDb();
            db.Modules.Add(new Module { CourseId = 1, Title = "Module 1", Content = "Content 1" });
            db.Modules.Add(new Module { CourseId = 2, Title = "Module 2", Content = "Content 2" });
            await db.SaveChangesAsync();

            var repo = new ModuleRepository(db);
            var list = (await repo.GetByCourseIdAsync(1)).ToList();

            Assert.Single(list);
            Assert.Equal(1, list[0].CourseId);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsCorrectModule()
        {
            using var db = CreateDb();
            db.Modules.Add(new Module { Id = 5, Title = "Module Test", Content = "Test Content" });
            await db.SaveChangesAsync();

            var repo = new ModuleRepository(db);
            var module = await repo.GetByIdAsync(5);

            Assert.NotNull(module);
            Assert.Equal(5, module!.Id);
        }

        [Fact]
        public async Task AddAsync_ShouldAddModule()
        {
            using var db = CreateDb();
            var repo = new ModuleRepository(db);

            await repo.AddAsync(new Module { Title = "MTest", Content = "Module Test Content" });
            await repo.SaveChangesAsync();

            Assert.Single(db.Modules);
        }
    }
}
