using AutoFixture;
using Gateway.Contracts.Progress;
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
        private readonly Fixture _fixture;
        private readonly Mock<IHttpClientFactory> _factoryMock;
        private readonly Mock<HttpMessageHandler> _handlerMock;
        private readonly HttpClient _client;

        public ProgressGatewayControllerTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

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
            return new ProgressGatewayController(_factoryMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        // ---------------------------------------------------------------------
        // Response helper
        // ---------------------------------------------------------------------
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

        // ---------------------------------------------------------------------
        // GET USER PROGRESS
        // ---------------------------------------------------------------------
        [Fact]
        public async Task GetUserProgress_ShouldCallCorrectUrl_AndReturnContent()
        {
            var expectedJson = "{\"progress\":50}";
            SetupJsonResponse(expectedJson);

            var controller = CreateController();
            var userId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

            // Update the test to pass the userId as part of the request body
            var result = await controller.GetUserProgress();

            var content = Assert.IsType<ContentResult>(result);
            Assert.Equal(expectedJson, content.Content);

            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.ToString()
                        .EndsWith("/api/progress/user") &&
                    req.Content != null), // Ensure that content is sent
                ItExpr.IsAny<CancellationToken>());
        }
        // ---------------------------------------------------------------------
        // COMPLETE MODULE
        // ---------------------------------------------------------------------
        [Fact]
        public async Task CompleteModule_ShouldPost_ToCorrectUrl_AndReturnResponse()
        {
            var expectedJson = "{\"success\":true}";
            SetupJsonResponse(expectedJson);

            var controller = CreateController();

            var payload = new ModuleCompleteRequest
            {
                ModuleId = _fixture.Create<int>()
            };

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
