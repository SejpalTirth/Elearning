using AutoFixture;
using CourseService.BLL.DTOs;
using CourseService.BLL.Interface;
using CourseService.DAL.Models;
using CourseService.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace LMS.Tests.CourseService
{
    public class CoursesControllerTests
    {
        private readonly Mock<ICourseService> _courseMock;
        private readonly Mock<IModuleService> _moduleMock;
        private readonly CoursesController _controller;
        private readonly Fixture _fixture;

        public CoursesControllerTests()
        {
            _fixture = new Fixture();

            // Prevent recursion for EF-like models
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _courseMock = new Mock<ICourseService>();
            _moduleMock = new Mock<IModuleService>();

            _controller = new CoursesController(_courseMock.Object, _moduleMock.Object);
        }

        // -----------------------------------------------------
        // GET ALL
        // -----------------------------------------------------
        [Fact]
        public async Task GetAll_ShouldReturnList()
        {
            var dtoList = _fixture.CreateMany<CourseResponseDto>(1).ToList();

            _courseMock.Setup(s => s.GetAllAsync()).ReturnsAsync(dtoList);

            var result = await _controller.GetAll() as OkObjectResult;

            Assert.NotNull(result);

            var data = Assert.IsType<List<CourseResponseDto>>(result!.Value);
            Assert.Single(data);
        }

        // -----------------------------------------------------
        // GET BY ID
        // -----------------------------------------------------
        [Fact]
        public async Task GetById_ShouldReturnCourse_WhenExists()
        {
            var dto = _fixture.Create<CourseResponseDto>();

            _courseMock.Setup(s => s.GetByIdAsync(dto.Id)).ReturnsAsync(dto);

            var result = await _controller.GetById(dto.Id) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(dto, result!.Value);
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenMissing()
        {
            _courseMock.Setup(s => s.GetByIdAsync(It.IsAny<int>()))
                       .ReturnsAsync((CourseResponseDto?)null);

            var result = await _controller.GetById(1000);

            Assert.IsType<NotFoundResult>(result);
        }

        // -----------------------------------------------------
        // CREATE
        // -----------------------------------------------------
        [Fact]
        public async Task Create_ShouldReturnCreated()
        {
            var req = _fixture.Build<CourseDto>()
                .With(r => r.InstructorUserId, Guid.NewGuid().ToString())
                .With(r => r.Title, "NodeJS")
                .Create();

            var courseEntity = _fixture.Build<Course>()
                .With(c => c.Id, 99)
                .With(c => c.Title, "NodeJS")
                .Create();

            _courseMock.Setup(s => s.CreateAsync(req))
                       .ReturnsAsync(courseEntity);

            var result = await _controller.Create(req) as CreatedAtActionResult;

            Assert.NotNull(result);
            Assert.Equal(99, ((Course)result!.Value!).Id);
        }

        // -----------------------------------------------------
        // UPDATE
        // -----------------------------------------------------
        [Fact]
        public async Task Update_ShouldReturnOk_WhenUpdated()
        {
            var dto = _fixture.Build<UpdateCourseDto>()
                              .With(d => d.Title, "Updated Course")
                              .Create();

            var updated = _fixture.Build<Course>()
                                  .With(c => c.Id, 1)
                                  .With(c => c.Title, "Updated Course")
                                  .Create();

            _courseMock.Setup(s => s.UpdateAsync(1, dto))
                       .ReturnsAsync(updated);

            var result = await _controller.Update(1, dto) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(updated, result!.Value);
        }

        [Fact]
        public async Task Update_ShouldReturnNotFound_WhenMissing()
        {
            _courseMock.Setup(s => s.UpdateAsync(It.IsAny<int>(), It.IsAny<UpdateCourseDto>()))
                       .ReturnsAsync((Course?)null);

            var result = await _controller.Update(999, new UpdateCourseDto());

            Assert.IsType<NotFoundResult>(result);
        }

        // -----------------------------------------------------
        // DELETE
        // -----------------------------------------------------
        [Fact]
        public async Task Delete_ShouldReturnNoContent_WhenDeleted()
        {
            _courseMock.Setup(s => s.DeleteAsync(1)).ReturnsAsync(true);

            var result = await _controller.Delete(1);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_ShouldReturnNotFound_WhenMissing()
        {
            _courseMock.Setup(s => s.DeleteAsync(2)).ReturnsAsync(false);

            var result = await _controller.Delete(2);

            Assert.IsType<NotFoundResult>(result);
        }

        // -----------------------------------------------------
        // MODULES ROUTE
        // -----------------------------------------------------
        [Fact]
        public async Task GetModulesForCourse_ShouldReturnModules()
        {
            var modules = _fixture.CreateMany<ModuleSummaryDto>(1).ToList();

            _moduleMock.Setup(s => s.GetModulesByCourseAsync(3))
                       .ReturnsAsync(modules);

            var result = await _controller.GetModulesForCourse(3) as OkObjectResult;

            Assert.NotNull(result);

            var list = Assert.IsType<List<ModuleSummaryDto>>(result!.Value);
            Assert.Single(list);
        }
    }
}
