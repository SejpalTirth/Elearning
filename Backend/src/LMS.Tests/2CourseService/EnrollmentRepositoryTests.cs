using AutoFixture;
using CourseService.DAL.Models;
using CourseService.DAL.Repo;
using Microsoft.Extensions.DependencyInjection;

namespace LMS.Tests.CourseService.Repository
{
    public class EnrollmentRepositoryTests : BaseTest
    {
        private readonly EnrollmentRepository _repo;
        private readonly Fixture _fixture;

        public EnrollmentRepositoryTests()
        {
            _fixture = new Fixture();

            // Prevent circular graph creation
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _repo = new EnrollmentRepository(CourseContext);

            SeedCategory();
        }

        // --------------------------------------------------------------------
        // SEED REQUIRED CATEGORY (clean, no Courses populated)
        // --------------------------------------------------------------------
        private void SeedCategory()
        {
            var category = _fixture.Build<Category>()
                .Without(c => c.Courses)      // prevent EF graph pollution
                .Create();

            CourseContext.Categories.Add(category);
            CourseContext.SaveChanges();
        }

        private int CategoryId => CourseContext.Categories.First().Id;

        // --------------------------------------------------------------------
        // CLEAN FACTORY: Course (NO navigation properties)
        // --------------------------------------------------------------------
        private Course CreateCourse(int id)
        {
            return _fixture.Build<Course>()
                .With(c => c.Id, id)
                .With(c => c.CategoryId, CategoryId)
                .Without(c => c.Category)
                .Without(c => c.Modules)
                .Without(c => c.Enrollments)
                .Create();
        }

        // --------------------------------------------------------------------
        // CLEAN FACTORY: Enrollment (NO navigation Course)
        // --------------------------------------------------------------------
        private Enrollment CreateEnrollment(string userId, int courseId, int? id = null)
        {
            var e = _fixture.Build<Enrollment>()
                .With(e => e.UserId, userId)
                .With(e => e.CourseId, courseId)
                .Without(e => e.Course)
                .Create();

            if (id != null)
                e.Id = id.Value;

            return e;
        }

        protected override void ConfigureGatewayDependencies(IServiceCollection services)
        {
            // Gateway not needed for CourseService tests
        }

        // ====================================================================
        // ADD
        // ====================================================================
        [Fact]
        public async Task AddAsync_ShouldAddEnrollment()
        {
            var course = CreateCourse(1);
            CourseContext.Courses.Add(course);
            CourseContext.SaveChanges();

            var enrollment = CreateEnrollment("U1", 1);

            await _repo.AddAsync(enrollment);
            await _repo.SaveChangesAsync();

            Assert.Single(CourseContext.Enrollments);
        }

        // ====================================================================
        // REMOVE
        // ====================================================================
        [Fact]
        public async Task RemoveAsync_ShouldDeleteEnrollment()
        {
            var course = CreateCourse(1);
            CourseContext.Courses.Add(course);
            CourseContext.SaveChanges();

            var enrollment = CreateEnrollment("User1", 1, id: 10);
            CourseContext.Enrollments.Add(enrollment);
            CourseContext.SaveChanges();

            await _repo.RemoveAsync(10);
            await _repo.SaveChangesAsync();

            Assert.Empty(CourseContext.Enrollments);
        }

        // ====================================================================
        // GET BY USER ID
        // ====================================================================
        [Fact]
        public async Task GetByUserIdAsync_ReturnsCorrectList()
        {
            var course1 = CreateCourse(1);
            var course2 = CreateCourse(2);

            CourseContext.Courses.AddRange(course1, course2);
            CourseContext.SaveChanges();

            var e1 = CreateEnrollment("A", 1);
            var e2 = CreateEnrollment("B", 2);

            CourseContext.Enrollments.AddRange(e1, e2);
            CourseContext.SaveChanges();

            var list = (await _repo.GetByUserIdAsync("A")).ToList();

            Assert.Single(list);
            Assert.Equal("A", list[0].UserId);
            Assert.Equal(1, list[0].CourseId);
        }

        // ====================================================================
        // IS USER ENROLLED
        // ====================================================================
        [Fact]
        public async Task IsUserEnrolledAsync_WorksCorrectly()
        {
            var course = CreateCourse(99);
            CourseContext.Courses.Add(course);
            CourseContext.SaveChanges();

            var enrollment = CreateEnrollment("Tirth", 99);
            CourseContext.Enrollments.Add(enrollment);
            CourseContext.SaveChanges();

            Assert.True(await _repo.IsUserEnrolledAsync("Tirth", 99));
            Assert.False(await _repo.IsUserEnrolledAsync("Someone", 99));
        }

        // ====================================================================
        // GET BY COURSE ID
        // ====================================================================
        [Fact]
        public async Task GetByCourseIdAsync_ReturnsList()
        {
            var course = CreateCourse(5);
            CourseContext.Courses.Add(course);
            CourseContext.SaveChanges();

            var e1 = CreateEnrollment("User1", 5);
            var e2 = CreateEnrollment("User2", 6);

            CourseContext.Enrollments.AddRange(e1, e2);
            CourseContext.SaveChanges();

            var list = (await _repo.GetByCourseIdAsync(5)).ToList();

            Assert.Single(list);
            Assert.Equal(5, list[0].CourseId);
        }

        // ====================================================================
        // GET BY ID
        // ====================================================================
        [Fact]
        public async Task GetByIdAsync_ReturnsEnrollment()
        {
            var course = CreateCourse(1);
            CourseContext.Courses.Add(course);
            CourseContext.SaveChanges();

            var enrollment = CreateEnrollment("User1", 1, id: 7);
            CourseContext.Enrollments.Add(enrollment);
            CourseContext.SaveChanges();

            var result = await _repo.GetByIdAsync(7);

            Assert.NotNull(result);
            Assert.Equal("User1", result!.UserId);
            Assert.Equal(1, result.CourseId);
        }
    }
}
