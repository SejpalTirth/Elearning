using CourseService.BLL.DTOs;
using CourseService.BLL.Service;
using CourseService.DAL.Models;
using CourseService.DAL.Repo;
using Moq;
using System.Text.Json;
using System.Net;

namespace LMS.Tests.CourseServiceTests
{
    public class CourseServiceTests
    {
        private readonly Mock<ICourseRepository> _courseRepo;
        private readonly Mock<IEnrollmentRepository> _enrollRepo;
        private readonly Mock<IModuleRepository> _moduleRepo;
        private readonly Mock<IHttpClientFactory> _httpClientFactory;

        private readonly FakeHttpHandler _userHandler;
        private readonly FakeHttpHandler _progressHandler;

        private readonly HttpClient _fakeUserClient;
        private readonly HttpClient _fakeProgressClient;

        public CourseServiceTests()
        {
            _courseRepo = new Mock<ICourseRepository>();
            _enrollRepo = new Mock<IEnrollmentRepository>();
            _moduleRepo = new Mock<IModuleRepository>();
            _httpClientFactory = new Mock<IHttpClientFactory>();

            // Create handlers
            _userHandler = new FakeHttpHandler();
            _progressHandler = new FakeHttpHandler();

            // Create HttpClients using handlers and set BaseAddress so relative URLs work
            _fakeUserClient = new HttpClient(_userHandler)
            {
                BaseAddress = new Uri("http://fake-user/")    // important
            };
            _fakeProgressClient = new HttpClient(_progressHandler)
            {
                BaseAddress = new Uri("http://fake-progress/") // important
            };

            // Mock HttpClientFactory to return our configured clients
            _httpClientFactory.Setup(f => f.CreateClient("UserService"))
                .Returns(_fakeUserClient);

            _httpClientFactory.Setup(f => f.CreateClient("ProgressService"))
                .Returns(_fakeProgressClient);
        }


