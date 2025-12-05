using AutoFixture;
using CourseService.DAL.Models;
using CourseService.DAL.Repo;
using Microsoft.Extensions.DependencyInjection;

namespace LMS.Tests.CourseService.Repository
{
    public class CourseRepositoryTests : BaseTest
    {
        private readonly CourseRepository _repo;
        private readonly Fixture _fixture;

        public CourseRepositoryTests()
        {
            // -----------------------------
            // AutoFixture setup
            // -----------------------------
            _fixture = new Fixture();

            // Prevent EF circular recursion (Course → Category → Course…)
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            // -----------------------------
            // Seed a Category (needed for FK)
            // -----------------------------
            var category = _fixture.Build<Category>()
                .With(c => c.Name, "Test Category")
                .Without(c => c.Courses)
                .Create();

            CourseContext.Categories.Add(category);
            CourseContext.SaveChanges();

            // -----------------------------
            // Repo under test
            // -----------------------------
            _repo = new CourseRepository(CourseContext);
        }

        protected override void ConfigureGatewayDependencies(IServiceCollection services)
        {
            // No Gateway dependencies for CourseService
        }

        // Helper to create courses safely
        private Course CreateCourse(int? id = null, Guid? instructor = null, bool isDeleted = false)
        {
            var course = _fixture.Build<Course>()
                .Without(c => c.Category) // Prevent recursion
                .With(c => c.CategoryId, CourseContext.Categories.First().Id)
                .With(c => c.IsDeleted, isDeleted)
                .Create();

            if (id.HasValue) course.Id = id.Value;
            if (instructor.HasValue) course.InstructorUserId = instructor.Value;

            return course;
        }

        // -----------------------------------------------------
        // GetAllAsync – only non-deleted courses
        // -----------------------------------------------------
        [Fact]
        public async Task GetAllAsync_ReturnsNonDeletedCourses()
        {
            var catId = CourseContext.Categories.First().Id;

            var active = CreateCourse(isDeleted: false);
            var deleted = CreateCourse(isDeleted: true);

            CourseContext.Courses.AddRange(active, deleted);
            await CourseContext.SaveChangesAsync();

            var list = (await _repo.GetAllAsync()).ToList();

            Assert.Single(list);
            Assert.Equal(active.Title, list[0].Title);
        }

        // -----------------------------------------------------
        // GetByIdAsync – course exists
        // -----------------------------------------------------
        [Fact]
        public async Task GetByIdAsync_ReturnsCourse_WhenExists()
        {
            var course = CreateCourse(id: 1, isDeleted: false);

            CourseContext.Courses.Add(course);
            await CourseContext.SaveChangesAsync();

            var result = await _repo.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal(course.Title, result!.Title);
        }

        // -----------------------------------------------------
        // GetByIdAsync – returns null when deleted
        // -----------------------------------------------------
        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenDeleted()
        {
            var course = CreateCourse(id: 2, isDeleted: true);

            CourseContext.Courses.Add(course);
            await CourseContext.SaveChangesAsync();

            var result = await _repo.GetByIdAsync(2);

            Assert.Null(result);
        }

        // -----------------------------------------------------
        // AddAsync
        // -----------------------------------------------------
        [Fact]
        public async Task AddAsync_ShouldAddCourse()
        {
            var course = CreateCourse(isDeleted: false);

            await _repo.AddAsync(course);
            await CourseContext.SaveChangesAsync();

            Assert.Single(CourseContext.Courses);
        }

        // -----------------------------------------------------
        // UpdateAsync
        // -----------------------------------------------------
        [Fact]
        public async Task UpdateAsync_ShouldModifyCourse()
        {
            var course = CreateCourse(id: 1);
            CourseContext.Courses.Add(course);
            await CourseContext.SaveChangesAsync();

            course.Title = "Updated";
            await _repo.UpdateAsync(course);
            await CourseContext.SaveChangesAsync();

            Assert.Equal("Updated", CourseContext.Courses.First().Title);
        }

        // -----------------------------------------------------
        // ExistsAsync
        // -----------------------------------------------------
        [Fact]
        public async Task ExistsAsync_ReturnsTrue_ForExistingCourse()
        {
            var course = CreateCourse(id: 3);
            CourseContext.Courses.Add(course);
            await CourseContext.SaveChangesAsync();

            Assert.True(await _repo.ExistsAsync(3));
        }

        // -----------------------------------------------------
        // GetByIdsAsync
        // -----------------------------------------------------
        [Fact]
        public async Task GetByIdsAsync_ReturnsMatchingCourses()
        {
            var c1 = CreateCourse(id: 1);
            var c2 = CreateCourse(id: 2);

            CourseContext.Courses.AddRange(c1, c2);
            await CourseContext.SaveChangesAsync();

            var list = (await _repo.GetByIdsAsync(new List<int> { 1 })).ToList();

            Assert.Single(list);
            Assert.Equal(1, list[0].Id);
        }

        // -----------------------------------------------------
        // GetByInstructorIdAsync
        // -----------------------------------------------------
        [Fact]
        public async Task GetByInstructorIdAsync_ReturnsCorrectData()
        {
            Guid instructor = Guid.NewGuid();

            var matching = CreateCourse(id: 1, instructor: instructor);
            var other = CreateCourse(id: 2, instructor: Guid.NewGuid());

            CourseContext.Courses.AddRange(matching, other);
            await CourseContext.SaveChangesAsync();

            var list = (await _repo.GetByInstructorIdAsync(instructor)).ToList();

            Assert.Single(list);
            Assert.Equal(1, list[0].Id);
        }
    }
}
