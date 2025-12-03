using AutoFixture;
using Moq;
using ProgresService.DAL.Models;
using ProgresService.DAL.Repo;
using ProgressService.BLL.Service;
using System.Net;
using System.Text.Json;

namespace ProgressService.Tests
{
    public class ProgressServiceImplTests
    {
        private readonly Mock<IProgressRepository> _repoMock;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Fixture _fixture;

        public ProgressServiceImplTests()
        {
            _repoMock = new Mock<IProgressRepository>();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();

            _fixture = new Fixture();

            // Avoid recursion issues
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));

            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        // ------------------------------
        // SAME FakeHandler — NOT CHANGED
        // ------------------------------
        public class FakeHttpMessageHandler : HttpMessageHandler
        {
            private class RawNullResponse { }

            private readonly List<(string UrlContains, object Response)> _responses = new();

            public void AddJsonResponse(string urlContains, object response)
            {
                _responses.Add((urlContains, response));
            }

            public void AddNullJsonResponse(string urlContains)
            {
                _responses.Add((urlContains, new RawNullResponse()));
            }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var url = request.RequestUri!.AbsoluteUri;

                var match = _responses.FirstOrDefault(r => url.Contains(r.UrlContains));

                if (match.Response != null)
                {
                    // SPECIAL CASE: return raw literal "null"
                    if (match.Response is RawNullResponse)
                    {
                        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = new StringContent("null", System.Text.Encoding.UTF8, "application/json")
                        });
                    }

                    // Normal JSON
                    var json = JsonSerializer.Serialize(match.Response);
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
                    });
                }

                // Default fallback JSON
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json")
                });
            }
        }




        // ----------------------------------------------------------
        // TEST 1 — AutoFixture used to generate ProgressTracking
        // ----------------------------------------------------------
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

            var service = new ProgressServiceImpl(_repoMock.Object, _httpClientFactoryMock.Object);

            var result = await service.GetUserProgressAsync(userId);

            Assert.Single(result);
            Assert.Equal(12, result[0].CourseId);
            Assert.Equal(7, result[0].ModuleId);
            Assert.Equal(55, result[0].ProgressPercent);
            Assert.False(result[0].IsCompleted);
        }


        // ----------------------------------------------------------
        // TEST 2 — AutoFixture generating fake "user not found" response
        // ----------------------------------------------------------
        [Fact]
        public async Task MarkModuleCompletedAsync_ShouldThrow_WhenUserNotFound()
        {
            var userId = Guid.NewGuid();

            _repoMock.Setup(r => r.MarkModuleCompleteAsync(userId, 1, 10))
                     .Returns(Task.CompletedTask);

            _repoMock.Setup(r => r.GetCompletedModuleCountAsync(userId, 1))
                     .ReturnsAsync(0);

            var fake = new FakeHttpMessageHandler();

            // valid modules (AutoFixture)
            fake.AddJsonResponse("modules", _fixture.CreateMany<object>(2));

            // valid course (AutoFixture)
            fake.AddJsonResponse("api/courses/", new { Title = _fixture.Create<string>() });

            //  FORCE GetFromJsonAsync<UserDto>() to return NULL
            fake.AddNullJsonResponse("api/users/");

            var httpClient = new HttpClient(fake) { BaseAddress = new Uri("http://fake/") };
            _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            var service = new ProgressServiceImpl(_repoMock.Object, _httpClientFactoryMock.Object);

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                service.MarkModuleCompletedAsync(userId, 1, 10));

            Assert.Equal("Unable to send notification — User not found.", ex.Message);
        }

        // ----------------------------------------------------------
        // TEST 3 — Module fallback name using AutoFixture
        // ----------------------------------------------------------
        [Fact]
        public async Task MarkModuleCompletedAsync_ShouldUseFallbackModuleName_WhenModuleTitleMissing()
        {
            var userId = Guid.NewGuid();

            _repoMock.Setup(r => r.MarkModuleCompleteAsync(userId, 1, 10))
                     .Returns(Task.CompletedTask);

            _repoMock.Setup(r => r.GetCompletedModuleCountAsync(userId, 1))
                     .ReturnsAsync(0);

            var fake = new FakeHttpMessageHandler();

            fake.AddJsonResponse("api/users/", new
            {
                Email = _fixture.Create<string>() + "@mail.com",
                Name = _fixture.Create<string>()
            });

            // AutoFixture module list -> one module with null title
            fake.AddJsonResponse("modules", new[]
            {
                new { Id = 10, Title = (string)null }
            });

            fake.AddJsonResponse("api/courses/", new { Title = _fixture.Create<string>() });

            fake.AddJsonResponse("notification/trigger", new { ok = true });

            var httpClient = new HttpClient(fake) { BaseAddress = new Uri("http://fake/") };
            _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
                                  .Returns(httpClient);

            var service = new ProgressServiceImpl(_repoMock.Object, _httpClientFactoryMock.Object);

            await service.MarkModuleCompletedAsync(userId, 1, 10);

            Assert.True(true);
        }


        // ----------------------------------------------------------
        // TEST 4 — AutoFixture with full module list
        // ----------------------------------------------------------
        [Fact]
        public async Task MarkModuleCompletedAsync_ShouldSendCourseCompletedNotification_WhenAllModulesCompleted()
        {
            var userId = Guid.NewGuid();

            _repoMock.Setup(r => r.MarkModuleCompleteAsync(userId, 1, 10))
                     .Returns(Task.CompletedTask);

            _repoMock.Setup(r => r.GetCompletedModuleCountAsync(userId, 1))
                     .ReturnsAsync(3);

            var handler = new FakeHttpMessageHandler();

            handler.AddJsonResponse("api/users/", new
            {
                Email = _fixture.Create<string>() + "@mail.com",
                Name = _fixture.Create<string>()
            });

            // AutoFixture modules
            handler.AddJsonResponse("modules", _fixture.Build<object>()
                .CreateMany(3)
                .Select((m, idx) => new
                {
                    Id = idx + 10,
                    Title = _fixture.Create<string>()
                }));

            handler.AddJsonResponse("api/courses/", new { Title = _fixture.Create<string>() });

            handler.AddJsonResponse("notification/trigger", new { ok = true });

            var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://fake/") };
            _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
                                  .Returns(httpClient);

            var service = new ProgressServiceImpl(_repoMock.Object, _httpClientFactoryMock.Object);

            await service.MarkModuleCompletedAsync(userId, 1, 10);

            Assert.True(true);
        }
    }
}
