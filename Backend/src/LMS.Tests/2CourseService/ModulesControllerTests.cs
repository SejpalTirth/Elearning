using CourseService.BLL.DTOs;
using CourseService.BLL.Interface;
using CourseService.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace LMS.Tests.CourseService
{
    public class ModulesControllerTests
    {
        private readonly Mock<IModuleService> _moduleMock;
        private readonly ModulesController _controller;

        public ModulesControllerTests()
        {
            _moduleMock = new Mock<IModuleService>();
            _controller = new ModulesController(_moduleMock.Object);
        }

        // -----------------------------------------------------
        // GET BY COURSE
        // -----------------------------------------------------
        [Fact]
        public async Task GetByCourse_ShouldReturnModules()
        {
            _moduleMock.Setup(m => m.GetModulesByCourseAsync(10))
                .ReturnsAsync(new List<ModuleSummaryDto> { new ModuleSummaryDto { Id = 1, Title = "Module 1" } });

            var result = await _controller.GetByCourse(10) as OkObjectResult;

            Assert.NotNull(result);
            var modules = Assert.IsType<List<ModuleSummaryDto>>(result!.Value);
            Assert.Single(modules);
        }

        // -----------------------------------------------------
        // GET CONTENT
        // -----------------------------------------------------
        [Fact]
        public async Task GetContent_ShouldReturnModule_WhenExists()
        {
            var dto = new ModuleContentResponseDto { Id = 5, Title = "Module 5", Content = "Hello" };

            _moduleMock.Setup(m => m.GetModuleContentAsync(5)).ReturnsAsync(dto);

            var result = await _controller.GetContent(5) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(dto, result!.Value);
        }

        [Fact]
        public async Task GetContent_ShouldReturnNotFound_WhenMissing()
        {
            _moduleMock.Setup(m => m.GetModuleContentAsync(3))
                .ReturnsAsync((ModuleContentResponseDto?)null);

            var result = await _controller.GetContent(3);

            Assert.IsType<NotFoundResult>(result);
        }

        // -----------------------------------------------------
        // GET COURSE ID & MODULE ID
        // -----------------------------------------------------
        [Fact]
        public async Task GetCourseId_ShouldReturnData()
        {
            var response = new ModuleAndCourseIdDTO
            {
                ModuleId = 1,
                CourseId = 10
            };

            _moduleMock.Setup(m => m.GetModuleAndCourseIdAsync(1))
                .ReturnsAsync(response);

            var result = await _controller.GetCourseId(1) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(response, result!.Value);
        }

        [Fact]
        public async Task GetCourseId_ShouldReturnNotFound_WhenMissing()
        {
            _moduleMock.Setup(m => m.GetModuleAndCourseIdAsync(2))
                .ReturnsAsync((ModuleAndCourseIdDTO?)null);

            var result = await _controller.GetCourseId(2);

            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
