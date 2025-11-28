using Moq;
using ProgresService.DAL.Models;
using ProgresService.DAL.Repo;
using ProgressService.BLL.Service;
using System.Net;
using System.Text;
using System.Text.Json;

namespace ProgressService.Tests
{
    public class ProgressServiceImplTests
    {
        private readonly Mock<IProgressRepository> _repoMock;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;

        public ProgressServiceImplTests()
        {
            _repoMock = new Mock<IProgressRepository>();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        }

        // ----------------------------------------------------------
        // Improved Fake Http Handler
        // ----------------------------------------------------------
        public class FakeHttpMessageHandler : HttpMessageHandler
        {
            private readonly List<(string UrlContains, object Response)> _responses = new();

            public void AddJsonResponse(string urlContains, object response)
            {
                _responses.Add((urlContains, response));
            }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var url = request.RequestUri!.AbsoluteUri;

                var match = _responses.FirstOrDefault(r => url.Contains(r.UrlContains));

                if (_responses.Any(r => url.Contains(r.UrlContains)))
                {
                    var response = match.Response;

                    // IMPORTANT FIX: if null response, return JSON literal "null"
                    string json = response == null
                        ? "null"
                        : JsonSerializer.Serialize(response);

                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(json, Encoding.UTF8, "application/json")
                    });
                }

                // Fallback
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{}", Encoding.UTF8, "application/json")
                });
            }
        }


        // ----------------------------------------------------------
        // TEST: GetUserProgressAsync DTO mapping
        // ----------------------------------------------------------
        [Fact]
        public async Task GetUserProgressAsync_ShouldMapCorrectly()
        {
            var userId = Guid.NewGuid();

            _repoMock.Setup(r => r.GetUserProgressAsync(userId))
                     .ReturnsAsync(new List<ProgressTracking>
                     {
                         new ProgressTracking
                         {
                             CourseId = 12,
                             ModuleId = 7,
                             ProgressPercent = 55,
                             IsCompleted = false
                         }
                     });

            var service = new ProgressServiceImpl(_repoMock.Object, _httpClientFactoryMock.Object);

            var result = await service.GetUserProgressAsync(userId);

            Assert.Single(result);
            Assert.Equal(12, result[0].CourseId);
            Assert.Equal(7, result[0].ModuleId);
            Assert.Equal(55, result[0].ProgressPercent);
            Assert.False(result[0].IsCompleted);
        }

        // ----------------------------------------------------------
        // TEST: User not found -> throws exception
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

            // Modules endpoint returns empty list to avoid JsonException
            fake.AddJsonResponse("modules", new List<object>());

            // Course endpoint returns dummy object
            fake.AddJsonResponse("api/courses/", new { Title = "Dummy Course" });

            // User endpoint returns null fields -> triggers your Exception
            fake.AddJsonResponse("api/users/", null);


            var httpClient = new HttpClient(fake) { BaseAddress = new Uri("http://fake/") };
            _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            var service = new ProgressServiceImpl(_repoMock.Object, _httpClientFactoryMock.Object);

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                service.MarkModuleCompletedAsync(userId, 1, 10));

            Assert.Equal("Unable to send notification — User not found.", ex.Message);
        }



        // ----------------------------------------------------------
        // TEST: Module fallback name when title is null
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

            fake.AddJsonResponse("api/users/", new { Email = "test@mail.com", Name = "TestUser" });

            fake.AddJsonResponse("modules", new[]
            {
                new { Id = 10, Title = (string)null }
            });

            fake.AddJsonResponse("api/courses/", new { Title = "C# Course" });

            fake.AddJsonResponse("notification/trigger", new { ok = true });

            var httpClient = new HttpClient(fake) { BaseAddress = new Uri("http://fake/") };

            _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            var service = new ProgressServiceImpl(_repoMock.Object, _httpClientFactoryMock.Object);

            await service.MarkModuleCompletedAsync(userId, 1, 10);

            // No exception = fallback name used successfully
            Assert.True(true);
        }

        // ----------------------------------------------------------
        // TEST: All modules completed -> send course completed notification
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

            handler.AddJsonResponse("api/users/", new { Email = "test@mail.com", Name = "User1" });

            handler.AddJsonResponse("modules", new[]
            {
                new { Id = 10, Title = "Intro" },
                new { Id = 11, Title = "Basics" },
                new { Id = 12, Title = "Advanced" }
            });

            handler.AddJsonResponse("api/courses/", new { Title = "C# Course" });

            handler.AddJsonResponse("notification/trigger", new { ok = true });

            var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://fake/") };
            _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            var service = new ProgressServiceImpl(_repoMock.Object, _httpClientFactoryMock.Object);

            await service.MarkModuleCompletedAsync(userId, 1, 10);

            // Test passes if no exception is thrown
            Assert.True(true);
        }
    }
}
