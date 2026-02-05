using AutoFixture;
using Gateway.Contracts.Notification;
using Gateway.Contracts.Users;
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
        private readonly Fixture _fixture;
        private readonly Mock<IHttpClientFactory> _factoryMock;
        private readonly Mock<HttpMessageHandler> _handlerMock;
        private readonly HttpClient _client;

        public NotificationGatewayControllerTests()
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
                BaseAddress = new Uri("http://fake-notification-service/")
            };

            _factoryMock = new Mock<IHttpClientFactory>();
            _factoryMock.Setup(f => f.CreateClient("NotificationService"))
                        .Returns(_client);
        }

        private NotificationGatewayController CreateController()
        {
            return new NotificationGatewayController(_factoryMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        // ---------------------------------------------------------
        // Response Setup Helpers
        // ---------------------------------------------------------
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

        // ---------------------------------------------------------
        // POST /send (EmailRequest)
        // ---------------------------------------------------------
        [Fact]
        public async Task Send_ShouldPostCorrectUrl_WhenValidRequest()
        {
            var emailRequest = _fixture.Create<EmailRequest>();
            SetupJsonResponse("{\"sent\":true}");

            var controller = CreateController();
            var result = await controller.Send(emailRequest);
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

        [Fact]
        public async Task Send_ShouldReturn500_WhenRequestFails()
        {
            var emailRequest = _fixture.Create<EmailRequest>();
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exception("Notification service error"));

            var controller = CreateController();
            var result = await controller.Send(emailRequest);
            var statusCodeResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Contains("Gateway error", statusCodeResult.Value.ToString());
        }

        // ---------------------------------------------------------
        // POST /template/send (TemplateRequest)
        // ---------------------------------------------------------
        [Fact]
        public async Task SendTemplate_ShouldPostCorrectUrl_WhenValidRequest()
        {
            var templateRequest = _fixture.Create<TemplateRequest>();
            SetupJsonResponse("{\"templated\":true}");

            var controller = CreateController();
            var result = await controller.SendTemplate(templateRequest);
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

        [Fact]
        public async Task SendTemplate_ShouldReturn500_WhenRequestFails()
        {
            var templateRequest = _fixture.Create<TemplateRequest>();
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exception("Template notification service error"));

            var controller = CreateController();
            var result = await controller.SendTemplate(templateRequest);
            var statusCodeResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Contains("Gateway error", statusCodeResult.Value.ToString());
        }

        // ---------------------------------------------------------
        // POST /trigger (TriggerNotification)
        // ---------------------------------------------------------
        [Fact]
        public async Task Trigger_ShouldPostCorrectUrl_WhenValidRequest()
        {
            var triggerNotification = _fixture.Create<TriggerNotification>();
            SetupJsonResponse("{\"triggered\":true}");

            var controller = CreateController();
            var result = await controller.Trigger(triggerNotification);
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

        [Fact]
        public async Task Trigger_ShouldReturn500_WhenRequestFails()
        {
            var triggerNotification = _fixture.Create<TriggerNotification>();
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exception("Trigger notification service error"));

            var controller = CreateController();
            var result = await controller.Trigger(triggerNotification);
            var statusCodeResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Contains("Gateway error", statusCodeResult.Value.ToString());
        }

        // ---------------------------------------------------------
        // POST /user (GetUserNotifications)
        // ---------------------------------------------------------
        [Fact]
        public async Task GetUserNotifications_ShouldReturnNotifications_WhenValidRequest()
        {
            var userIdRequest = _fixture.Create<UserIdRequest>();
            SetupJsonResponse("[{\"title\":\"T1\"}]");

            var controller = CreateController();
            var result = await controller.GetUserNotifications(userIdRequest);
            var content = Assert.IsType<ContentResult>(result);

            Assert.Equal("[{\"title\":\"T1\"}]", content.Content);

            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.ToString().EndsWith("/api/notification/user")),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task GetUserNotifications_ShouldReturn500_WhenRequestFails()
        {
            var userIdRequest = _fixture.Create<UserIdRequest>();
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exception("Get user notifications error"));

            var controller = CreateController();
            var result = await controller.GetUserNotifications(userIdRequest);
            var statusCodeResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Contains("Gateway error", statusCodeResult.Value.ToString());
        }

        // ---------------------------------------------------------
        // Edge Cases (Invalid Request / Bad Data)
        // ---------------------------------------------------------
        [Fact]
        public async Task Send_ShouldReturn400_WhenInvalidRequest()
        {
            var invalidEmailRequest = new EmailRequest(); // Missing UserId (Required)
            var controller = CreateController();

            var result = await controller.Send(invalidEmailRequest);
            var statusCodeResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(400, statusCodeResult.StatusCode);
            Assert.Contains("UserId is required", statusCodeResult.Value.ToString());
        }

        [Fact]
        public async Task Trigger_ShouldReturn400_WhenInvalidType()
        {
            var invalidTriggerNotification = _fixture.Create<TriggerNotification>();
            invalidTriggerNotification.Type = (NotificationType)999;

            var controller = CreateController();
            var result = await controller.Trigger(invalidTriggerNotification);
            var statusCodeResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(400, statusCodeResult.StatusCode);
            Assert.Contains("Invalid Notification Type", statusCodeResult.Value.ToString());
        }
    }
}
