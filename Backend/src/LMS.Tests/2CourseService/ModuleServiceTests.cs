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

            // Prevent recursion in EF-like entities
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
            public bool ThrowException { get; set; }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                if (ThrowException)
                    throw new HttpRequestException("Simulated failure");

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

            var result = await CreateService().GetModuleAndCourseIdAsync(10);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetModuleAndCourseIdAsync_WhenFound_ReturnsCorrectDto()
        {
            var module = _fixture.Build<Module>()
                .With(m => m.Id, 99)
                .With(m => m.CourseId, 5)
                .Without(m => m.Course)
                .Create();

            _moduleRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync(module);

            var result = await CreateService().GetModuleAndCourseIdAsync(99);

            Assert.NotNull(result);
            Assert.Equal(99, result!.ModuleId);
            Assert.Equal(5, result.CourseId);
        }

        // =====================================================================
        // GetModulesByCourseAsync
        // =====================================================================
        [Fact]
        public async Task GetModulesByCourseAsync_ReturnsModuleSummaryDtos()
        {
            var modules = _fixture.Build<Module>()
                .Without(m => m.Course)
                .CreateMany(2)
                .ToList();

            modules[0].Title = "Intro";
            modules[1].Title = "Advanced";

            _moduleRepo.Setup(r => r.GetByCourseIdAsync(3))
                .ReturnsAsync(modules);

            var list = (await CreateService().GetModulesByCourseAsync(3)).ToList();

            Assert.Equal(2, list.Count);
            Assert.Equal("Intro", list[0].Title);
        }

        // =====================================================================
        // GetModuleContentAsync
        // =====================================================================
        [Fact]
        public async Task GetModuleContentAsync_ReturnsNull_WhenNotFound()
        {
            _moduleRepo.Setup(r => r.GetByIdAsync(7)).ReturnsAsync((Module?)null);

            var result = await CreateService().GetModuleContentAsync(7);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetModuleContentAsync_ReturnsMappedDto_WithQuiz()
        {
            var module = _fixture.Build<Module>()
                .With(m => m.Id, 22)
                .With(m => m.Title, "Module X")
                .With(m => m.Content, "Data")
                .Without(m => m.Course)
                .Create();

            _moduleRepo.Setup(r => r.GetByIdAsync(22)).ReturnsAsync(module);

            _assessmentHandler.ResponseToSend = 15;

            var result = await CreateService().GetModuleContentAsync(22);

            Assert.NotNull(result);
            Assert.Equal(22, result!.Id);
            Assert.Equal("Module X", result.Title);
            Assert.Equal("Data", result.Content);
            Assert.Equal(15, result.QuizId);
        }

        // =====================================================================
        // FetchQuizIdForModule - Error Cases
        // =====================================================================
        [Fact]
        public async Task GetModuleContentAsync_WhenAssessmentThrows_ReturnsQuizIdZero()
        {
            var module = _fixture.Build<Module>()
                .With(m => m.Id, 33)
                .Without(m => m.Course)
                .Create();

            _moduleRepo.Setup(r => r.GetByIdAsync(33)).ReturnsAsync(module);

            _assessmentHandler.ThrowException = true;

            var result = await CreateService().GetModuleContentAsync(33);

            Assert.NotNull(result);
            Assert.Equal(0, result!.QuizId);
        }

        [Fact]
        public async Task GetModuleContentAsync_WhenAssessmentReturnsInvalidJson_ReturnsZero()
        {
            var module = _fixture.Build<Module>()
                .With(m => m.Id, 77)
                .Without(m => m.Course)
                .Create();

            _moduleRepo.Setup(r => r.GetByIdAsync(77)).ReturnsAsync(module);

            // Simulate invalid JSON by sending a string
            _assessmentHandler.ResponseToSend = "not_an_int";

            var result = await CreateService().GetModuleContentAsync(77);

            Assert.NotNull(result);
            Assert.Equal(0, result!.QuizId);
        }

        [Fact]
        public async Task GetModuleContentAsync_WhenAssessmentReturnsNonSuccessStatus_ReturnsZero()
        {
            var module = _fixture.Build<Module>()
                .With(m => m.Id, 40)
                .Without(m => m.Course)
                .Create();

            _moduleRepo.Setup(r => r.GetByIdAsync(40)).ReturnsAsync(module);

            _assessmentHandler.Status = HttpStatusCode.BadRequest;

            var result = await CreateService().GetModuleContentAsync(40);

            Assert.NotNull(result);
            Assert.Equal(0, result!.QuizId);
        }
    }
}
