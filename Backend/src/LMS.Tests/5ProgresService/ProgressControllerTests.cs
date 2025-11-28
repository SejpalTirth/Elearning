using Microsoft.AspNetCore.Mvc;
using Moq;
using ProgressService.Web.Controllers;
using ProgressService.BLL.Interface;
using ProgresService.BLL.DTOs;
using ProgressService.BLL.Models;
using System.Net;
using System.Text.Json;

namespace ProgressService.Tests
{
    public class ProgressControllerTests
    {
        private readonly Mock<IProgressService> _serviceMock;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;

        public ProgressControllerTests()
        {
            _serviceMock = new Mock<IProgressService>();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        }

        // ------------------------------------------------------------
        // Fake HTTP Handler (improved version)
        // ------------------------------------------------------------
        public class FakeHttpMessageHandler : HttpMessageHandler
        {
            private readonly Dictionary<string, object?> _responses = new();

            public void AddJsonResponse(string urlContains, object? response)
            {
                _responses[urlContains] = response;
            }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var url = request.RequestUri!.AbsoluteUri;

                foreach (var kvp in _responses)
                {
                    if (url.Contains(kvp.Key))
                    {
                        string json = kvp.Value == null
                            ? "null"
                            : JsonSerializer.Serialize(kvp.Value);

                        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
                        });
                    }
                }

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
            }
        }

        // ------------------------------------------------------------
        // TEST 1: GET /api/progress/{userId}
        // ------------------------------------------------------------
        [Fact]
        public async Task GetUserProgress_ShouldReturnOkWithData()
        {
            var userId = Guid.NewGuid();

            var mockResult = new List<ProgressDto>
            {
                new ProgressDto { CourseId = 1, ModuleId = 10, IsCompleted = true }
            };

            _serviceMock.Setup(s => s.GetUserProgressAsync(userId))
                        .ReturnsAsync(mockResult);

            var controller = new ProgressController(_serviceMock.Object, _httpClientFactoryMock.Object);

            var result = await controller.GetUserProgress(userId) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(mockResult, result.Value);

            _serviceMock.Verify(s => s.GetUserProgressAsync(userId), Times.Once);
        }

        // ------------------------------------------------------------
        // TEST 2: POST /complete-module → Request null
        // ------------------------------------------------------------
        [Fact]
        public async Task CompleteModule_ShouldReturnBadRequest_WhenRequestIsNull()
        {
            var controller = new ProgressController(_serviceMock.Object, _httpClientFactoryMock.Object);

            var result = await controller.CompleteModule(null);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        // ------------------------------------------------------------
        // TEST 3: POST /complete-module → CourseService throws
        // ------------------------------------------------------------
        [Fact]
        public async Task CompleteModule_ShouldReturnBadRequest_WhenCourseServiceThrows()
        {
            var handler = new FakeHttpMessageHandler();

            // Simulate throws by using invalid JSON to force failure
            handler.AddJsonResponse("course-id", "{INVALID_JSON}");

            var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://fake/") };

            _httpClientFactoryMock.Setup(f => f.CreateClient("CourseService"))
                                  .Returns(httpClient);

            var request = new ModuleCompleteRequest
            {
                UserId = Guid.NewGuid(),
                ModuleId = 10
            };

            var controller = new ProgressController(_serviceMock.Object, _httpClientFactoryMock.Object);

            var result = await controller.CompleteModule(request) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
        }

        // ------------------------------------------------------------
        // TEST 4: CourseService returns null → NotFound
        // ------------------------------------------------------------
        [Fact]
        public async Task CompleteModule_ShouldReturnNotFound_WhenCourseServiceReturnsNull()
        {
            var handler = new FakeHttpMessageHandler();
            handler.AddJsonResponse("course-id", null); // returns "null"

            var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://fake/") };

            _httpClientFactoryMock.Setup(f => f.CreateClient("CourseService")).Returns(httpClient);

            var request = new ModuleCompleteRequest
            {
                UserId = Guid.NewGuid(),
                ModuleId = 99
            };

            var controller = new ProgressController(_serviceMock.Object, _httpClientFactoryMock.Object);

            var result = await controller.CompleteModule(request) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
        }

        // ------------------------------------------------------------
        // TEST 5: Success → Should call service with correct values
        // ------------------------------------------------------------
        [Fact]
        public async Task CompleteModule_ShouldCallService_AndReturnOk()
        {
            var handler = new FakeHttpMessageHandler();
            handler.AddJsonResponse("course-id", new CourseIdResponse
            {
                CourseId = 5,
                ModuleId = 10
            });

            var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://fake/") };

            _httpClientFactoryMock.Setup(f => f.CreateClient("CourseService"))
                                  .Returns(httpClient);

            var request = new ModuleCompleteRequest
            {
                UserId = Guid.NewGuid(),
                ModuleId = 10
            };

            var controller = new ProgressController(_serviceMock.Object, _httpClientFactoryMock.Object);

            var result = await controller.CompleteModule(request) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            _serviceMock.Verify(s =>
                s.MarkModuleCompletedAsync(request.UserId, 5, 10),
                Times.Once);
        }
    }
}
