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

            // Remove recursion issue for EF navigation
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            // -----------------------------
            // Seed Categories (AutoFixture)
            // -----------------------------
            var categories = _fixture.Build<Category>()
                .Without(c => c.Courses)     // Prevent recursion
                .CreateMany(2)
                .ToList();

            // Override predictable names
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
        // TEST: GET ALL CATEGORIES
        // -----------------------------------------------------
        [Fact]
        public async Task GetAll_ShouldReturnAllCategories()
        {
            // Act
            var result = await _controller.GetAll() as OkObjectResult;

            // Assert
            Assert.NotNull(result);

            var list = result.Value as IEnumerable<object>;
            Assert.NotNull(list);
            Assert.Equal(2, list.Count());
        }

    }
}
