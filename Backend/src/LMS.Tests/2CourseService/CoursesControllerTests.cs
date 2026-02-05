using AutoFixture;
using CourseService.BLL.Interface;
using CourseService.BLL.UserContext;
using CourseService.DAL.Models;
using CourseService.Web.Controllers;
using DTOs._2CourseService;
using DTOs._5ProgresService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace LMS.Tests.CourseService
{
    public class CoursesControllerTests
    {
        private readonly Mock<ICourseService> _courseMock;
        private readonly Mock<IModuleService> _moduleMock;
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
                _userContextMock.Object
            );

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        // ---------------- GET ALL ----------------
        [Fact]
        public async Task GetAll_ShouldReturnOk()
        {
            var courses = _fixture.CreateMany<CourseResponseDto>(2).ToList();
            _courseMock
                .Setup(s => s.GetAllAsync())
                .ReturnsAsync(courses);


            var result = await _controller.GetAll();

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(courses, ok.Value);
        }

        // ---------------- GET BY ID ----------------
        [Fact]
        public async Task GetById_ShouldReturnCourse()
        {
            var course = _fixture.Create<CourseResponseDto>();

            _courseMock
                .Setup(s => s.GetByIdAsync(1))
                .ReturnsAsync(course);


            var result = await _controller.GetById(new CourseIdRequestDto { CourseId = 1 });

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(course, ok.Value);
        }

        // ---------------- CREATE ----------------
        [Fact]
        public async Task Create_ShouldReturnCreated()
        {
            var dto = _fixture.Create<CourseDto>();
            var created = _fixture.Build<Course>().With(c => c.Id, 10).Create();

            _courseMock.Setup(s => s.CreateAsync(dto)).ReturnsAsync(created);

            var result = await _controller.Create(dto);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(created, createdResult.Value);
        }

        // ---------------- UPDATE ----------------
        [Fact]
        public async Task Update_ShouldReturnOk()
        {
            var req = _fixture.Create<UpdateCourseRequestDto>();
            var updated = _fixture.Build<Course>().With(c => c.Id, req.CourseId).Create();

            _courseMock.Setup(s => s.UpdateAsync(req.CourseId, req.Course))
                .ReturnsAsync(updated);

            var result = await _controller.Update(req);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(updated, ok.Value);
        }

        [Fact]
        public async Task Update_ShouldReturnNotFound()
        {
            var req = _fixture.Create<UpdateCourseRequestDto>();
            _courseMock.Setup(s => s.UpdateAsync(req.CourseId, req.Course))
                .ReturnsAsync((Course?)null);

            var result = await _controller.Update(req);

            Assert.IsType<NotFoundResult>(result);
        }

        // ---------------- DELETE ----------------
        [Fact]
        public async Task Delete_ShouldReturnOk()
        {
            _courseMock.Setup(s => s.DeleteAsync(1)).ReturnsAsync(true);

            var result = await _controller.Delete(new CourseIdRequestDto { CourseId = 1 });

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task Delete_ShouldReturnNotFound()
        {
            _courseMock.Setup(s => s.DeleteAsync(2)).ReturnsAsync(false);

            var result = await _controller.Delete(new CourseIdRequestDto { CourseId = 2 });

            Assert.IsType<NotFoundResult>(result);
        }

        // ---------------- ENROLLED ----------------
        [Fact]
        public async Task GetEnrolledCourses_ShouldReturnMappedDtos()
        {
            var courses = _fixture.CreateMany<Course>(2).ToList();

            _courseMock.Setup(s => s.GetUserEnrolledCoursesAsync(It.IsAny<string>()))
                .ReturnsAsync(courses);

            var result = await _controller.GetEnrolledCourses();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsAssignableFrom<IEnumerable<EnrolledCourseResponseDto>>(ok.Value);
            Assert.Equal(courses.Count, list.Count());
        }

        // ---------------- PUBLISH ----------------
        [Fact]
        public async Task Publish_ShouldReturnOk()
        {
            _courseMock.Setup(s => s.PublishCourseIfReadyAsync(1, It.IsAny<string>()))
                .ReturnsAsync(true);

            _controller.HttpContext!.Request.Headers["Authorization"] = "Bearer token";

            var result = await _controller.PublishCourse(new CourseIdRequestDto { CourseId = 1 });

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("published", ok.Value!.ToString());
        }

        [Fact]
        public async Task Publish_ShouldReturnBadRequest()
        {
            _courseMock.Setup(s => s.PublishCourseIfReadyAsync(1, It.IsAny<string>()))
                .ReturnsAsync(false);

            _controller.HttpContext!.Request.Headers["Authorization"] = "Bearer token";

            var result = await _controller.PublishCourse(new CourseIdRequestDto { CourseId = 1 });

            Assert.IsType<BadRequestObjectResult>(result);
        }

        // ---------------- CONTINUE ----------------
        [Fact]
        public async Task ContinueCourse_ShouldReturnOk()
        {
            var course = _fixture.Create<CourseResponseDto>();

            _courseMock
                .Setup(s => s.GetByIdAsync(5))
                .ReturnsAsync(course);

            var result = await _controller.ContinueCourse(
                new ContinueCourseRequestDto { CourseId = 5 });

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(course, ok.Value);
        }

        [Fact]
        public async Task ContinueCourse_ShouldReturnNotFound()
        {
            _courseMock
                .Setup(s => s.GetByIdAsync(It.IsAny<int>()))
                .Returns(Task.FromResult<CourseResponseDto?>(null));

            var result = await _controller.ContinueCourse(
                new ContinueCourseRequestDto { CourseId = 5 });

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Course not found.", notFound.Value);
        }
    }
}