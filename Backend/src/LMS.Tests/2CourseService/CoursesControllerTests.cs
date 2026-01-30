using AutoFixture;
using CourseService.BLL.DTOs;
using CourseService.BLL.Interface;
using CourseService.BLL.UserContext;
using CourseService.DAL.Models;
using CourseService.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace LMS.Tests.CourseService
{
    public class CoursesControllerTests
    {
        private readonly Mock<ICourseService> _courseMock;
        private readonly Mock<IModuleService> _moduleMock;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<IUserContextAccessor> _userContextMock;
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
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _userContextMock = new Mock<IUserContextAccessor>();

            _userContextMock.Setup(x => x.Current)
                .Returns(new UserContextDto
                {
                    UserId = Guid.NewGuid(),
                    Email = "test@mail.com"
                });

            _controller = new CoursesController(
                _courseMock.Object,
                _moduleMock.Object,
                _httpClientFactoryMock.Object,
                _userContextMock.Object
            );

            // Setup HttpContext for Authorization header if needed
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        // ---------------- GET ALL COURSES ----------------
        [Fact]
        public async Task GetAll_ShouldReturnList()
        {
            var list = _fixture.CreateMany<CourseResponseDto>(2).ToList();
            _courseMock.Setup(s => s.GetAllAsync()).ReturnsAsync(list);

            var result = await _controller.GetAll() as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(list, result!.Value);
        }

        // ---------------- GET BY ID ----------------
        [Fact]
        public async Task GetById_ShouldReturnCourse_WhenExists()
        {
            var course = _fixture.Build<CourseResponseDto>()
            .With(c => c.Id, 1)
            .Create();

            _courseMock.Setup(s => s.GetByIdAsync(course.Id))
                .ReturnsAsync(course);


            var dto = new CourseIdRequestDto { CourseId = course.Id };
            var result = await _controller.GetById(dto) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(course, result!.Value);
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenMissing()
        {
            _courseMock
                .Setup(s => s.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((CourseResponseDto?)null);

            var dto = new CourseIdRequestDto { CourseId = 999 };
            var result = await _controller.GetById(dto);

            Assert.IsType<NotFoundResult>(result);
        }

        // ---------------- GET COURSES BY INSTRUCTOR ----------------
        [Fact]
        public async Task GetByInstructor_ShouldReturnList()
        {
            var userId = _userContextMock.Object.Current!.UserId;
            var list = _fixture.CreateMany<Course>(3).ToList();

            _courseMock.Setup(s => s.GetCoursesByInstructorAsync(userId))
                .ReturnsAsync(list);

            var result = await _controller.GetByInstructor() as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(list, result!.Value);
        }

        // ---------------- CREATE COURSE ----------------
        [Fact]
        public async Task Create_ShouldReturnOk()
        {
            var req = _fixture.Build<CourseDto>()
                .With(r => r.InstructorUserId, _userContextMock.Object.Current!.UserId.ToString())
                .Create();

            var created = _fixture.Build<Course>()
                .With(c => c.Id, 10)
                .Create();

            _courseMock.Setup(s => s.CreateAsync(req)).ReturnsAsync(created);

            var result = await _controller.Create(req) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(created, result!.Value);
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

        // ---------------- UPDATE COURSE ----------------
        [Fact]
        public async Task Update_ShouldReturnOk_WhenUpdated()
        {
            var dto = _fixture.Create<UpdateCourseRequestDto>();
            var updated = _fixture.Build<Course>().With(x => x.Id, dto.CourseId).Create();

            _courseMock.Setup(s => s.UpdateAsync(dto.CourseId, dto.Course)).ReturnsAsync(updated);

            var result = await _controller.Update(dto) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(updated, result!.Value);
        }

        [Fact]
        public async Task Update_ShouldReturnNotFound_WhenMissing()
        {
            var dto = _fixture.Create<UpdateCourseRequestDto>();
            _courseMock.Setup(s => s.UpdateAsync(dto.CourseId, dto.Course)).ReturnsAsync((Course?)null);

            var result = await _controller.Update(dto);

            Assert.IsType<NotFoundResult>(result);
        }

        // ---------------- DELETE COURSE ----------------
        [Fact]
        public async Task Delete_ShouldReturnOk_WhenDeleted()
        {
            var dto = new CourseIdRequestDto { CourseId = 1 };
            _courseMock.Setup(s => s.DeleteAsync(dto.CourseId)).ReturnsAsync(true);

            var result = await _controller.Delete(dto);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task Delete_ShouldReturnNotFound_WhenMissing()
        {
            var dto = new CourseIdRequestDto { CourseId = 2 };
            _courseMock.Setup(s => s.DeleteAsync(dto.CourseId)).ReturnsAsync(false);

            var result = await _controller.Delete(dto);

            Assert.IsType<NotFoundResult>(result);
        }

        // ---------------- GET ENROLLED COURSES ----------------
        [Fact]
        public async Task GetEnrolledCourses_ShouldReturnList()
        {
            var userId = _userContextMock.Object.Current!.UserId;
            var list = _fixture.CreateMany<Course>(2).ToList();

            _courseMock.Setup(s => s.GetUserEnrolledCoursesAsync(userId.ToString()))
                .ReturnsAsync(list);

            var result = await _controller.GetEnrolledCourses() as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(list, result!.Value);
        }

        // ---------------- MODULES ----------------
        [Fact]
        public async Task GetModulesForCourse_ShouldReturnModules()
        {
            var modules = _fixture.CreateMany<ModuleSummaryDto>(3).ToList();
            var dto = new CourseIdRequestDto { CourseId = 5 };

            _moduleMock.Setup(s => s.GetModulesByCourseAsync(dto.CourseId))
                .ReturnsAsync(modules);

            var result = await _controller.GetModulesForCourse(dto) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(modules, result!.Value);
        }

        // ---------------- PUBLISH COURSE ----------------
        [Fact]
        public async Task PublishCourse_ShouldReturnOk_WhenSuccess()
        {
            var dto = new CourseIdRequestDto { CourseId = 10 };
            _courseMock.Setup(s => s.PublishCourseIfReadyAsync(dto.CourseId, It.IsAny<string>()))
                .ReturnsAsync(true);

            _controller.ControllerContext.HttpContext!.Request.Headers["Authorization"] = "Bearer token";

            var result = await _controller.PublishCourse(dto) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal("Course published successfully.", ((dynamic)result!.Value).message);
        }

        [Fact]
        public async Task PublishCourse_ShouldReturnBadRequest_WhenNotReady()
        {
            var dto = new CourseIdRequestDto { CourseId = 10 };
            _courseMock.Setup(s => s.PublishCourseIfReadyAsync(dto.CourseId, It.IsAny<string>()))
                .ReturnsAsync(false);

            _controller.ControllerContext.HttpContext!.Request.Headers["Authorization"] = "Bearer token";

            var result = await _controller.PublishCourse(dto) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Contains("modules must have a quiz", ((dynamic)result!.Value).message.ToString());
        }

        // ---------------- CONTINUE COURSE ----------------
        [Fact]
        public async Task ContinueCourse_ShouldReturnOk()
        {
            var dto = new ContinueCourseRequestDto { CourseId = 5 };
            _courseMock.Setup(s => s.ContinueUnfinishedCourseAsync(dto.CourseId)).ReturnsAsync(true);

            var result = await _controller.ContinueCourse(dto) as OkObjectResult;

            Assert.NotNull(result);
            Assert.True((bool)result!.Value);
        }
    }
}
