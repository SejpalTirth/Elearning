using AutoFixture;
using CourseService.DAL.Models;
using CourseService.Web.Controllers;
using DTOs._2CourseService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LMS.Tests.CourseService
{
    public class CategoriesControllerTests
    {
        private readonly CourseContext _context;
        private readonly CategoriesController _controller;
        private readonly Fixture _fixture;

        public CategoriesControllerTests()
        {
            var options = new DbContextOptionsBuilder<CourseContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new CourseContext(options);

            _fixture = new Fixture();
            _fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _controller = new CategoriesController(_context);
        }

        // -----------------------------------------------------
        // GET ALL - empty DB
        // -----------------------------------------------------
        [Fact]
        public async Task GetAll_WhenNoCategories_ShouldReturnEmptyList()
        {
            var result = await _controller.GetAll();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsAssignableFrom<IEnumerable<CategoryResponseDto>>(ok.Value);

            Assert.Empty(list);
        }

        // -----------------------------------------------------
        // GET ALL - count
        // -----------------------------------------------------
        [Fact]
        public async Task GetAll_ShouldReturnAllCategories()
        {
            _context.Categories.AddRange(
                new Category { Id = 1, Name = "Backend" },
                new Category { Id = 2, Name = "Frontend" }
            );
            await _context.SaveChangesAsync();

            var result = await _controller.GetAll();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsAssignableFrom<IEnumerable<CategoryResponseDto>>(ok.Value);

            Assert.Equal(2, list.Count());
        }

        // -----------------------------------------------------
        // GET ALL - DTO mapping
        // -----------------------------------------------------
        [Fact]
        public async Task GetAll_ShouldReturnMappedDto()
        {
            var category = new Category
            {
                Id = 10,
                Name = "Backend"
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            var result = await _controller.GetAll();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsAssignableFrom<IEnumerable<CategoryResponseDto>>(ok.Value);

            var dto = list.Single();
            Assert.Equal(category.Id, dto.Id);
            Assert.Equal(category.Name, dto.Name);
        }
    }
}
