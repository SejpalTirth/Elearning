using AutoFixture;
using AutoMapper;
using CourseService.BLL.Service;
using CourseService.BLL.UserContext;
using CourseService.DAL.Models;
using CourseService.DAL.Repo;
using DTOs._2CourseService;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace LMS.Tests.CourseService
{
    public class CourseServiceTests
    {
        private readonly Mock<ICourseRepository> _courseRepo = new();
        private readonly Mock<IEnrollmentRepository> _enrollRepo = new();
        private readonly Mock<IModuleRepository> _moduleRepo = new();
        private readonly Mock<IHttpClientFactory> _httpFactory = new();
        private readonly Mock<IMapper> _mapper = new();
        private readonly Mock<IUserContextAccessor> _userContext = new();
        private readonly Mock<IHttpContextAccessor> _httpContext = new();

        private readonly Fixture _fixture = new();

        private readonly FakeHandler _userHandler = new();
        private readonly FakeHandler _assessmentHandler = new();

        public CourseServiceTests()
        {
            var userClient = new HttpClient(_userHandler)
            {
                BaseAddress = new Uri("https://user/")
            };

            var assessmentClient = new HttpClient(_assessmentHandler)
            {
                BaseAddress = new Uri("https://assessment/")
            };

            _httpFactory.Setup(x => x.CreateClient("UserService"))
                .Returns(userClient);

            _httpFactory.Setup(x => x.CreateClient("AssessmentService"))
                .Returns(assessmentClient);

            _httpContext.Setup(x => x.HttpContext)
                .Returns(new DefaultHttpContext());
        }

        // ------------------------------------------------
        // GET ALL
        // ------------------------------------------------
        [Fact]
        public async Task GetAllAsync_ReturnsMappedCourses()
        {
            var course = new Course { Id = 1, Title = "C1", InstructorUserId = Guid.NewGuid() };

            _courseRepo.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Course> { course });

            _userHandler.Response = new PublicUserDto { Name = "Instructor" };

            _mapper.Setup(m => m.Map<CourseResponseDto>(course))
                .Returns(new CourseResponseDto { Id = 1, Title = "C1" });

            var svc = CreateService();

            var result = (await svc.GetAllAsync()).ToList();

            Assert.Single(result);
            Assert.Equal("Instructor", result[0].InstructorName);
        }

        // ------------------------------------------------
        // GET BY ID
        // ------------------------------------------------
        [Fact]
        public async Task GetByIdAsync_ReturnsDto_WhenFound()
        {
            var course = new Course
            {
                Id = 5,
                InstructorUserId = Guid.NewGuid(),
                Modules = new List<Module>()
            };

            _courseRepo.Setup(r => r.GetByIdWithModulesAsync(5))
                .ReturnsAsync(course);

            _userHandler.Response = new PublicUserDto { Name = "Teacher" };

            _mapper.Setup(m => m.Map<CourseResponseDto>(course))
                .Returns(new CourseResponseDto { Id = 5 });

            var svc = CreateService();

            var dto = await svc.GetByIdAsync(5);

            Assert.NotNull(dto);
            Assert.Equal("Teacher", dto!.InstructorName);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenMissing()
        {
            _courseRepo.Setup(r => r.GetByIdWithModulesAsync(99))
                .ReturnsAsync((Course?)null);

            var svc = CreateService();

            Assert.Null(await svc.GetByIdAsync(99));
        }

        // ------------------------------------------------
        // CREATE
        // ------------------------------------------------
        [Fact]
        public async Task CreateAsync_AddsCourse()
        {
            var dto = _fixture.Create<CourseDto>();
            var course = new Course { Title = dto.Title };

            _mapper.Setup(m => m.Map<Course>(dto))
                .Returns(course);

            var svc = CreateService();

            var result = await svc.CreateAsync(dto);

            _courseRepo.Verify(r => r.AddAsync(course), Times.Once);
            _courseRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
            Assert.Equal(dto.Title, result.Title);
        }

        // ------------------------------------------------
        // CONTINUE COURSE
        // ------------------------------------------------
        [Fact]
        public async Task ContinueUnfinishedCourseAsync_ReturnsTrue_WhenExists()
        {
            _courseRepo.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(new Course());

            var svc = CreateService();

            Assert.True(await svc.ContinueUnfinishedCourseAsync(1));
        }

        [Fact]
        public async Task ContinueUnfinishedCourseAsync_ReturnsFalse_WhenMissing()
        {
            _courseRepo.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync((Course?)null);

            var svc = CreateService();

            Assert.False(await svc.ContinueUnfinishedCourseAsync(1));
        }

        // ------------------------------------------------
        // PUBLISH COURSE
        // ------------------------------------------------
        [Fact]
        public async Task PublishCourseIfReadyAsync_ReturnsTrue_WhenNoMissingQuizzes()
        {
            var course = new Course { Id = 10 };

            _courseRepo.Setup(r => r.GetByIdAllowDeletedAsync(10))
                .ReturnsAsync(course);

            _moduleRepo.Setup(m => m.GetByCourseIdAsync(10))
                .ReturnsAsync(new List<Module> { new() });

            _assessmentHandler.Response = new List<int>(); // no missing quizzes

            var svc = CreateService();

            var ok = await svc.PublishCourseIfReadyAsync(10, "Bearer token");

            Assert.True(ok);
            Assert.False(course.IsDeleted);
        }

        // ------------------------------------------------
        // USER ENROLLED COURSES
        // ------------------------------------------------
        [Fact]
        public async Task GetUserEnrolledCoursesAsync_ReturnsCourses()
        {
            _enrollRepo.Setup(r => r.GetByUserIdAsync("u1"))
                .ReturnsAsync(new List<Enrollment>
                {
                    new Enrollment { CourseId = 3 }
                });

            _courseRepo.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
                .ReturnsAsync(new List<Course>
                {
                    new Course { Id = 3 }
                });

            var svc = CreateService();

            var list = (await svc.GetUserEnrolledCoursesAsync("u1")).ToList();

            Assert.Single(list);
            Assert.Equal(3, list[0].Id);
        }

        // ------------------------------------------------
        // UPDATE COURSE
        // ------------------------------------------------
        [Fact]
        public async Task UpdateAsync_UpdatesExistingModulesAndAddsNewModules()
        {
            // Arrange
            var course = new Course
            {
                Id = 1,
                Title = "Original Title",
                Modules = new List<Module>
        {
            new Module { Id = 1, Title = "Old Module", Content = "Old Content" }
        }
            };

            _courseRepo.Setup(r => r.GetByIdWithModulesAsync(1))
                       .ReturnsAsync(course);

            var updateDto = new UpdateCourseDto
            {
                Title = "Updated Title",
                Modules = new List<UpdateModuleDto>
        {
            new UpdateModuleDto { Id = 1, Title = "Old Module Updated", Content = "New Content" }, // update existing
            new UpdateModuleDto { Id = 0, Title = "New Module", Content = "Brand New Content" }     // add new
        }
            };

            // Update course itself
            _mapper.Setup(m => m.Map(updateDto, course))
                   .Callback<UpdateCourseDto, Course>((dto, c) => c.Title = dto.Title);

            // Add new module
            _mapper.Setup(m => m.Map<Module>(It.IsAny<UpdateModuleDto>()))
                   .Returns<UpdateModuleDto>(dto => new Module { Title = dto.Title, Content = dto.Content });

            // Update existing module
            _mapper.Setup(m => m.Map(It.IsAny<UpdateModuleDto>(), It.IsAny<Module>()))
                   .Callback<UpdateModuleDto, Module>((src, dest) =>
                   {
                       dest.Title = src.Title;
                       dest.Content = src.Content;
                   });

            var svc = CreateService();

            // Act
            var result = await svc.UpdateAsync(1, updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Title", result!.Title);
            Assert.Equal(2, result.Modules.Count); // old updated + new added
            Assert.Contains(result.Modules, m => m.Title == "Old Module Updated");
            Assert.Contains(result.Modules, m => m.Title == "New Module");

            _courseRepo.Verify(r => r.SaveChangesAsync(), Times.AtLeastOnce);
        }

        // ------------------------------------------------
        // ENROLL USER
        // ------------------------------------------------
        [Fact]
        public async Task EnrollUserAsync_EnrollsUserAndSendsNotification()
        {
            var userId = Guid.NewGuid();
            _enrollRepo.Setup(r => r.IsUserEnrolledAsync(userId.ToString(), 1)).ReturnsAsync(false);
            _courseRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Course { Title = "TestCourse" });

            _httpContext.Setup(x => x.HttpContext)
                        .Returns(new DefaultHttpContext { User = new System.Security.Claims.ClaimsPrincipal() });

            var svc = CreateService();

            var result = await svc.EnrollUserAsync(userId, 1, "user@test.com", "Bearer token");

            Assert.True(result);
            _enrollRepo.Verify(r => r.AddAsync(It.IsAny<Enrollment>()), Times.Once);
            _enrollRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        // ------------------------------------------------
        // GET ALL UNFINISHED COURSES
        // ------------------------------------------------
        [Fact]
        public async Task GetAllUnfinishedCoursesAsync_FiltersByInstructorAndDraft()
        {
            var instructorId = Guid.NewGuid();
            var courses = new List<Course>
    {
        new Course { InstructorUserId = instructorId, IsDraft = true, IsDeleted = false },
        new Course { InstructorUserId = instructorId, IsDraft = false, IsDeleted = false },
        new Course { InstructorUserId = Guid.NewGuid(), IsDraft = true, IsDeleted = false }
    };

            _courseRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(courses);

            var svc = CreateService();
            var result = (await svc.GetAllUnfinishedCoursesAsync(instructorId)).ToList();

            Assert.Single(result);
            Assert.True(result.All(c => c.IsDraft && !c.IsDeleted && c.InstructorUserId == instructorId));
        }

        // ------------------------------------------------
        // DELETE AND RESTORE
        // ------------------------------------------------
        [Fact]
        public async Task DeleteAsync_SetsIsDeletedAndDraftFlags()
        {
            var course = new Course();
            _courseRepo.Setup(r => r.GetByIdWithModulesAsync(1)).ReturnsAsync(course);

            var svc = CreateService();
            var result = await svc.DeleteAsync(1);

            Assert.True(result);
            Assert.True(course.IsDeleted);
            Assert.False(course.IsDraft);
            _courseRepo.Verify(r => r.UpdateAsync(course), Times.Once);
            _courseRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task RestoreAsync_SetsIsDeletedFalse()
        {
            var course = new Course { IsDeleted = true };
            _courseRepo.Setup(r => r.GetByIdAllowDeletedAsync(1)).ReturnsAsync(course);

            var svc = CreateService();
            var result = await svc.RestoreAsync(1);

            Assert.True(result);
            Assert.False(course.IsDeleted);
            _courseRepo.Verify(r => r.UpdateAsync(course), Times.Once);
            _courseRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        // ------------------------------------------------
        // FETCH INSTRUCTOR EDGE CASES
        // ------------------------------------------------
        [Fact]
        public async Task FetchInstructorAsync_ReturnsUnknown_WhenIdEmpty()
        {
            var svc = CreateService();
            var privateMethod = typeof(CourseServiceimpl)
                .GetMethod("FetchInstructorAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var result = await (Task<CourseServiceimpl.InstructorDto>)privateMethod!.Invoke(svc, new object[] { Guid.Empty })!;

            Assert.Equal("Unknown Instructor", result.Name);
        }


        // ------------------------------------------------
        // Helper
        // ------------------------------------------------
        private CourseServiceimpl CreateService()
        {
            return new CourseServiceimpl(
                _courseRepo.Object,
                _enrollRepo.Object,
                _moduleRepo.Object,
                _httpFactory.Object,
                _mapper.Object,
                _userContext.Object,
                _httpContext.Object
            );
        }

        // ------------------------------------------------
        // Fake HTTP handler
        // ------------------------------------------------
        private class FakeHandler : HttpMessageHandler
        {
            public object? Response { get; set; }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                var json = JsonSerializer.Serialize(Response);
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json)
                });
            }
        }
    }
}