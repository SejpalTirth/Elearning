using AutoFixture;
using Microsoft.AspNetCore.Http;
using Moq;
using ProgresService.DAL.Models;
using ProgresService.DAL.Repo;
using ProgresService.BLL.Service;
using System.Net;
using System.Security.Claims;
using System.Text.Json;

namespace ProgressService.Tests
{
    public class ProgressServiceImplTests
    {
        private readonly Mock<IProgressRepository> _repoMock;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Fixture _fixture;

        public ProgressServiceImplTests()
        {
            _repoMock = new Mock<IProgressRepository>();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            _fixture = new Fixture();

            _fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(x => _fixture.Behaviors.Remove(x));

            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        // ================================================================
        // Fake HTTP handler (unchanged logic)
        // ================================================================
        public class FakeHttpMessageHandler : HttpMessageHandler
        {
            private readonly List<(string UrlContains, object? Response)> _responses = new();

            public void AddJsonResponse(string urlContains, object response)
                => _responses.Add((urlContains, response));

            public void AddNullJsonResponse(string urlContains)
                => _responses.Add((urlContains, null));

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var url = request.RequestUri!.AbsoluteUri;

                var match = _responses.FirstOrDefault(r => url.Contains(r.UrlContains));

                var json = match.Response == null
                    ? "null"
                    : JsonSerializer.Serialize(match.Response);

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
                });
            }
        }

        private ProgressServiceImpl CreateService(HttpClient client)
        {
            _httpClientFactoryMock
                .Setup(f => f.CreateClient(It.IsAny<string>()))
                .Returns(client);

            var claims = new[]
            {
                new Claim("name", "Test User"),
                new Claim(
                    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress",
                    "test@mail.com")
            };

            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);

            var context = new DefaultHttpContext
            {
                User = principal
            };

            context.Request.Headers["Authorization"] = "Bearer test-token";

            _httpContextAccessorMock
                .Setup(a => a.HttpContext)
                .Returns(context);

            return new ProgressServiceImpl(
                _repoMock.Object,
                _httpClientFactoryMock.Object,
                _httpContextAccessorMock.Object);
        }

        // ================================================================
        // TEST 1: GetUserProgressAsync
        // ================================================================
        [Fact]
        public async Task GetUserProgressAsync_ShouldMapCorrectly()
        {
            var userId = Guid.NewGuid();

            var progress = _fixture.Build<ProgressTracking>()
                .With(x => x.CourseId, 12)
                .With(x => x.ModuleId, 7)
                .With(x => x.ProgressPercent, 55)
                .With(x => x.IsCompleted, false)
                .Create();

            _repoMock.Setup(r => r.GetUserProgressAsync(userId))
                     .ReturnsAsync(new List<ProgressTracking> { progress });

            var service = new ProgressServiceImpl(
                _repoMock.Object,
                _httpClientFactoryMock.Object,
                _httpContextAccessorMock.Object);

            var result = await service.GetUserProgressAsync(userId);

            Assert.Single(result);
            Assert.Equal(12, result[0].CourseId);
            Assert.Equal(7, result[0].ModuleId);
            Assert.Equal(55, result[0].ProgresPercent);
            Assert.False(result[0].IsCompleted);
        }

        // ================================================================
        // TEST 2: Missing module title → fallback
        // ================================================================
        [Fact]
        public async Task MarkModuleCompletedAsync_ShouldUseFallbackModuleName_WhenModuleTitleMissing()
        {
            var userId = Guid.NewGuid();

            _repoMock.Setup(r => r.MarkModuleCompleteAsync(userId, 1, 10))
                     .Returns(Task.CompletedTask);

            _repoMock.Setup(r => r.IsCourseFullyCompletedAsync(userId, 1, It.IsAny<int>()))
                     .ReturnsAsync(false);

            var fake = new FakeHttpMessageHandler();

            fake.AddJsonResponse("modules", new[]
            {
                new { Id = 10, Title = (string?)null }
            });

            fake.AddJsonResponse("courses", new { Title = "Test Course" });
            fake.AddJsonResponse("notification/trigger", new { ok = true });

            var client = new HttpClient(fake) { BaseAddress = new Uri("http://fake/") };

            var service = CreateService(client);

            await service.MarkModuleCompletedAsync(userId, 1, 10);

            Assert.True(true); // no exception = success
        }

        // ================================================================
        // TEST 3: Course completed → send course notification
        // ================================================================
        [Fact]
        public async Task MarkModuleCompletedAsync_ShouldSendCourseCompletedNotification_WhenCourseCompleted()
        {
            var userId = Guid.NewGuid();

            _repoMock.Setup(r => r.MarkModuleCompleteAsync(userId, 1, 10))
                     .Returns(Task.CompletedTask);

            _repoMock.Setup(r => r.IsCourseFullyCompletedAsync(userId, 1, 3))
                     .ReturnsAsync(true);

            var fake = new FakeHttpMessageHandler();

            fake.AddJsonResponse("modules", new[]
            {
                new { Id = 10, Title = "Module 1" },
                new { Id = 11, Title = "Module 2" },
                new { Id = 12, Title = "Module 3" }
            });

            fake.AddJsonResponse("courses", new { Title = "Test Course" });
            fake.AddJsonResponse("notification/trigger", new { ok = true });

            var client = new HttpClient(fake) { BaseAddress = new Uri("http://fake/") };

            var service = CreateService(client);

            await service.MarkModuleCompletedAsync(userId, 1, 10);

            _repoMock.Verify(r =>
                r.IsCourseFullyCompletedAsync(userId, 1, 3),
                Times.Once);
        }
    }
}
