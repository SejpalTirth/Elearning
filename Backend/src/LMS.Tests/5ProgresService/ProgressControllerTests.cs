using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ProgressService.Web.Controllers;
using ProgressService.BLL.Interface;
using ProgresService.BLL.DTOs;
using ProgressService.BLL.Models;
using System.Net;
using System.Text;
using System.Text.Json;

namespace LMS.Tests.ProgressService
{
    public class ProgressControllerTests : BaseTest
    {
        private readonly Mock<IProgressService> _serviceMock;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly IFixture _fixture;

        public ProgressControllerTests()
        {
            _serviceMock = new Mock<IProgressService>();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();

            _fixture = new Fixture();

            // ---- FIX AUTO-FIXTURE RECURSION ----
            _fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));

            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        // -------------------------------------------------------------------
        // Fake handler for mocking external CourseService calls
        // -------------------------------------------------------------------
        private class FakeHttpHandler : HttpMessageHandler
        {
            public readonly Dictionary<string, object?> Responses = new();

            public void Add(string contains, object? response)
            {
                Responses[contains] = response;
            }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                string url = request.RequestUri!.AbsoluteUri;

                foreach (var kv in Responses)
                {
                    if (url.Contains(kv.Key))
                    {
                        string json = kv.Value == null
                            ? "null"
                            : JsonSerializer.Serialize(kv.Value);

                        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = new StringContent(json, Encoding.UTF8, "application/json")
                        });
                    }
                }

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
            }
        }

        private ProgressController CreateController(HttpClient? httpClient = null)
        {
            if (httpClient != null)
            {
                _httpClientFactoryMock
                    .Setup(f => f.CreateClient("CourseService"))
                    .Returns(httpClient);
            }

            return new ProgressController(_serviceMock.Object, _httpClientFactoryMock.Object);
        }

        // -------------------------------------------------------------------
        // TEST 1: GET /api/progress/{userId}
        // -------------------------------------------------------------------
        [Fact]
        public async Task GetUserProgress_ShouldReturnOkWithData()
        {
            var userId = Guid.NewGuid();
            var progressList = new List<ProgressDto>
            {
                new ProgressDto { CourseId = 1, ModuleId = 10, IsCompleted = true }
            };

            _serviceMock.Setup(s => s.GetUserProgressAsync(userId))
                        .ReturnsAsync(progressList);

            var controller = CreateController();

            var result = await controller.GetUserProgress(userId) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(progressList, result.Value);
        }

        // -------------------------------------------------------------------
        // TEST 2: CompleteModule → Request null
        // -------------------------------------------------------------------
        [Fact]
        public async Task CompleteModule_ShouldReturnBadRequest_WhenRequestIsNull()
        {
            var controller = CreateController();

            var result = await controller.CompleteModule(null);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        // -------------------------------------------------------------------
        // TEST 3: CourseService returns invalid JSON → BadRequest
        // -------------------------------------------------------------------
        [Fact]
        public async Task CompleteModule_ShouldReturnBadRequest_WhenCourseServiceReturnsInvalidJson()
        {
            var handler = new FakeHttpHandler();
            handler.Add("course-id", "{invalid json}");

            var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://fake/") };

            var controller = CreateController(httpClient);

            var req = new ModuleCompleteRequest
            {
                UserId = Guid.NewGuid(),
                ModuleId = 10
            };

            var result = await controller.CompleteModule(req) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
        }

        // -------------------------------------------------------------------
        // TEST 4: CourseService returns null → NotFound
        // -------------------------------------------------------------------
        [Fact]
        public async Task CompleteModule_ShouldReturnNotFound_WhenCourseServiceReturnsNull()
        {
            var handler = new FakeHttpHandler();
            handler.Add("course-id", null);

            var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://fake/") };

            var controller = CreateController(httpClient);

            var req = new ModuleCompleteRequest
            {
                UserId = Guid.NewGuid(),
                ModuleId = 99
            };

            var result = await controller.CompleteModule(req) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
        }

        // -------------------------------------------------------------------
        // TEST 5: Success → Should call service
        // -------------------------------------------------------------------
        [Fact]
        public async Task CompleteModule_ShouldCallService_AndReturnOk()
        {
            var response = new CourseIdResponse { CourseId = 5, ModuleId = 10 };

            var handler = new FakeHttpHandler();
            handler.Add("course-id", response);

            var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://fake/")
            };

            var controller = CreateController(httpClient);

            var req = new ModuleCompleteRequest
            {
                UserId = Guid.NewGuid(),
                ModuleId = 10
            };

            var result = await controller.CompleteModule(req) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            _serviceMock.Verify(s =>
                s.MarkModuleCompletedAsync(req.UserId, 5, 10),
                Times.Once);
        }
    }
}
