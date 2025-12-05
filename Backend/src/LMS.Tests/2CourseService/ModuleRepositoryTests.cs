using AutoFixture;
using CourseService.DAL.Models;
using CourseService.DAL.Repo;
using Microsoft.Extensions.DependencyInjection;

namespace LMS.Tests.CourseService.Repository
{
    public class ModuleRepositoryTests : BaseTest
    {
        private readonly ModuleRepository _repo;
        private readonly Fixture _fixture;

        public ModuleRepositoryTests()
        {
            _fixture = new Fixture();

            // Prevent circular graphs
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _repo = new ModuleRepository(CourseContext);

            SeedCategoryAndCourse();
        }

        // --------------------------------------------------------------------
        // SEED REQUIRED CATEGORY + COURSE FOR FK RELATION
        // --------------------------------------------------------------------
        private int _courseId;

        private void SeedCategoryAndCourse()
        {
            var category = _fixture.Build<Category>()
                .Without(c => c.Courses)
                .Create();

            CourseContext.Categories.Add(category);
            CourseContext.SaveChanges();

            var course = _fixture.Build<Course>()
                .With(c => c.CategoryId, category.Id)
                .Without(c => c.Category)
                .Without(c => c.Modules)
                .Without(c => c.Enrollments)
                .Create();

            CourseContext.Courses.Add(course);
            CourseContext.SaveChanges();

            _courseId = course.Id;
        }

        // --------------------------------------------------------------------
        // CLEAN MODULE FACTORY
        // --------------------------------------------------------------------
        private Module CreateModule(int? id = null, int? courseId = null)
        {
            var module = _fixture.Build<Module>()
                .With(m => m.CourseId, courseId ?? _courseId)
                .Without(m => m.Course)  // avoid EF cascade insertion
                .Create();

            if (id != null)
                module.Id = id.Value;

            return module;
        }

        protected override void ConfigureGatewayDependencies(IServiceCollection services)
        {
            // No external services needed
        }

        // ====================================================================
        // GET BY COURSE ID
        // ====================================================================
        [Fact]
        public async Task GetByCourseIdAsync_ReturnsModules()
        {
            var m1 = CreateModule(courseId: _courseId);
            var m2 = CreateModule(courseId: _courseId + 1);

            CourseContext.Modules.AddRange(m1, m2);
            CourseContext.SaveChanges();

            var list = (await _repo.GetByCourseIdAsync(_courseId)).ToList();

            Assert.Single(list);
            Assert.Equal(_courseId, list[0].CourseId);
        }

        // ====================================================================
        // GET BY ID
        // ====================================================================
        [Fact]
        public async Task GetByIdAsync_ReturnsCorrectModule()
        {
            var module = CreateModule(id: 5);

            CourseContext.Modules.Add(module);
            CourseContext.SaveChanges();

            var result = await _repo.GetByIdAsync(5);

            Assert.NotNull(result);
            Assert.Equal(5, result!.Id);
        }

        // ====================================================================
        // ADD
        // ====================================================================
        [Fact]
        public async Task AddAsync_ShouldAddModule()
        {
            var module = CreateModule();

            await _repo.AddAsync(module);
            await _repo.SaveChangesAsync();

            Assert.Single(CourseContext.Modules);
        }
    }
}
