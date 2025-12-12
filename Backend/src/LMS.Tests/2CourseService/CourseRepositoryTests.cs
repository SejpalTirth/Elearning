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
            _fixture = new Fixture();

            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));

            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            // Seed category
            var category = _fixture.Build<Category>()
                .With(c => c.Name, "Test Category")
                .Without(c => c.Courses)
                .Create();

            CourseContext.Categories.Add(category);
            CourseContext.SaveChanges();

            _repo = new CourseRepository(CourseContext);
        }

        protected override void ConfigureGatewayDependencies(IServiceCollection services) { }

        private Course CreateCourse(
            int? id = null,
            Guid? instructor = null,
            bool isDeleted = false)
        {
            var c = _fixture.Build<Course>()
                .Without(c => c.Category)
                .With(c => c.CategoryId, CourseContext.Categories.First().Id)
                .With(c => c.IsDeleted, isDeleted)
                .Create();

            if (id.HasValue) c.Id = id.Value;
            if (instructor.HasValue) c.InstructorUserId = instructor.Value;
            return c;
        }

        // ----------------------------------------------------------------------------------------
        // EXISTING TESTS (your originals) untouched
        // ----------------------------------------------------------------------------------------

        [Fact]
        public async Task GetAllAsync_ReturnsNonDeletedCourses()
        {
            var active = CreateCourse(isDeleted: false);
            var deleted = CreateCourse(isDeleted: true);

            CourseContext.Courses.AddRange(active, deleted);
            await CourseContext.SaveChangesAsync();

            var list = (await _repo.GetAllAsync()).ToList();

            Assert.Single(list);
            Assert.Equal(active.Title, list[0].Title);
        }

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

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenDeleted()
        {
            var course = CreateCourse(id: 2, isDeleted: true);
            CourseContext.Courses.Add(course);
            await CourseContext.SaveChangesAsync();

            var result = await _repo.GetByIdAsync(2);

            Assert.Null(result);
        }

        [Fact]
        public async Task AddAsync_ShouldAddCourse()
        {
            var course = CreateCourse(id: 11, isDeleted: false);

            await _repo.AddAsync(course);
            await _repo.SaveChangesAsync();

            var count = CourseContext.Courses.Count();
            Assert.Equal(1, count);
        }

        [Fact]
        public async Task UpdateAsync_ShouldModifyCourse()
        {
            var course = CreateCourse(id: 1, isDeleted: false);
            CourseContext.Courses.Add(course);
            await CourseContext.SaveChangesAsync();

            course.Title = "Updated";
            await _repo.UpdateAsync(course);
            await CourseContext.SaveChangesAsync();

            Assert.Equal("Updated", CourseContext.Courses.First().Title);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsTrue_ForExistingCourse()
        {
            var c = CreateCourse(id: 3, isDeleted: false);
            CourseContext.Courses.Add(c);
            await CourseContext.SaveChangesAsync();

            Assert.True(await _repo.ExistsAsync(3));
        }

        [Fact]
        public async Task GetByIdsAsync_ReturnsMatchingCourses()
        {
            var c1 = CreateCourse(id: 1, isDeleted: false);
            var c2 = CreateCourse(id: 2, isDeleted: false);

            CourseContext.Courses.AddRange(c1, c2);
            await CourseContext.SaveChangesAsync();

            var list = (await _repo.GetByIdsAsync(new List<int> { 1 })).ToList();

            Assert.Single(list);
            Assert.Equal(1, list[0].Id);
        }

        [Fact]
        public async Task GetByInstructorIdAsync_ReturnsCorrectData()
        {
            Guid instructor = Guid.NewGuid();

            var c1 = CreateCourse(id: 1, instructor: instructor, isDeleted: false);
            var c2 = CreateCourse(id: 2, instructor: Guid.NewGuid(), isDeleted: false);

            CourseContext.Courses.AddRange(c1, c2);
            await CourseContext.SaveChangesAsync();

            var list = (await _repo.GetByInstructorIdAsync(instructor)).ToList();

            Assert.Single(list);
            Assert.Equal(1, list[0].Id);
        }

        // ----------------------------------------------------------------------------------------
        // 📌 NEW TESTS FOR FULL COVERAGE
        // ----------------------------------------------------------------------------------------

        [Fact]
        public async Task ExistsAsync_ReturnsFalse_WhenNotExists()
        {
            Assert.False(await _repo.ExistsAsync(9999));
        }

        [Fact]
        public async Task GetByIdsAsync_ReturnsEmpty_WhenNoneMatch()
        {
            var list = await _repo.GetByIdsAsync(new List<int> { 100, 200 });
            Assert.Empty(list);
        }

        [Fact]
        public async Task GetByInstructorIdAsync_ReturnsEmpty_WhenNoCourseMatches()
        {
            var result = await _repo.GetByInstructorIdAsync(Guid.NewGuid());
            Assert.Empty(result);
        }

        // ---------------- GetByIdWithModulesAsync ----------------
        [Fact]
        public async Task GetByIdWithModulesAsync_ReturnsCourseWithModules()
        {
            var courseId = 10;

            // Build a valid course with NO modules initially
            var course = _fixture.Build<Course>()
                .With(c => c.Id, courseId)
                .With(c => c.Description, "Test description") // FIX required field
                .Without(c => c.Modules)                      // prevent AutoFixture from filling list
                .Without(c => c.Enrollments)
                .Create();

            // Build exactly ONE module
            var module = _fixture.Build<Module>()
                .With(m => m.CourseId, courseId)
                .With(m => m.Course, course)
                .Create();

            // Assign the module manually
            course.Modules = new List<Module> { module };

            // Save to DB
            CourseContext.Courses.Add(course);
            CourseContext.Modules.Add(module);
            CourseContext.SaveChanges();

            // Act
            var result = await _repo.GetByIdWithModulesAsync(courseId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result!.Modules);
        }
        // ---------------- GetFirstUnpublishedCourse ----------------
        [Fact]
        public async Task GetFirstUnpublishedCourse_ReturnsLowestIdUnpublishedCourse()
        {
            Guid instructor = Guid.NewGuid();

            var c1 = CreateCourse(id: 5, instructor: instructor, isDeleted: true);
            var c2 = CreateCourse(id: 10, instructor: instructor, isDeleted: true);

            CourseContext.Courses.AddRange(c1, c2);
            await CourseContext.SaveChangesAsync();

            var result = await _repo.GetFirstUnpublishedCourse(instructor);

            Assert.NotNull(result);
            Assert.Equal(5, result!.Id);
        }

        // ---------------- GetLatestUnfinishedCourseAsync ----------------
        [Fact]
        public async Task GetLatestUnfinishedCourseAsync_ReturnsHighestIdUnpublished()
        {
            Guid instructor = Guid.NewGuid();

            var c1 = CreateCourse(id: 100, instructor: instructor, isDeleted: true);
            var c2 = CreateCourse(id: 200, instructor: instructor, isDeleted: true);

            CourseContext.Courses.AddRange(c1, c2);
            await CourseContext.SaveChangesAsync();

            var result = await _repo.GetLatestUnfinishedCourseAsync(instructor);

            Assert.NotNull(result);
            Assert.Equal(200, result!.Id);
        }

        // ---------------- GetByIdAllowDeletedAsync ----------------
        [Fact]
        public async Task GetByIdAllowDeletedAsync_ShouldReturnDeletedCourse()
        {
            var c = CreateCourse(id: 50, isDeleted: true);
            CourseContext.Courses.Add(c);
            await CourseContext.SaveChangesAsync();

            var result = await _repo.GetByIdAllowDeletedAsync(50);

            Assert.NotNull(result);
            Assert.Equal(50, result!.Id);
        }

        [Fact]
        public async Task GetByIdAllowDeletedAsync_ShouldReturnNullWhenNotExists()
        {
            var result = await _repo.GetByIdAllowDeletedAsync(999);
            Assert.Null(result);
        }

        // ---------------- SaveChangesAsync ----------------
        [Fact]
        public async Task SaveChangesAsync_CommitsToDatabase()
        {
            var c = CreateCourse(id: 80, isDeleted: false);

            CourseContext.Courses.Add(c);
            await _repo.SaveChangesAsync();

            Assert.True(CourseContext.Courses.Any(x => x.Id == 80));
        }
    }
}
