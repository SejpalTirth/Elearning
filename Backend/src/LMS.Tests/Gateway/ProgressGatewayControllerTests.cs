using Gateway.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;

namespace LMS.Tests.Gateway
{
    public class ProgressGatewayControllerTests
    {
        private readonly Mock<IHttpClientFactory> _factoryMock;
        private readonly Mock<HttpMessageHandler> _handlerMock;
        private readonly HttpClient _client;

        public ProgressGatewayControllerTests()
        {
            _handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            _client = new HttpClient(_handlerMock.Object)
            {
                BaseAddress = new Uri("http://fake-progress-service/")
            };

            _factoryMock = new Mock<IHttpClientFactory>();
            _factoryMock.Setup(f => f.CreateClient("ProgressService"))
                        .Returns(_client);
        }

        private ProgressGatewayController CreateController()
        {
            var controller = new ProgressGatewayController(_factoryMock.Object);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            return controller;
        }

        private void SetupJsonResponse(string json, HttpStatusCode code = HttpStatusCode.OK)
        {
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = code,
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                });
        }

        // ---------------------------------------------------------------
        // GET USER PROGRESS
        // ---------------------------------------------------------------
        [Fact]
        public async Task GetUserProgress_ShouldCallCorrectUrl_AndReturnContent()
        {
            var expectedJson = "{\"progress\":50}";
            SetupJsonResponse(expectedJson);

            var controller = CreateController();
            var userId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

            var result = await controller.GetUserProgress(userId);
            var content = Assert.IsType<ContentResult>(result);

            Assert.Equal(expectedJson, content.Content);

            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().EndsWith("/api/progress/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")),
                ItExpr.IsAny<CancellationToken>());
        }

        // ---------------------------------------------------------------
        // COMPLETE MODULE
        // ---------------------------------------------------------------
        [Fact]
        public async Task CompleteModule_ShouldPost_ToCorrectUrl_AndReturnResponse()
        {
            var expectedJson = "{\"success\":true}";
            SetupJsonResponse(expectedJson);

            var controller = CreateController();
            var payload = new { userId = "abc", moduleId = 7 };

            var result = await controller.CompleteModule(payload);
            var content = Assert.IsType<ContentResult>(result);

            Assert.Equal(expectedJson, content.Content);

            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.ToString().EndsWith("/api/progress/complete-module") &&
                    req.Content != null),
                ItExpr.IsAny<CancellationToken>());
        }
    }
}
