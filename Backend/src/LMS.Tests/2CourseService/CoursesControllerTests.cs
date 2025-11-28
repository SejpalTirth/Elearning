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

        public CoursesControllerTests()
        {
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
            _courseMock.Setup(s => s.GetAllAsync())
                .ReturnsAsync(new List<CourseResponseDto>
                {
                    new CourseResponseDto { Id = 1, Title = "C# Basics" }
                });

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
            var dto = new CourseResponseDto { Id = 10, Title = "React" };
            _courseMock.Setup(s => s.GetByIdAsync(10)).ReturnsAsync(dto);

            var result = await _controller.GetById(10) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(dto, result!.Value);
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenMissing()
        {
            _courseMock.Setup(s => s.GetByIdAsync(5)).ReturnsAsync((CourseResponseDto?)null);

            var result = await _controller.GetById(5);

            Assert.IsType<NotFoundResult>(result);
        }

        // -----------------------------------------------------
        // CREATE
        // -----------------------------------------------------
        [Fact]
        public async Task Create_ShouldReturnCreated()
        {
            var req = new CourseDto
            {
                InstructorUserId = Guid.NewGuid().ToString(),
                Title = "NodeJS"
            };

            var courseEntity = new Course { Id = 99, Title = "NodeJS" };
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
            var dto = new UpdateCourseDto { Title = "Updated Course" };
            var updated = new Course { Id = 1, Title = "Updated Course" };

            _courseMock.Setup(s => s.UpdateAsync(1, dto)).ReturnsAsync(updated);

            var result = await _controller.Update(1, dto) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(updated, result!.Value);
        }

        [Fact]
        public async Task Update_ShouldReturnNotFound_WhenMissing()
        {
            _courseMock.Setup(s => s.UpdateAsync(5, It.IsAny<UpdateCourseDto>()))
                .ReturnsAsync((Course?)null);

            var result = await _controller.Update(5, new UpdateCourseDto());

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
            _moduleMock.Setup(s => s.GetModulesByCourseAsync(3))
                .ReturnsAsync(new List<ModuleSummaryDto> { new ModuleSummaryDto { Id = 1, Title = "Module 1" } });

            var result = await _controller.GetModulesForCourse(3) as OkObjectResult;

            Assert.NotNull(result);
            var list = Assert.IsType<List<ModuleSummaryDto>>(result!.Value);
            Assert.Single(list);
        }
    }
}
