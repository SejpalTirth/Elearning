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

            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));

            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _repo = new EnrollmentRepository(CourseContext);

            SeedCategory();
        }

        // --------------------------------------------------------------------
        // REQUIRED CATEGORY
        // --------------------------------------------------------------------
        private void SeedCategory()
        {
            var category = _fixture.Build<Category>()
                .Without(c => c.Courses)
                .Create();

            CourseContext.Categories.Add(category);
            CourseContext.SaveChanges();
        }

        private int CategoryId => CourseContext.Categories.First().Id;

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
            // no gateway dependencies 
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
            var saved = await _repo.SaveChangesAsync();

            Assert.True(saved);
            Assert.Single(CourseContext.Enrollments);
        }

        // ====================================================================
        // REMOVE
        // ====================================================================
        [Fact]
        public async Task RemoveAsync_ShouldRemoveEnrollment()
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
        // GET ALL
        // ====================================================================
        [Fact]
        public async Task GetAllAsync_ShouldReturnAllEnrollments()
        {
            var c1 = CreateCourse(1);
            var c2 = CreateCourse(2);

            CourseContext.Courses.AddRange(c1, c2);
            CourseContext.SaveChanges();

            CourseContext.Enrollments.Add(CreateEnrollment("A", 1));
            CourseContext.Enrollments.Add(CreateEnrollment("B", 2));
            CourseContext.SaveChanges();

            var result = await _repo.GetAllAsync();

            Assert.Equal(2, result.Count());
        }

        // ====================================================================
        // GET BY ID
        // ====================================================================
        [Fact]
        public async Task GetByIdAsync_ShouldReturnEnrollment_WhenExists()
        {
            var course = CreateCourse(1);
            CourseContext.Courses.Add(course);
            CourseContext.SaveChanges();

            var e = CreateEnrollment("User1", 1, id: 7);
            CourseContext.Enrollments.Add(e);
            CourseContext.SaveChanges();

            var result = await _repo.GetByIdAsync(7);

            Assert.NotNull(result);
            Assert.Equal("User1", result!.UserId);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
        {
            var result = await _repo.GetByIdAsync(999);

            Assert.Null(result);
        }

        // ====================================================================
        // GET BY USER ID
        // ====================================================================
        [Fact]
        public async Task GetByUserIdAsync_ShouldReturnCorrectEnrollments()
        {
            var c1 = CreateCourse(1);
            var c2 = CreateCourse(2);

            CourseContext.Courses.AddRange(c1, c2);
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
        // GET BY COURSE ID
        // ====================================================================
        [Fact]
        public async Task GetByCourseIdAsync_ShouldReturnCorrectEnrollments()
        {
            var c1 = CreateCourse(5);
            var c2 = CreateCourse(6);

            CourseContext.Courses.AddRange(c1, c2);
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
        // IS USER ENROLLED?
        // ====================================================================
        [Fact]
        public async Task IsUserEnrolledAsync_ShouldReturnCorrectValue()
        {
            var c = CreateCourse(99);
            CourseContext.Courses.Add(c);
            CourseContext.SaveChanges();

            var e = CreateEnrollment("Tirth", 99);
            CourseContext.Enrollments.Add(e);
            CourseContext.SaveChanges();

            Assert.True(await _repo.IsUserEnrolledAsync("Tirth", 99));
            Assert.False(await _repo.IsUserEnrolledAsync("Other", 99));
        }

        // ====================================================================
        // SAVE CHANGES
        // ====================================================================
        [Fact]
        public async Task SaveChangesAsync_ShouldReturnFalse_WhenNoChanges()
        {
            var result = await _repo.SaveChangesAsync();
            Assert.False(result);
        }
    }
}
