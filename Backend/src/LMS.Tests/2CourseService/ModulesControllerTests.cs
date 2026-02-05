using CourseService.BLL.Interface;
using CourseService.Web.Controllers;
using DTOs._2CourseService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace LMS.Tests._2CourseService
{
    public class ModulesControllerTests
    {
        private readonly Mock<IModuleService> _moduleService = new();
        private readonly ModulesController _controller;

        public ModulesControllerTests()
        {
            _controller = new ModulesController(_moduleService.Object);
            _controller.ControllerContext.HttpContext = new DefaultHttpContext(); // optional, for full setup
        }

        // ----------------------------
        // GET MODULES BY COURSE
        // ----------------------------
        [Fact]
        public async Task GetByCourse_ReturnsOk_WithModules()
        {
            // Arrange
            var courseId = 1;
            var dto = new CourseIdRequestDto { CourseId = courseId };

            var modules = new List<ModuleSummaryDto>
            {
                new ModuleSummaryDto { Id = 1, Title = "Module 1", Content = "Content 1" },
                new ModuleSummaryDto { Id = 2, Title = "Module 2", Content = "Content 2" }
            };

            _moduleService.Setup(s => s.GetModulesByCourseAsync(courseId))
                          .ReturnsAsync(modules);

            // Act
            var result = await _controller.GetByCourse(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsAssignableFrom<IEnumerable<ModuleSummaryDto>>(okResult.Value);
            Assert.Equal(2, returned.Count());
        }

        // ----------------------------
        // GET MODULE CONTENT
        // ----------------------------
        [Fact]
        public async Task GetContent_ReturnsOk_WhenModuleExists()
        {
            // Arrange
            var moduleId = 5;
            var dto = new ModuleIdRequestDto { ModuleId = moduleId };

            var module = new ModuleContentResponseDto
            {
                Id = moduleId,
                Title = "Module 5",
                Content = "Some content",
                QuizId = 10
            };

            _moduleService.Setup(s => s.GetModuleContentAsync(moduleId))
                          .ReturnsAsync(module);

            // Act
            var result = await _controller.GetContent(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<ModuleContentResponseDto>(okResult.Value);
            Assert.Equal(moduleId, returned.Id);
            Assert.Equal(10, returned.QuizId);
        }

        [Fact]
        public async Task GetContent_ReturnsNotFound_WhenModuleMissing()
        {
            // Arrange
            var moduleId = 99;
            var dto = new ModuleIdRequestDto { ModuleId = moduleId };

            _moduleService.Setup(s => s.GetModuleContentAsync(moduleId))
                          .ReturnsAsync((ModuleContentResponseDto?)null);

            // Act
            var result = await _controller.GetContent(dto);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        // ----------------------------
        // GET MODULE + COURSE ID
        // ----------------------------
        [Fact]
        public async Task GetCourseId_ReturnsOk_WhenModuleExists()
        {
            // Arrange
            var moduleId = 3;
            var dto = new ModuleIdRequestDto { ModuleId = moduleId };

            var moduleData = new ModuleAndCourseIdDTO
            {
                ModuleId = moduleId,
                CourseId = 42
            };

            _moduleService.Setup(s => s.GetModuleAndCourseIdAsync(moduleId))
                          .ReturnsAsync(moduleData);

            // Act
            var result = await _controller.GetCourseId(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<ModuleAndCourseIdDTO>(okResult.Value);
            Assert.Equal(42, returned.CourseId);
            Assert.Equal(moduleId, returned.ModuleId);
        }

        [Fact]
        public async Task GetCourseId_ReturnsNotFound_WhenModuleMissing()
        {
            // Arrange
            var moduleId = 77;
            var dto = new ModuleIdRequestDto { ModuleId = moduleId };

            _moduleService.Setup(s => s.GetModuleAndCourseIdAsync(moduleId))
                          .ReturnsAsync((ModuleAndCourseIdDTO?)null);

            // Act
            var result = await _controller.GetCourseId(dto);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("not found", notFoundResult.Value!.ToString()!);
        }
    }
}
