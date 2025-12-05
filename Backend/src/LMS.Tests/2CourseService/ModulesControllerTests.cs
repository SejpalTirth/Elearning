using AutoFixture;
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
        private readonly Fixture _fixture;

        public ModulesControllerTests()
        {
            _moduleMock = new Mock<IModuleService>();
            _controller = new ModulesController(_moduleMock.Object);

            _fixture = new Fixture();

            // Prevent accidental recursion for DTOs (safe default)
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        // -----------------------------------------------------
        // GET BY COURSE
        // -----------------------------------------------------
        [Fact]
        public async Task GetByCourse_ShouldReturnModules()
        {
            var moduleList = _fixture.CreateMany<ModuleSummaryDto>(1).ToList();

            _moduleMock.Setup(m => m.GetModulesByCourseAsync(10))
                .ReturnsAsync(moduleList);

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
            var dto = _fixture.Build<ModuleContentResponseDto>()
                .With(m => m.Id, 5)
                .Create();

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
            var response = _fixture.Build<ModuleAndCourseIdDTO>()
                .With(d => d.ModuleId, 1)
                .With(d => d.CourseId, 10)
                .Create();

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
