using AutoFixture;
using CourseService.BLL.Service;
using CourseService.DAL.Models;
using Moq;
using System.Net;
using System.Text.Json;

namespace LMS.Tests.ModuleServiceTests
{
    public class ModuleServiceTests
    {
        private readonly Mock<IModuleRepository> _moduleRepo;
        private readonly Mock<IHttpClientFactory> _httpFactory;

        private readonly FakeHttpHandler _assessmentHandler;
        private readonly HttpClient _fakeAssessmentClient;

        private readonly Fixture _fixture;

        public ModuleServiceTests()
        {
            _fixture = new Fixture();

            // Prevent potential nesting (safe default)
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _moduleRepo = new Mock<IModuleRepository>();
            _httpFactory = new Mock<IHttpClientFactory>();

            _assessmentHandler = new FakeHttpHandler();

            _fakeAssessmentClient = new HttpClient(_assessmentHandler)
            {
                BaseAddress = new Uri("http://fake-assessment/")
            };

            _httpFactory.Setup(f => f.CreateClient("AssessmentService"))
                .Returns(_fakeAssessmentClient);
        }

        // -----------------------------------------------------
        // FAKE HTTP HANDLER
        // -----------------------------------------------------
        private class FakeHttpHandler : HttpMessageHandler
        {
            public object? ResponseToSend { get; set; }
            public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;
            public bool ThrowException { get; set; } = false;

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                if (ThrowException)
                    throw new HttpRequestException("Fake failure");

                var json = JsonSerializer.Serialize(ResponseToSend ?? new { });

                return Task.FromResult(new HttpResponseMessage(Status)
                {
                    Content = new StringContent(json)
                });
            }
        }

        private ModuleService CreateService() =>
            new ModuleService(_moduleRepo.Object, _httpFactory.Object);

        // =====================================================================
        // GetModuleAndCourseIdAsync
        // =====================================================================
        [Fact]
        public async Task GetModuleAndCourseIdAsync_WhenNotFound_ReturnsNull()
        {
            _moduleRepo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync((Module?)null);

            var service = CreateService();
            var result = await service.GetModuleAndCourseIdAsync(10);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetModuleAndCourseIdAsync_WhenFound_ReturnsMapping()
        {
            var module = _fixture.Build<Module>()
                .With(m => m.Id, 99)
                .With(m => m.CourseId, 5)
                .Without(m => m.Course)
                .Create();

            _moduleRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync(module);

            var service = CreateService();
            var result = await service.GetModuleAndCourseIdAsync(99);

            Assert.NotNull(result);
            Assert.Equal(99, result!.ModuleId);
            Assert.Equal(5, result.CourseId);
        }

        // =====================================================================
        // GetModulesByCourseAsync
        // =====================================================================
        [Fact]
        public async Task GetModulesByCourseAsync_ReturnsSummaryList()
        {
            var modules = _fixture.Build<Module>()
                .Without(m => m.Course)
                .CreateMany(2)
                .ToList();

            modules[0].Id = 1;
            modules[0].Title = "M1";
            modules[1].Id = 2;
            modules[1].Title = "M2";

            _moduleRepo.Setup(r => r.GetByCourseIdAsync(3))
                .ReturnsAsync(modules);

            var service = CreateService();

            var list = (await service.GetModulesByCourseAsync(3)).ToList();

            Assert.Equal(2, list.Count);
            Assert.Equal("M1", list[0].Title);
        }

        // =====================================================================
        // GetModuleContentAsync
        // =====================================================================
        [Fact]
        public async Task GetModuleContentAsync_WhenModuleNotFound_ReturnsNull()
        {
            _moduleRepo.Setup(r => r.GetByIdAsync(7)).ReturnsAsync((Module?)null);

            var service = CreateService();
            var result = await service.GetModuleContentAsync(7);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetModuleContentAsync_WhenModuleFound_ReturnsContent()
        {
            var module = _fixture.Build<Module>()
                .With(m => m.Id, 50)
                .With(m => m.Title, "Module 50")
                .With(m => m.Content, "Content 50")
                .Without(m => m.Course)
                .Create();

            _moduleRepo.Setup(r => r.GetByIdAsync(50)).ReturnsAsync(module);

            _assessmentHandler.ResponseToSend = 42; // Quiz ID

            var service = CreateService();

            var result = await service.GetModuleContentAsync(50);

            Assert.NotNull(result);
            Assert.Equal(50, result!.Id);
            Assert.Equal("Module 50", result.Title);
            Assert.Equal("Content 50", result.Content);
            Assert.Equal(42, result.QuizId);
        }

        [Fact]
        public async Task GetModuleContentAsync_WhenAssessmentThrows_ReturnsQuizIdZero()
        {
            var module = _fixture.Build<Module>()
                .With(m => m.Id, 22)
                .With(m => m.Title, "Module 22")
                .With(m => m.Content, "X")
                .Without(m => m.Course)
                .Create();

            _moduleRepo.Setup(r => r.GetByIdAsync(22)).ReturnsAsync(module);

            _assessmentHandler.ThrowException = true;

            var service = CreateService();
            var result = await service.GetModuleContentAsync(22);

            Assert.NotNull(result);
            Assert.Equal(0, result!.QuizId);
        }
    }
}
