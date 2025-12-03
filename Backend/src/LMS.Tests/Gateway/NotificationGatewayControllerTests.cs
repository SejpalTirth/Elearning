using Gateway.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;

namespace LMS.Tests.Gateway
{
    public class NotificationGatewayControllerTests
    {
        private readonly Mock<IHttpClientFactory> _factoryMock;
        private readonly Mock<HttpMessageHandler> _handlerMock;
        private readonly HttpClient _client;

        public NotificationGatewayControllerTests()
        {
            _handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            _client = new HttpClient(_handlerMock.Object)
            {
                BaseAddress = new Uri("http://fake-notification-service/")
            };

            _factoryMock = new Mock<IHttpClientFactory>();
            _factoryMock.Setup(f => f.CreateClient("NotificationService"))
                        .Returns(_client);
        }

        private NotificationGatewayController CreateController()
        {
            var controller = new NotificationGatewayController(_factoryMock.Object);

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

        // -------------------------------------------------------------
        // /test
        // -------------------------------------------------------------
        [Fact]
        public async Task Test_ShouldCallCorrectUrl_AndReturnContent()
        {
            SetupJsonResponse("{\"ping\":true}");

            var controller = CreateController();

            var result = await controller.Test();
            var content = Assert.IsType<ContentResult>(result);

            Assert.Equal("{\"ping\":true}", content.Content);

            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().EndsWith("/api/notification/test")),
                ItExpr.IsAny<CancellationToken>());
        }

        // -------------------------------------------------------------
        // POST /send
        // -------------------------------------------------------------
        [Fact]
        public async Task Send_ShouldPostCorrectUrl()
        {
            SetupJsonResponse("{\"sent\":true}");

            var controller = CreateController();
            var dto = new { email = "test@mail.com" };

            var result = await controller.Send(dto);
            var content = Assert.IsType<ContentResult>(result);

            Assert.Equal("{\"sent\":true}", content.Content);

            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.ToString().EndsWith("/api/notification/send")),
                ItExpr.IsAny<CancellationToken>());
        }

        // -------------------------------------------------------------
        // POST /template/send
        // -------------------------------------------------------------
        [Fact]
        public async Task SendTemplate_ShouldPostCorrectUrl()
        {
            SetupJsonResponse("{\"templated\":true}");

            var controller = CreateController();
            var dto = new { template = "welcome" };

            var result = await controller.SendTemplate(dto);
            var content = Assert.IsType<ContentResult>(result);

            Assert.Equal("{\"templated\":true}", content.Content);

            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.ToString().EndsWith("/api/notification/template/send")),
                ItExpr.IsAny<CancellationToken>());
        }

        // -------------------------------------------------------------
        // POST /trigger
        // -------------------------------------------------------------
        [Fact]
        public async Task Trigger_ShouldPostCorrectUrl()
        {
            SetupJsonResponse("{\"triggered\":true}");

            var controller = CreateController();
            var dto = new { type = "Enrollment" };

            var result = await controller.Trigger(dto);
            var content = Assert.IsType<ContentResult>(result);

            Assert.Equal("{\"triggered\":true}", content.Content);

            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.ToString().EndsWith("/api/notification/trigger")),
                ItExpr.IsAny<CancellationToken>());
        }

        // -------------------------------------------------------------
        // GET /{userId}
        // -------------------------------------------------------------
        [Fact]
        public async Task GetUserNotifications_ShouldGetCorrectUrl()
        {
            SetupJsonResponse("[{\"title\":\"T1\"}]");

            var controller = CreateController();
            var userId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

            var result = await controller.GetUserNotifications(userId);
            var content = Assert.IsType<ContentResult>(result);

            Assert.Equal("[{\"title\":\"T1\"}]", content.Content);

            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().EndsWith("/api/notification/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")),
                ItExpr.IsAny<CancellationToken>());
        }
    }
}
