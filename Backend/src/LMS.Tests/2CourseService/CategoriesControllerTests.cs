using AutoFixture;
using CourseService.DAL.Models;
using CourseService.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace LMS.Tests.CourseService
{
    public class CategoriesControllerTests : BaseTest
    {
        private readonly CategoriesController _controller;
        private readonly Fixture _fixture;

        public CategoriesControllerTests()
        {
            // -----------------------------
            // AutoFixture Setup
            // -----------------------------
            _fixture = new Fixture();

            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            // -----------------------------
            // Seed Categories
            // -----------------------------
            var categories = _fixture.Build<Category>()
                .Without(c => c.Courses)
                .CreateMany(2)
                .ToList();

            categories[0].Name = "Backend";
            categories[1].Name = "Frontend";

            CourseContext.Categories.AddRange(categories);
            CourseContext.SaveChanges();

            // -----------------------------
            // Controller Under Test
            // -----------------------------
            _controller = new CategoriesController(CourseContext);
        }

        protected override void ConfigureGatewayDependencies(IServiceCollection services)
        {
            // No gateway dependencies needed for CourseService tests
        }

        // -----------------------------------------------------
        // BASIC: GET ALL COUNT
        // -----------------------------------------------------
        [Fact]
        public async Task GetAll_ShouldReturnAllCategories()
        {
            var result = await _controller.GetAll() as OkObjectResult;

            Assert.NotNull(result);

            var list = Assert.IsAssignableFrom<IEnumerable<object>>(result!.Value);
            Assert.Equal(2, list.Count());
        }

        // -----------------------------------------------------
        // SHAPE: each object has Id & Name
        // -----------------------------------------------------
        [Fact]
        public async Task GetAll_ShouldReturnObjectsWithIdAndName()
        {
            var result = await _controller.GetAll() as OkObjectResult;

            Assert.NotNull(result);

            var list = Assert.IsAssignableFrom<IEnumerable<object>>(result!.Value);
            Assert.NotEmpty(list);

            var first = list.First();
            var type = first.GetType();

            var idProp = type.GetProperty("Id");
            var nameProp = type.GetProperty("Name");

            Assert.NotNull(idProp);
            Assert.NotNull(nameProp);

            var idValue = idProp!.GetValue(first);
            var nameValue = nameProp!.GetValue(first) as string;

            Assert.NotNull(idValue);
            Assert.True((int)idValue! > 0);

            Assert.False(string.IsNullOrWhiteSpace(nameValue));
        }

        // -----------------------------------------------------
        // CONTENT: contains Backend & Frontend names
        // -----------------------------------------------------
        [Fact]
        public async Task GetAll_ShouldContainSeededCategoryNames()
        {
            var result = await _controller.GetAll() as OkObjectResult;

            Assert.NotNull(result);

            var list = Assert.IsAssignableFrom<IEnumerable<object>>(result!.Value);

            var names = list
                .Select(o => (string)o.GetType().GetProperty("Name")!.GetValue(o)!)
                .ToList();

            Assert.Contains("Backend", names);
            Assert.Contains("Frontend", names);
        }
    }
}
