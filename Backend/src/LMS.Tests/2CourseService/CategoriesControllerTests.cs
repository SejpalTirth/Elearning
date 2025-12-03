using CourseService.DAL.Models;
using CourseService.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.Tests.CourseService
{
    public class CategoriesControllerTests
    {
        private readonly CourseContext _context;
        private readonly CategoriesController _controller;

        public CategoriesControllerTests()
        {
            var options = new DbContextOptionsBuilder<CourseContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CourseContext(options);

            // Seed Data
            _context.Categories.AddRange(
                new Category { Id = 1, Name = "Backend" },
                new Category { Id = 2, Name = "Frontend" }
            );
            _context.SaveChanges();

            _controller = new CategoriesController(_context);
        }

        [Fact]
        public async Task GetAll_ShouldReturnAllCategories()
        {
            var result = await _controller.GetAll() as OkObjectResult;

            Assert.NotNull(result);

            var list = result!.Value as System.Collections.IList;
            Assert.NotNull(list);
            Assert.Equal(2, list.Count);
        }
    }
}
