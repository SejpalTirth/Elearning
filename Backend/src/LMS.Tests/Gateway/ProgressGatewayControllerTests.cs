using AutoFixture;
using Gateway.Contracts.Progress;
using Gateway.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using static Gateway.Controllers.ProgressGatewayController;

namespace LMS.Tests.Gateway
{
    public class ProgressGatewayControllerTests
    {
        private readonly Fixture _fixture;
        private readonly Mock<IHttpClientFactory> _mockFactory;
        private readonly Mock<HttpMessageHandler> _mockHandler;
        private readonly ProgressGatewayController _controller;

        public ProgressGatewayControllerTests()
        {
            _fixture = new Fixture();
            _mockHandler = new Mock<HttpMessageHandler>();
            _mockFactory = new Mock<IHttpClientFactory>();

            // Setup the HttpClient with a BaseAddress to avoid ForwardPost internal crashes
            var client = new HttpClient(_mockHandler.Object)
            {
                BaseAddress = new Uri("http://progress-service/")
            };

            _mockFactory.Setup(_ => _.CreateClient("ProgressService")).Returns(client);

            _controller = new ProgressGatewayController(_mockFactory.Object);

            // Mock User context for [Authorize]
            var user = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity());
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        private void SetupMockResponse(HttpStatusCode code, object content)
        {
            var response = new HttpResponseMessage(code)
            {
                Content = JsonContent.Create(content)
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);
        }

        #region Happy Paths (Success)

        [Fact]
        public async Task GetUserProgress_ReturnsOk_WhenSuccessful()
        {
            // Arrange
            var expectedData = _fixture.CreateMany<ProgressRecordDto>(3).ToList();
            SetupMockResponse(HttpStatusCode.OK, expectedData);

            // Act
            var result = await _controller.GetUserProgress();

            // Assert
            var objectResult = Assert.IsAssignableFrom<ObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
        }

        [Fact]
        public async Task CompleteModule_ReturnsOk_WhenSuccessful()
        {
            // Arrange
            var payload = _fixture.Create<ModuleCompleteRequest>();
            SetupMockResponse(HttpStatusCode.OK, new { success = true });

            // Act
            var result = await _controller.CompleteModule(payload);

            // Assert
            var objectResult = Assert.IsAssignableFrom<ObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
        }

        #endregion

        #region Exception Paths (Catch Blocks - Crucial for Coverage)

        [Fact]
        public async Task GetUserProgress_Returns500_OnException()
        {
            // Arrange: Force an exception to trigger the 'catch' block
            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new Exception("Microservice unavailable"));

            // Act
            var result = await _controller.GetUserProgress();

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
            // Verify the specific error message from your controller
            Assert.Contains("Gateway error", objectResult.Value.ToString());
        }

        [Fact]
        public async Task CompleteModule_Returns500_OnException()
        {
            // Arrange
            var payload = _fixture.Create<ModuleCompleteRequest>();
            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            var result = await _controller.CompleteModule(payload);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
        }

        #endregion
    }
}