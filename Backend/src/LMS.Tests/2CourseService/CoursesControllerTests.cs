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
            var list = _fixture.CreateMany<CourseResponseDto>(2).ToList();
            _courseMock.Setup(s => s.GetAllAsync()).ReturnsAsync(list);

            var result = await _controller.GetAll() as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(list, result!.Value);
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

            var result = await _controller.GetById(999);

            Assert.IsType<NotFoundResult>(result);
        }

        // -----------------------------------------------------
        // GET BY INSTRUCTOR
        // -----------------------------------------------------
        [Fact]
        public async Task GetByInstructor_ShouldReturnList()
        {
            var instructor = Guid.NewGuid();
            var list = _fixture.CreateMany<Course>(3).ToList();

            _courseMock.Setup(s => s.GetCoursesByInstructorAsync(instructor))
                       .Returns(Task.FromResult((IEnumerable<Course>)list));

            var result = await _controller.GetByInstructor(instructor) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(list, result!.Value);
        }

        // -----------------------------------------------------
        // CREATE
        // -----------------------------------------------------
        [Fact]
        public async Task Create_ShouldReturnCreated()
        {
            var req = _fixture.Build<CourseDto>()
                .With(r => r.InstructorUserId, Guid.NewGuid().ToString())
                .Create();

            var created = _fixture.Build<Course>()
                .With(c => c.Id, 10)
                .Create();

            _courseMock.Setup(s => s.CreateAsync(req)).ReturnsAsync(created);

            var result = await _controller.Create(req) as CreatedAtActionResult;

            Assert.NotNull(result);
            Assert.Equal(created, result!.Value);
            Assert.Equal("GetById", result.ActionName);
        }

        [Fact]
        public async Task Create_ShouldReturnBadRequest_WhenInstructorMissing()
        {
            var req = _fixture.Build<CourseDto>()
                .With(r => r.InstructorUserId, string.Empty)
                .Create();

            var result = await _controller.Create(req);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("InstructorUserId", bad.Value!.ToString());
        }

        // -----------------------------------------------------
        // UPDATE
        // -----------------------------------------------------
        [Fact]
        public async Task Update_ShouldReturnOk_WhenUpdated()
        {
            var dto = _fixture.Create<UpdateCourseDto>();
            var updated = _fixture.Build<Course>().With(x => x.Id, 1).Create();

            _courseMock.Setup(s => s.UpdateAsync(1, dto))
                       .ReturnsAsync(updated);

            var result = await _controller.Update(1, dto) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(updated, result!.Value);
        }

        [Fact]
        public async Task Update_ShouldReturnNotFound_WhenMissing()
        {
            _courseMock.Setup(s => s.UpdateAsync(1, It.IsAny<UpdateCourseDto>()))
                       .ReturnsAsync((Course?)null);

            var result = await _controller.Update(1, new UpdateCourseDto());

            Assert.IsType<NotFoundResult>(result);
        }

        // -----------------------------------------------------
        // DELETE
        // -----------------------------------------------------
        [Fact]
        public async Task Delete_ShouldReturnOk_WhenDeleted()
        {
            _courseMock.Setup(s => s.DeleteAsync(1)).ReturnsAsync(true);

            var result = await _controller.Delete(1);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task Delete_ShouldReturnNotFound_WhenMissing()
        {
            _courseMock.Setup(s => s.DeleteAsync(2)).ReturnsAsync(false);

            var result = await _controller.Delete(2);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("The course cannot be found", notFound.Value);
        }

        // -----------------------------------------------------
        // GET ENROLLED COURSES
        // -----------------------------------------------------
        [Fact]
        public async Task GetEnrolledCourses_ShouldReturnList()
        {
            var list = _fixture.CreateMany<Course>(2).ToList();

            _courseMock.Setup(s => s.GetUserEnrolledCoursesAsync("123"))
                       .Returns(Task.FromResult((IEnumerable<Course>)list));

            var result = await _controller.GetEnrolledCourses("123") as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(list, result!.Value);
        }

        // -----------------------------------------------------
        // MODULE ROUTE
        // -----------------------------------------------------
        [Fact]
        public async Task GetModulesForCourse_ShouldReturnModules()
        {
            var modules = _fixture.CreateMany<ModuleSummaryDto>(3).ToList();

            _moduleMock.Setup(s => s.GetModulesByCourseAsync(5))
                       .ReturnsAsync(modules);

            var result = await _controller.GetModulesForCourse(5) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(modules, result!.Value);
        }

        // -----------------------------------------------------
        // PUBLISH COURSE
        // -----------------------------------------------------
        [Fact]
        public async Task PublishCourse_ShouldReturnOk_WhenSuccess()
        {
            _courseMock.Setup(s => s.PublishCourseIfReadyAsync(10))
                       .ReturnsAsync(true);

            var result = await _controller.PublishCourse(10) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal("Course published successfully.", result!.Value);
        }

        [Fact]
        public async Task PublishCourse_ShouldReturnBadRequest_WhenNotReady()
        {
            _courseMock.Setup(s => s.PublishCourseIfReadyAsync(10))
                       .ReturnsAsync(false);

            var result = await _controller.PublishCourse(10) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Contains("modules must have a quiz", result!.Value!.ToString());
        }

        // -----------------------------------------------------
        // UNFINISHED COURSE
        // -----------------------------------------------------
        [Fact]
        public async Task GetUnfinishedCourse_ShouldReturnCourse()
        {
            var instructorId = Guid.NewGuid();
            var dto = new { id = 1, title = "Test Course", description = "Test", categoryId = 1 };

            _courseMock.Setup(s => s.GetUnfinishedCourseAsync(instructorId))
                       .Returns(Task.FromResult((object?)dto));

            var result = await _controller.GetUnfinishedCourse(instructorId) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(dto, result!.Value);
        }

        // -----------------------------------------------------
        // CONTINUE COURSE
        // -----------------------------------------------------
        [Fact]
        public async Task ContinueCourse_ShouldReturnOk()
        {
            _courseMock.Setup(s => s.ContinueUnfinishedCourseAsync(5))
                       .Returns(Task.FromResult(true));

            var result = await _controller.ContinueCourse(5) as OkObjectResult;

            Assert.NotNull(result);
            Assert.True((bool)result!.Value!);
        }
    }
}
