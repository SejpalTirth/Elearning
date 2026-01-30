using AutoFixture;
using Gateway.Controllers;
using Gateway.Contracts.Notification;
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

        // ------------------- SEND EMAIL -------------------
        [Fact]
        public async Task Send_ShouldPostCorrectUrl()
        {
            SetupJsonResponse("{\"sent\":true}");

            var controller = CreateController();
            var dto = new EmailRequest
            {
                UserId = Guid.NewGuid(),
                Subject = "Hello",
                Body = "Test email body"
            };

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

        // ------------------- SEND TEMPLATE EMAIL -------------------
        [Fact]
        public async Task SendTemplate_ShouldPostCorrectUrl()
        {
            SetupJsonResponse("{\"templated\":true}");

            var controller = CreateController();
            var dto = new TemplateRequest
            {
                UserId = Guid.NewGuid(),
                TemplateName = "welcome",
                Model = new { Name = "John" }
            };

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

        // ------------------- TRIGGER NOTIFICATION -------------------
        [Fact]
        public async Task Trigger_ShouldPostCorrectUrl()
        {
            SetupJsonResponse("{\"triggered\":true}");

            var controller = CreateController();
            var dto = new TriggerNotification
            {
                UserId = Guid.NewGuid(),
                Type = NotificationType.Enrollment,
                Email = "user@mail.com",
                Data = new Dictionary<string, string> { { "Course", "Math" } }
            };

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

        // ------------------- GET USER NOTIFICATIONS -------------------
        [Fact]
        public async Task GetUserNotifications_ShouldPostCorrectUrl()
        {
            SetupJsonResponse("[{\"title\":\"T1\"}]");

            var controller = CreateController();
            var dto = new UserIdRequest
            {
                UserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")
            };

            var result = await controller.GetUserNotifications(dto);
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
    }
}
