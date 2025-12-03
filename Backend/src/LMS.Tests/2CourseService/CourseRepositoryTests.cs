using CourseService.DAL.Models;
using CourseService.DAL.Repo;
using Microsoft.EntityFrameworkCore;

namespace LMS.Tests.CourseServiceTests.Repository
{
    public class CourseRepositoryTests
    {
        private CourseContext CreateDb()
        {
            var options = new DbContextOptionsBuilder<CourseContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new CourseContext(options);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsNonDeletedCourses()
        {
            using var db = CreateDb();
            db.Categories.Add(new Category { Id = 1, Name = "Test Category" });
            await db.SaveChangesAsync();
            
            db.Courses.Add(new Course { Title = "A", Description = "Desc A", CategoryId = 1, IsDeleted = false });
            db.Courses.Add(new Course { Title = "B", Description = "Desc B", CategoryId = 1, IsDeleted = true });
            await db.SaveChangesAsync();

            var repo = new CourseRepository(db);
            var list = (await repo.GetAllAsync()).ToList();

            Assert.Single(list);
            Assert.Equal("A", list[0].Title);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsCourse_WhenExists()
        {
            using var db = CreateDb();
            db.Categories.Add(new Category { Id = 1, Name = "Test Category" });
            await db.SaveChangesAsync();
            
            db.Courses.Add(new Course { Id = 1, Title = "Test", Description = "Test description", CategoryId = 1, IsDeleted = false });
            await db.SaveChangesAsync();

            var repo = new CourseRepository(db);
            var result = await repo.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal("Test", result!.Title);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenDeleted()
        {
            using var db = CreateDb();
            db.Categories.Add(new Category { Id = 1, Name = "Test Category" });
            await db.SaveChangesAsync();
            
            db.Courses.Add(new Course { Id = 2, Title = "X", Description = "Deleted course", CategoryId = 1, IsDeleted = true });
            await db.SaveChangesAsync();

            var repo = new CourseRepository(db);
            var result = await repo.GetByIdAsync(2);

            Assert.Null(result);
        }

        [Fact]
        public async Task AddAsync_ShouldAddCourse()
        {
            using var db = CreateDb();
            db.Categories.Add(new Category { Id = 1, Name = "Test Category" });
            await db.SaveChangesAsync();
            
            var repo = new CourseRepository(db);

            await repo.AddAsync(new Course { Title = "New", Description = "New course description", CategoryId = 1, IsDeleted = false });
            await repo.SaveChangesAsync();

            Assert.Single(db.Courses);
        }

        [Fact]
        public async Task UpdateAsync_ShouldModifyCourse()
        {
            using var db = CreateDb();
            db.Categories.Add(new Category { Id = 1, Name = "Test Category" });
            await db.SaveChangesAsync();
            
            db.Courses.Add(new Course { Id = 1, Title = "Old", Description = "Old description", CategoryId = 1, IsDeleted = false });
            await db.SaveChangesAsync();

            var repo = new CourseRepository(db);
            var course = await repo.GetByIdAsync(1);
            course!.Title = "Updated";

            await repo.UpdateAsync(course);
            await db.SaveChangesAsync();

            Assert.Equal("Updated", db.Courses.First().Title);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsTrue_ForExistingCourse()
        {
            using var db = CreateDb();
            db.Categories.Add(new Category { Id = 1, Name = "Test Category" });
            await db.SaveChangesAsync();
            
            db.Courses.Add(new Course { Id = 3, Title = "Exists", Description = "Exists description", CategoryId = 1, IsDeleted = false });
            await db.SaveChangesAsync();

            var repo = new CourseRepository(db);
            Assert.True(await repo.ExistsAsync(3));
        }

        [Fact]
        public async Task GetByIdsAsync_ReturnsMatchingCourses()
        {
            using var db = CreateDb();
            db.Categories.Add(new Category { Id = 1, Name = "Test Category" });
            await db.SaveChangesAsync();
            
            db.Courses.Add(new Course { Id = 1, Title = "Course 1", Description = "Desc 1", CategoryId = 1, IsDeleted = false });
            db.Courses.Add(new Course { Id = 2, Title = "Course 2", Description = "Desc 2", CategoryId = 1, IsDeleted = false });
            await db.SaveChangesAsync();

            var repo = new CourseRepository(db);
            var list = (await repo.GetByIdsAsync(new List<int> { 1 })).ToList();

            Assert.Single(list);
            Assert.Equal(1, list[0].Id);
        }

        [Fact]
        public async Task GetByInstructorIdAsync_ReturnsCorrectData()
        {
            Guid instructor = Guid.NewGuid();

            using var db = CreateDb();
            db.Categories.Add(new Category { Id = 1, Name = "Test Category" });
            await db.SaveChangesAsync();
            
            db.Courses.Add(new Course { Id = 1, Title = "Instructor Course 1", InstructorUserId = instructor, Description = "Instructor 1", CategoryId = 1, IsDeleted = false });
            db.Courses.Add(new Course { Id = 2, Title = "Instructor Course 2", InstructorUserId = Guid.NewGuid(), Description = "Instructor 2", CategoryId = 1, IsDeleted = false });
            await db.SaveChangesAsync();

            var repo = new CourseRepository(db);
            var list = (await repo.GetByInstructorIdAsync(instructor)).ToList();

            Assert.Single(list);
            Assert.Equal(1, list[0].Id);
        }
    }
}
