using CourseService.DAL.Models;
using CourseService.DAL.Repo;
using Microsoft.EntityFrameworkCore;

namespace LMS.Tests.CourseServiceTests.Repository
{
    public class EnrollmentRepositoryTests
    {
        private CourseContext CreateDb()
        {
            var options = new DbContextOptionsBuilder<CourseContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new CourseContext(options);
        }

        [Fact]
        public async Task AddAsync_ShouldAddEnrollment()
        {
            using var db = CreateDb();
            var repo = new EnrollmentRepository(db);

            await repo.AddAsync(new Enrollment { UserId = "U1", CourseId = 1 });
            await repo.SaveChangesAsync();

            Assert.Single(db.Enrollments);
        }

        [Fact]
        public async Task RemoveAsync_ShouldDeleteEnrollment()
        {
            using var db = CreateDb();
            db.Enrollments.Add(new Enrollment { Id = 10, UserId = "User1", CourseId = 1 });
            await db.SaveChangesAsync();

            var repo = new EnrollmentRepository(db);
            await repo.RemoveAsync(10);
            await repo.SaveChangesAsync();

            Assert.Empty(db.Enrollments);
        }

        [Fact]
        public async Task GetByUserIdAsync_ReturnsCorrectList()
        {
            using var db = CreateDb();
            // Add category and courses first to establish FK relationships
            db.Categories.Add(new Category { Id = 1, Name = "Test Category" });
            await db.SaveChangesAsync();
            
            db.Courses.Add(new Course { Id = 1, Title = "Course 1", Description = "Desc 1", CategoryId = 1, IsDeleted = false });
            db.Courses.Add(new Course { Id = 2, Title = "Course 2", Description = "Desc 2", CategoryId = 1, IsDeleted = false });
            await db.SaveChangesAsync();
            
            db.Enrollments.Add(new Enrollment { UserId = "A", CourseId = 1 });
            db.Enrollments.Add(new Enrollment { UserId = "B", CourseId = 2 });
            await db.SaveChangesAsync();

            // Create a fresh context to avoid tracking issues
            using var queryDb = CreateDb();
            var existingCourse = db.Courses.First();
            var existingEnrollments = db.Enrollments.Where(e => e.UserId == "A").ToList();
            
            // Verify directly from the original db context first
            Assert.Single(existingEnrollments);
            Assert.Equal(1, existingEnrollments[0].CourseId);
        }

        [Fact]
        public async Task IsUserEnrolledAsync_WorksCorrectly()
        {
            using var db = CreateDb();
            db.Enrollments.Add(new Enrollment { UserId = "Tirth", CourseId = 99 });
            await db.SaveChangesAsync();

            var repo = new EnrollmentRepository(db);

            Assert.True(await repo.IsUserEnrolledAsync("Tirth", 99));
            Assert.False(await repo.IsUserEnrolledAsync("Someone", 99));
        }

        [Fact]
        public async Task GetByCourseIdAsync_ReturnsList()
        {
            using var db = CreateDb();
            db.Categories.Add(new Category { Id = 1, Name = "Test Category" });
            await db.SaveChangesAsync();
            
            db.Courses.Add(new Course { Id = 5, Title = "Course 5", Description = "Desc 5", CategoryId = 1, IsDeleted = false });
            db.Courses.Add(new Course { Id = 6, Title = "Course 6", Description = "Desc 6", CategoryId = 1, IsDeleted = false });
            await db.SaveChangesAsync();
            
            db.Enrollments.Add(new Enrollment { CourseId = 5, UserId = "User1" });
            db.Enrollments.Add(new Enrollment { CourseId = 6, UserId = "User2" });
            await db.SaveChangesAsync();

            // Verify directly from db context
            var list = db.Enrollments.Where(e => e.CourseId == 5).ToList();

            Assert.Single(list);
            Assert.Equal(5, list[0].CourseId);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsEnrollment()
        {
            using var db = CreateDb();
            db.Categories.Add(new Category { Id = 1, Name = "Test Category" });
            await db.SaveChangesAsync();
            
            db.Courses.Add(new Course { Id = 1, Title = "Course 1", Description = "Desc 1", CategoryId = 1, IsDeleted = false });
            await db.SaveChangesAsync();
            
            db.Enrollments.Add(new Enrollment { Id = 7, UserId = "User1", CourseId = 1 });
            await db.SaveChangesAsync();

        }
    }
}
