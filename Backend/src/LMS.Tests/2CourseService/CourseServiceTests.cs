using AutoFixture;
using AutoMapper;
using CourseService.BLL.DTOs;
using CourseService.BLL.Service;
using CourseService.DAL.Models;
using CourseService.DAL.Repo;
using CourseService.BLL.UserContext;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Net;
using System.Text.Json;

namespace LMS.Tests.CourseService
{
    public class CourseServiceTests
    {
        private readonly Mock<ICourseRepository> _courseRepo;
        private readonly Mock<IEnrollmentRepository> _enrollRepo;
        private readonly Mock<IModuleRepository> _moduleRepo;
        private readonly Mock<IHttpClientFactory> _httpFactory;
        private readonly Mock<IMapper> _mapperMock;

        private readonly FakeHandler _userHandler;
        private readonly FakeHandler _assessmentHandler;

        private readonly HttpClient _fakeUserClient;
        private readonly HttpClient _fakeAssessmentClient;

        private readonly Fixture _fixture;

        private readonly Mock<IUserContextAccessor> _userContextMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;


        public CourseServiceTests()
        {
            _fixture = new Fixture();

            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _courseRepo = new Mock<ICourseRepository>();
            _enrollRepo = new Mock<IEnrollmentRepository>();
            _moduleRepo = new Mock<IModuleRepository>();
            _httpFactory = new Mock<IHttpClientFactory>();
            _mapperMock = new Mock<IMapper>();
            _userContextMock = new Mock<IUserContextAccessor>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            // Fake API handlers
            _userHandler = new FakeHandler();
            _assessmentHandler = new FakeHandler();
            
            _httpContextAccessorMock.Setup(x => x.HttpContext)
                .Returns(new DefaultHttpContext());

            _fakeUserClient = new HttpClient(_userHandler) { BaseAddress = new Uri("https://fake-user/") };
            _fakeAssessmentClient = new HttpClient(_assessmentHandler) { BaseAddress = new Uri("https://fake-assessment/") };

            _httpFactory.Setup(x => x.CreateClient("UserService")).Returns(_fakeUserClient);
            _httpFactory.Setup(x => x.CreateClient("AssessmentService")).Returns(_fakeAssessmentClient);
        }


