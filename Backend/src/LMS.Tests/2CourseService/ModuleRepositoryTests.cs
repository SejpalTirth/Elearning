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

        private int _courseId;

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

        // ====================================================================
        // SEED REQUIRED ENTITIES
        // ====================================================================
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

        // ====================================================================
        // CLEAN MODULE FACTORY
        // ====================================================================
        private Module CreateModule(int? id = null, int? courseId = null)
        {
            var m = _fixture.Build<Module>()
                .With(m => m.CourseId, courseId ?? _courseId)
                .Without(m => m.Course)
                .Create();

            if (id != null)
                m.Id = id.Value;

            return m;
        }

        protected override void ConfigureGatewayDependencies(IServiceCollection services)
        {
            // no gateway dependencies
        }

        // ====================================================================
        // GET BY COURSE ID
        // ====================================================================
        [Fact]
        public async Task GetByCourseIdAsync_ShouldReturnModulesForCourse()
        {
            var m1 = CreateModule(courseId: _courseId);
            var m2 = CreateModule(courseId: _courseId + 1);

            CourseContext.Modules.AddRange(m1, m2);
            CourseContext.SaveChanges();

            var list = (await _repo.GetByCourseIdAsync(_courseId)).ToList();

            Assert.Single(list);
            Assert.Equal(_courseId, list[0].CourseId);
        }

        [Fact]
        public async Task GetByCourseIdAsync_ShouldReturnEmpty_WhenNoModules()
        {
            var result = await _repo.GetByCourseIdAsync(99999);

            Assert.Empty(result);
        }

        // ====================================================================
        // GET BY ID
        // ====================================================================
        [Fact]
        public async Task GetByIdAsync_ShouldReturnModule_WhenExists()
        {
            var module = CreateModule(id: 10);

            CourseContext.Modules.Add(module);
            CourseContext.SaveChanges();

            var result = await _repo.GetByIdAsync(10);

            Assert.NotNull(result);
            Assert.Equal(10, result!.Id);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
        {
            var result = await _repo.GetByIdAsync(99999);
            Assert.Null(result);
        }

        // ====================================================================
        // ADD MODULE
        // ====================================================================
        [Fact]
        public async Task AddAsync_ShouldAddModule()
        {
            var module = CreateModule();

            await _repo.AddAsync(module);
            await _repo.SaveChangesAsync();

            Assert.Single(CourseContext.Modules);
        }

        // ====================================================================
        // SAVE CHANGES (No changes)
        // ====================================================================
        [Fact]
        public async Task SaveChangesAsync_ShouldReturn_WhenNoChanges()
        {
            // This should not throw and should return successfully.
            await _repo.SaveChangesAsync();
        }
    }
}
