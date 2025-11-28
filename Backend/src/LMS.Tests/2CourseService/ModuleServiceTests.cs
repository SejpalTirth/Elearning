using CourseService.BLL.Service;
using CourseService.DAL.Models;
using Moq;
using System.Text.Json;
using System.Net;

namespace LMS.Tests.ModuleServiceTests
{
    public class ModuleServiceTests
    {
        private readonly Mock<IModuleRepository> _moduleRepo;
        private readonly Mock<IHttpClientFactory> _httpFactory;

        private readonly FakeHttpHandler _assessmentHandler;
        private readonly HttpClient _fakeAssessmentClient;

        public ModuleServiceTests()
        {
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

        // -------------------------
        // FAKE HTTP HANDLER
        // -------------------------
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

        // -------------------------------------------------------------
        // GetModuleAndCourseIdAsync
        // -------------------------------------------------------------
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
            _moduleRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync(new Module
            {
                Id = 99,
                CourseId = 5
            });

            var service = CreateService();
            var result = await service.GetModuleAndCourseIdAsync(99);

            Assert.NotNull(result);
            Assert.Equal(99, result!.ModuleId);
            Assert.Equal(5, result.CourseId);
        }

        // -------------------------------------------------------------
        // GetModulesByCourseAsync
        // -------------------------------------------------------------
        [Fact]
        public async Task GetModulesByCourseAsync_ReturnsSummaryList()
        {
            _moduleRepo.Setup(r => r.GetByCourseIdAsync(3))
                .ReturnsAsync(new List<Module>
                {
                    new Module { Id = 1, Title = "M1" },
                    new Module { Id = 2, Title = "M2" }
                });

            var service = CreateService();

            var list = (await service.GetModulesByCourseAsync(3)).ToList();

            Assert.Equal(2, list.Count);
            Assert.Equal("M1", list[0].Title);
        }

        // -------------------------------------------------------------
        // GetModuleContentAsync
        // -------------------------------------------------------------
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
            _moduleRepo.Setup(r => r.GetByIdAsync(50)).ReturnsAsync(
                new Module
                {
                    Id = 50,
                    Title = "Module 50",
                    Content = "Content 50"
                });

            _assessmentHandler.ResponseToSend = 42; // quiz id

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
            _moduleRepo.Setup(r => r.GetByIdAsync(22)).ReturnsAsync(
                new Module
                {
                    Id = 22,
                    Title = "Module 22",
                    Content = "X"
                });

            _assessmentHandler.ThrowException = true;

            var service = CreateService();
            var result = await service.GetModuleContentAsync(22);

            Assert.NotNull(result);
            Assert.Equal(0, result!.QuizId); // fallback to zero
        }
    }
}
