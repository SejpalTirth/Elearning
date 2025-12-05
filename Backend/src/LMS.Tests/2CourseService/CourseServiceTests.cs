using AutoFixture;
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

        private readonly Fixture _fixture;

        public CourseServiceTests()
        {
            _fixture = new Fixture();

            // Fix circular recursion on EF-like models
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));

            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _courseRepo = new Mock<ICourseRepository>();
            _enrollRepo = new Mock<IEnrollmentRepository>();
            _moduleRepo = new Mock<IModuleRepository>();
            _httpClientFactory = new Mock<IHttpClientFactory>();

            // Fake HTTP handlers
            _userHandler = new FakeHttpHandler();
            _progressHandler = new FakeHttpHandler();

            _fakeUserClient = new HttpClient(_userHandler)
            {
                BaseAddress = new Uri("http://fake-user/")
            };

            _fakeProgressClient = new HttpClient(_progressHandler)
            {
                BaseAddress = new Uri("http://fake-progress/")
            };

            _httpClientFactory.Setup(f => f.CreateClient("UserService"))
                .Returns(_fakeUserClient);

            _httpClientFactory.Setup(f => f.CreateClient("ProgressService"))
                .Returns(_fakeProgressClient);
        }


        // -----------------------------------------
        // FAKE HANDLER
        // -----------------------------------------
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

        // -----------------------------------------
        // GET ALL
        // -----------------------------------------
        [Fact]
        public async Task GetAllAsync_ShouldReturnMappedCourses()
        {
            // Fake user from UserService
            var instructor = _fixture.Build<UserAuthDto>()
                .With(u => u.Name, "Instructor")
                .Create();

            _userHandler.ResponseToSend = instructor;

            var courseEntity = _fixture.Build<Course>()
                .With(c => c.Id, 1)
                .With(c => c.Title, "C# Basics")
                .Without(c => c.Modules)
                .Create();

            _courseRepo.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Course> { courseEntity });

            var moduleList = _fixture.Build<Module>()
                .With(m => m.Title, "Intro")
                .CreateMany(1)
                .ToList();

            _moduleRepo.Setup(m => m.GetByCourseIdAsync(1))
                .ReturnsAsync(moduleList);

            var service = new CourseServiceimpl(
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

        // -----------------------------------------
        // GET BY ID
        // -----------------------------------------
        [Fact]
        public async Task GetByIdAsync_WhenFound_ReturnsMappedCourse()
        {
            var instructor = _fixture.Build<UserAuthDto>()
                .With(u => u.Name, "Teacher")
                .Create();

            _userHandler.ResponseToSend = instructor;

            var course = _fixture.Build<Course>()
                .With(c => c.Id, 1)
                .With(c => c.Title, "Math")
                .Without(c => c.Modules)
                .Create();

            _courseRepo.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(course);

            var modules = _fixture.Build<Module>()
                .With(m => m.Title, "Algebra")
                .With(m => m.Content, "A+")
                .CreateMany(1)
                .ToList();

            _moduleRepo.Setup(m => m.GetByCourseIdAsync(1))
                .ReturnsAsync(modules);

            var service = new CourseServiceimpl(
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

        // -----------------------------------------
        // CREATE
        // -----------------------------------------
        [Fact]
        public async Task CreateAsync_ShouldCreateCourseAndModules()
        {
            var dto = _fixture.Build<CourseDto>()
                .With(d => d.Title, "New Course")
                .With(d => d.Description, "Desc")
                .With(d => d.CategoryId, 1)
                .With(d => d.InstructorUserId, Guid.NewGuid().ToString())
                .With(d => d.Modules, new List<ModuleDto>
                {
                    new ModuleDto { Title = "M1", Content = "C1" }
                })
                .Create();

            Course? capturedCourse = null;

            _courseRepo.Setup(r => r.AddAsync(It.IsAny<Course>()))
                .Callback<Course>(c => capturedCourse = c)
                .Returns(Task.CompletedTask);

            var service = new CourseServiceimpl(
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

        // -----------------------------------------
        // UPDATE
        // -----------------------------------------
        [Fact]
        public async Task UpdateAsync_WhenCourseExists_ShouldUpdateFields()
        {
            var course = _fixture.Build<Course>()
                .With(c => c.Id, 1)
                .With(c => c.CategoryId, 1)
                .With(c => c.Title, "Old")
                .With(c => c.Description, "Desc")
                .Without(c => c.Modules)
                .Create();

            course.Modules = new List<Module>
            {
                _fixture.Build<Module>().With(m => m.Id, 10).With(m => m.Title, "Old M").Create()
            };

            _courseRepo.Setup(r => r.GetByIdWithModulesAsync(1))
                .ReturnsAsync(course);

            var dto = _fixture.Build<UpdateCourseDto>()
                .With(d => d.Title, "Updated")
                .With(d => d.Description, "NewDesc")
                .With(d => d.CategoryId, 5)
                .With(d => d.Modules, new List<UpdateModuleDto>
                {
                    new UpdateModuleDto { Id = 0, Title = "M1", Content = "C1" }
                })
                .Create();

            var service = new CourseServiceimpl(
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

        // -----------------------------------------
        // ENROLL
        // -----------------------------------------
        [Fact]
        public async Task EnrollUserAsync_WhenNotAlreadyEnrolled_ShouldReturnTrue()
        {
            _enrollRepo.Setup(r => r.IsUserEnrolledAsync("u1", 5))
                .ReturnsAsync(false);

            var service = new CourseServiceimpl(
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

        // -----------------------------------------
        // DELETE
        // -----------------------------------------
        [Fact]
        public async Task DeleteAsync_WhenCourseExists_ShouldMarkAsDeleted()
        {
            var course = _fixture.Build<Course>()
                .With(c => c.Id, 1)
                .Without(c => c.Modules)
                .Create();

            _courseRepo.Setup(r => r.GetByIdWithModulesAsync(1))
                .ReturnsAsync(course);

            var service = new CourseServiceimpl(
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
