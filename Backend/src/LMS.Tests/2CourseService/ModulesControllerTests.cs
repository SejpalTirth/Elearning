using AutoFixture;
using CourseService.BLL.DTOs;
using CourseService.BLL.Interface;
using CourseService.Web.Controllers;
using DTOs._2CourseService;
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

            // Prevent recursion for DTO generation
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        // =====================================================================
        // GET MODULES BY COURSE
        // =====================================================================
        [Fact]
        public async Task GetByCourse_ShouldReturnOk_WithModules()
        {
            var modules = _fixture.CreateMany<ModuleSummaryDto>(2).ToList();

            _moduleMock.Setup(s => s.GetModulesByCourseAsync(5))
                .ReturnsAsync(modules);

            var result = await _controller.GetByCourse(5) as OkObjectResult;

            Assert.NotNull(result);
            var list = Assert.IsType<List<ModuleSummaryDto>>(result.Value);
            Assert.Equal(2, list.Count);
        }

        [Fact]
        public async Task GetByCourse_ShouldReturnOk_WithEmptyList()
        {
            _moduleMock.Setup(s => s.GetModulesByCourseAsync(5))
                .ReturnsAsync(new List<ModuleSummaryDto>());

            var result = await _controller.GetByCourse(CourseIdRequestDto { 5 }) as OkObjectResult;

            Assert.NotNull(result);
            var list = Assert.IsType<List<ModuleSummaryDto>>(result.Value);
            Assert.Empty(list);
        }

        // =====================================================================
        // GET MODULE CONTENT
        // =====================================================================
        [Fact]
        public async Task GetContent_ShouldReturnOk_WhenModuleFound()
        {
            var dto = _fixture.Build<ModuleContentResponseDto>()
                .With(m => m.Id, 10)
                .Create();

            _moduleMock.Setup(s => s.GetModuleContentAsync(10))
                .ReturnsAsync(dto);

            var result = await _controller.GetContent(10) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(dto, result.Value);
        }

        [Fact]
        public async Task GetContent_ShouldReturnNotFound_WhenModuleMissing()
        {
            _moduleMock.Setup(s => s.GetModuleContentAsync(15))
                .ReturnsAsync((ModuleContentResponseDto?)null);

            var result = await _controller.GetContent(15);

            Assert.IsType<NotFoundResult>(result);
        }

        // =====================================================================
        // GET MODULE + COURSE ID
        // =====================================================================
        [Fact]
        public async Task GetCourseId_ShouldReturnOk_WhenFound()
        {
            var dto = _fixture.Build<ModuleAndCourseIdDTO>()
                .With(d => d.ModuleId, 3)
                .With(d => d.CourseId, 50)
                .Create();

            _moduleMock.Setup(s => s.GetModuleAndCourseIdAsync(3))
                .ReturnsAsync(dto);

            var result = await _controller.GetCourseId(3) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(dto, result.Value);
        }

        [Fact]
        public async Task GetCourseId_ShouldReturnNotFound_WhenMissing()
        {
            _moduleMock.Setup(s => s.GetModuleAndCourseIdAsync(99))
                .ReturnsAsync((ModuleAndCourseIdDTO?)null);

            var result = await _controller.GetCourseId(99);

            var nf = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("not found", nf.Value!.ToString(), StringComparison.OrdinalIgnoreCase);
        }
    }
}