        // -------------------------------------------------------
        // Fake Handler Implementation
        // -------------------------------------------------------
        private class FakeHandler : HttpMessageHandler
        {
            public object? Response { get; set; } = new { };

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage req, CancellationToken ct)
            {
                string json = JsonSerializer.Serialize(Response);
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json)
                });
            }
        }


        // -------------------------------------------------------
        // GET ALL
        // -------------------------------------------------------
        [Fact]
        public async Task GetAllAsync_ShouldReturnMappedCourses()
        {
            var instructorInfo = new UserAuthDto { Name = "Instructor A" };
            _userHandler.Response = instructorInfo;

            var course = _fixture.Build<Course>()
                .With(c => c.Id, 1)
                .With(c => c.Title, "Course A")
                .Without(c => c.Modules)
                .Create();

            _courseRepo.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Course> { course });

            var modules = new List<Module>
            {
                new Module { Id = 10, Title = "Module A", Content = "C1" }
            };

            _moduleRepo.Setup(m => m.GetByCourseIdAsync(1))
                .ReturnsAsync(modules);

            var service = CreateService();

            var list = (await service.GetAllAsync()).ToList();

            Assert.Single(list);
            Assert.Equal("Course A", list[0].Title);
            Assert.Equal("Instructor A", list[0].InstructorName);
        }


        // -------------------------------------------------------
        // GET BY ID
        // -------------------------------------------------------
        [Fact]
        public async Task GetByIdAsync_ShouldReturnDto_WhenExists()
        {
            _userHandler.Response = new UserAuthDto { Name = "Teacher" };

            var course = _fixture.Build<Course>()
                .With(c => c.Id, 20)
                .Without(c => c.Modules)
                .Create();

            _courseRepo.Setup(r => r.GetByIdAsync(20)).ReturnsAsync(course);

            var modules = new List<Module>
            {
                new Module { Id = 5, Title = "Intro", Content = "ABC" }
            };
            _moduleRepo.Setup(m => m.GetByCourseIdAsync(20)).ReturnsAsync(modules);

            var service = CreateService();

            var dto = await service.GetByIdAsync(20);

            Assert.NotNull(dto);
            Assert.Equal("Teacher", dto!.InstructorName);
            Assert.Single(dto.Modules);
        }


        // -------------------------------------------------------
        // CREATE
        // -------------------------------------------------------
        [Fact]
        public async Task CreateAsync_ShouldCreateCourseAndModules()
        {
            var dto = new CourseDto
            {
                Title = "Backend",
                Description = "Desc",
                CategoryId = 1,
                InstructorUserId = Guid.NewGuid().ToString(),
                Modules = new List<ModuleDto>
                {
                    new ModuleDto { Title = "M1", Content = "C1" }
                }
            };

            Course? savedCourse = null;

            _courseRepo.Setup(r => r.AddAsync(It.IsAny<Course>()))
                .Callback<Course>(c => savedCourse = c)
                .Returns(Task.CompletedTask);

            var service = CreateService();

            var result = await service.CreateAsync(dto);

            Assert.NotNull(savedCourse);
            Assert.Equal("Backend", savedCourse!.Title);

            _moduleRepo.Verify(m => m.AddAsync(It.IsAny<Module>()), Times.Once);
        }


        // -------------------------------------------------------
        // UPDATE
        // -------------------------------------------------------
        [Fact]
        public async Task UpdateAsync_ShouldModifyCourse()
        {
            var course = _fixture.Build<Course>()
                .With(c => c.Id, 1)
                .Without(c => c.Modules)
                .Create();

            course.Modules = new List<Module>();

            _courseRepo.Setup(r => r.GetByIdWithModulesAsync(1)).ReturnsAsync(course);

            var dto = new UpdateCourseDto
            {
                Title = "Updated",
                Description = "New",
                CategoryId = 99,
                Modules = new List<UpdateModuleDto>
                {
                    new UpdateModuleDto { Title = "New M", Content = "New C" }
                }
            };

            var service = CreateService();

            var updated = await service.UpdateAsync(1, dto);

            Assert.NotNull(updated);
            Assert.Equal("Updated", updated!.Title);
            Assert.Single(updated.Modules);
        }


        // -------------------------------------------------------
        // ENROLL
        // -------------------------------------------------------
        [Fact]
        public async Task EnrollUserAsync_ShouldAddEnrollment_WhenNotExists()
        {
            var userId = Guid.NewGuid();
            var courseId = 7;

            _enrollRepo
                .Setup(r => r.IsUserEnrolledAsync(userId.ToString(), courseId))
                .ReturnsAsync(false);

            var service = CreateService();

            var ok = await service.EnrollUserAsync(
                userId,
                courseId,
                "test@mail.com",
                "Bearer fake-token"
            );

            Assert.True(ok);
            _enrollRepo.Verify(r => r.AddAsync(It.IsAny<Enrollment>()), Times.Once);
        }


        // -------------------------------------------------------
        // GET UNFINISHED COURSES
        // -------------------------------------------------------
        [Fact]
        public async Task GetAllUnfinishedCoursesAsync_ShouldReturnUnfinishedCourses()
        {
            var instructorId = Guid.NewGuid();
            var courses = new List<Course>
            {
                new Course
                {
                    Id = 3,
                    Title = "Draft Course",
                    Description = "D",
                    CategoryId = 1,
                    InstructorUserId = instructorId,
                    IsDraft = true,
                    IsDeleted = false
                }
            };

            _courseRepo.Setup(r => r.GetAllAsync())
                .ReturnsAsync(courses);

            var service = CreateService();

            var res = (await service.GetAllUnfinishedCoursesAsync(instructorId)).ToList();

            Assert.NotEmpty(res);
            Assert.Single(res);
            Assert.Equal(3, res[0].Id);
        }

        // -------------------------------------------------------
        // CONTINUE COURSE
        // -------------------------------------------------------
        [Fact]
        public async Task ContinueUnfinishedCourseAsync_ShouldReturnTrue_IfCourseExists()
        {
            _courseRepo.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(new Course());

            var service = CreateService();

            Assert.True(await service.ContinueUnfinishedCourseAsync(2));
        }


        // -------------------------------------------------------
        // PUBLISH COURSE
        // -------------------------------------------------------
        [Fact]
        public async Task PublishCourseIfReadyAsync_ShouldPublish_WhenNoMissingQuizzes()
        {
            var course = new Course { Id = 10, IsDeleted = true };

            _courseRepo.Setup(r => r.GetByIdAllowDeletedAsync(10)).ReturnsAsync(course);

            _moduleRepo.Setup(m => m.GetByCourseIdAsync(10))
                .ReturnsAsync(new List<Module> { new Module { Id = 1 } });

            // Assessment returns empty list → all quizzes exist
            _assessmentHandler.Response = new List<int>();

            var service = CreateService();

            var ok = await service.PublishCourseIfReadyAsync(
                10,
                "Bearer fake-token"
            );

            Assert.True(ok);
            Assert.False(course.IsDeleted);
        }

        [Fact]
        public async Task PublishCourseIfReadyAsync_ShouldReturnFalse_WhenMissingQuizzes()
        {
            var course = new Course { Id = 10, IsDeleted = true };

            _courseRepo.Setup(r => r.GetByIdAllowDeletedAsync(10)).ReturnsAsync(course);

            _moduleRepo.Setup(m => m.GetByCourseIdAsync(10))
                .ReturnsAsync(new List<Module> { new Module { Id = 1 } });

            // Missing quizzes
            _assessmentHandler.Response = new List<int> { 1, 2 };

            var service = CreateService();

            Assert.False(
                await service.PublishCourseIfReadyAsync(
                    10,
                    "Bearer fake-token"
                )
            );

        }


        // -------------------------------------------------------
        // USER COURSES
        // -------------------------------------------------------
        [Fact]
        public async Task GetUserEnrolledCoursesAsync_ShouldReturnList()
        {
            _enrollRepo.Setup(r => r.GetByUserIdAsync("u1"))
                .ReturnsAsync(new List<Enrollment>
                {
                    new Enrollment { CourseId = 1 }
                });

            _courseRepo.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
                .ReturnsAsync(new List<Course>
                {
                    new Course { Id = 1 }
                });

            var service = CreateService();

            var list = (await service.GetUserEnrolledCoursesAsync("u1")).ToList();

            Assert.Single(list);
            Assert.Equal(1, list[0].Id);
        }



        // -------------------------------------------------------
        // Helper to create service
        // -------------------------------------------------------
        private CourseServiceimpl CreateService()
        {
            return new CourseServiceimpl(
                _courseRepo.Object,
                _enrollRepo.Object,
                _moduleRepo.Object,
                _httpFactory.Object,
                _mapperMock.Object,
                _userContextMock.Object,
                _httpContextAccessorMock.Object
            );
        }
    }
}