        // --------------------------
        // FAKE HTTP HANDLER
        // --------------------------
        private class FakeHttpHandler : HttpMessageHandler
        {
            public object? ResponseToSend { get; set; }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                var json = JsonSerializer.Serialize(ResponseToSend ?? new { });
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json)
                });
            }
        }

        // --------------------------------------------------------
        // GET ALL
        // --------------------------------------------------------
        [Fact]
        public async Task GetAllAsync_ShouldReturnMappedCourses()
        {
            _userHandler.ResponseToSend = new UserAuthDto
            {
                Id = Guid.NewGuid(),
                Email = "inst@mail.com",
                Name = "Instructor"
            };

            _courseRepo.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Course>
                {
                    new Course { Id = 1, Title = "C# Basics", InstructorUserId = Guid.NewGuid() }
                });

            _moduleRepo.Setup(m => m.GetByCourseIdAsync(1))
                .ReturnsAsync(new List<Module>
                {
                    new Module { Id = 20, Title = "Intro" }
                });

            var service = new Courseservice(
                _courseRepo.Object,
                _enrollRepo.Object,
                _moduleRepo.Object,
                _httpClientFactory.Object
            );

            var result = (await service.GetAllAsync()).ToList();

            Assert.Single(result);
            Assert.Equal("C# Basics", result[0].Title);
            Assert.Equal("Instructor", result[0].InstructorName);
        }

        // --------------------------------------------------------
        // GET BY ID
        // --------------------------------------------------------
        [Fact]
        public async Task GetByIdAsync_WhenFound_ReturnsMappedCourse()
        {
            _userHandler.ResponseToSend = new UserAuthDto
            {
                Id = Guid.NewGuid(),
                Email = "inst@mail.com",
                Name = "Teacher"
            };

            _courseRepo.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(new Course
                {
                    Id = 1,
                    Title = "Math",
                    InstructorUserId = Guid.NewGuid()
                });

            _moduleRepo.Setup(m => m.GetByCourseIdAsync(1))
                .ReturnsAsync(new List<Module>
                {
                    new Module { Id = 10, Title = "Algebra", Content = "A+" }
                });

            var service = new Courseservice(
                _courseRepo.Object,
                _enrollRepo.Object,
                _moduleRepo.Object,
                _httpClientFactory.Object
            );

            var result = await service.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal("Math", result!.Title);
            Assert.Equal("Teacher", result.InstructorName);
        }

        // --------------------------------------------------------
        // CREATE
        // --------------------------------------------------------
        [Fact]
        public async Task CreateAsync_ShouldCreateCourseAndModules()
        {
            var dto = new CourseDto
            {
                Title = "New Course",
                Description = "Desc",
                CategoryId = 1,
                InstructorUserId = Guid.NewGuid().ToString(),
                Modules = new()
                {
                    new ModuleDto { Title = "M1", Content = "C1" }
                }
            };

            Course? capturedCourse = null;

            _courseRepo.Setup(r => r.AddAsync(It.IsAny<Course>()))
                .Callback<Course>(c => capturedCourse = c)
                .Returns(Task.CompletedTask);

            var service = new Courseservice(
                _courseRepo.Object,
                _enrollRepo.Object,
                _moduleRepo.Object,
                _httpClientFactory.Object
            );

            var result = await service.CreateAsync(dto);

            Assert.NotNull(capturedCourse);
            Assert.Equal("New Course", capturedCourse!.Title);

            _moduleRepo.Verify(m => m.AddAsync(It.IsAny<Module>()), Times.Once);
        }

        // --------------------------------------------------------
        // UPDATE
        // --------------------------------------------------------
        [Fact]
        public async Task UpdateAsync_WhenCourseExists_ShouldUpdateFields()
        {
            var course = new Course
            {
                Id = 1,
                Title = "Old",
                Description = "Desc",
                CategoryId = 1,
                Modules = new List<Module>
                {
                    new Module { Id = 10, Title = "Old M" }
                }
            };

            _courseRepo.Setup(r => r.GetByIdWithModulesAsync(1))
                .ReturnsAsync(course);

            var dto = new UpdateCourseDto
            {
                Title = "Updated",
                Description = "NewDesc",
                CategoryId = 5,
                Modules = new()
                {
                    new UpdateModuleDto { Id = 0, Title = "M1", Content = "C1" }
                }
            };

            var service = new Courseservice(
                _courseRepo.Object,
                _enrollRepo.Object,
                _moduleRepo.Object,
                _httpClientFactory.Object
            );

            var updated = await service.UpdateAsync(1, dto);

            Assert.NotNull(updated);
            Assert.Equal("Updated", updated!.Title);
            Assert.Single(updated.Modules);
        }

        // --------------------------------------------------------
        // ENROLL
        // --------------------------------------------------------
        [Fact]
        public async Task EnrollUserAsync_WhenNotAlreadyEnrolled_ShouldReturnTrue()
        {
            _enrollRepo.Setup(r => r.IsUserEnrolledAsync("u1", 5))
                .ReturnsAsync(false);

            var service = new Courseservice(
                _courseRepo.Object,
                _enrollRepo.Object,
                _moduleRepo.Object,
                _httpClientFactory.Object
            );

            var result = await service.EnrollUserAsync(new EnrollRequestDto
            {
                CourseId = 5,
                UserId = "u1"
            });

            Assert.True(result);
            _enrollRepo.Verify(r => r.AddAsync(It.IsAny<Enrollment>()), Times.Once);
        }

        // --------------------------------------------------------
        // DELETE (SOFT DELETE)
        // --------------------------------------------------------
        [Fact]
        public async Task DeleteAsync_WhenCourseExists_ShouldMarkAsDeleted()
        {
            var course = new Course { Id = 1 };

            _courseRepo.Setup(r => r.GetByIdWithModulesAsync(1))
                .ReturnsAsync(course);

            var service = new Courseservice(
                _courseRepo.Object,
                _enrollRepo.Object,
                _moduleRepo.Object,
                _httpClientFactory.Object
            );

            var result = await service.DeleteAsync(1);

            Assert.True(result);
            Assert.True(course.IsDeleted);
        }
    }
}
