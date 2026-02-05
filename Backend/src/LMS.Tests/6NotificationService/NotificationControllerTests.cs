using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using Moq;
using DTOs._6NotificationService;
using NotificationService.BLL.Interface;
using NotificationService.Web.Controllers;

namespace LMS.Tests.NotificationService
{
    public class NotificationControllerTests
    {
        private readonly Mock<INotificationService> _serviceMock;
        private readonly NotificationController _controller;
        private readonly Fixture _fixture;

        public NotificationControllerTests()
        {
            _serviceMock = new Mock<INotificationService>();
            _controller = new NotificationController(_serviceMock.Object);

            _fixture = new Fixture();
            _fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        // ---------------- Direct Email ----------------
        [Fact]
        public async Task Send_ShouldInvokeService_AndReturnOk()
        {
            var req = _fixture.Build<EmailRequestDTO>()
                .With(x => x.Subject, "Hello")
                .With(x => x.Body, "Body")
                .Create();

            var result = await _controller.Send(req) as OkObjectResult;

            _serviceMock.Verify(s =>
                s.SendEmailAsync(req.UserId, req.Subject, req.Body),
                Times.Once);

            Assert.NotNull(result);
            Assert.Equal("Email sent successfully.", result!.Value);
        }

        // ---------------- Template Email ----------------
        [Fact]
        public async Task SendTemplate_ShouldInvokeService_AndReturnOk()
        {
            var req = _fixture.Build<TemplateRequestDTO>()
                .With(t => t.TemplateName, "Welcome")
                .With(t => t.Model, new { Name = "Kira" })
                .Create();

            var result = await _controller.SendTemplate(req) as OkObjectResult;

            _serviceMock.Verify(s =>
                s.SendEmailByTemplateAsync(req.UserId, req.TemplateName, req.Model),
                Times.Once);

            Assert.NotNull(result);
            Assert.Equal("Template email sent successfully.", result!.Value);
        }

        // ---------------- User Notifications ----------------
        [Fact]
        public async Task GetUserNotifications_ShouldReturnNotifications()
        {
            var dto = _fixture.Create<UserIdRequestDto>();

            var notifications = _fixture.CreateMany<NotificationDto>(1).ToList();
            notifications[0].Title = "T1";

            _serviceMock
                .Setup(s => s.GetUserNotificationsAsync(dto.UserId))
                .ReturnsAsync(notifications);

            // Access the Result property of ActionResult<T>
            var actionResult = await _controller.GetUserNotifications(dto);
            var okResult = actionResult.Result as OkObjectResult;

            Assert.NotNull(okResult);
            var list = Assert.IsType<List<NotificationDto>>(okResult!.Value);
            Assert.Single(list);
            Assert.Equal("T1", list[0].Title);
        }

        // ---------------- Triggered Notification ----------------
        [Fact]
        public async Task TriggerNotification_ShouldCallService_AndReturnOk()
        {
            var dto = _fixture.Build<TriggerNotificationDto>()
                .With(x => x.Type, NotificationType.Enrollment)
                .With(x => x.Data, new Dictionary<string, string> { { "UserName", "Kira" } })
                .Create();

            var result = await _controller.TriggerNotification(dto) as OkObjectResult;

            _serviceMock.Verify(s =>
                s.HandleTriggeredNotificationAsync(dto),
                Times.Once);

            Assert.NotNull(result);
            Assert.Equal("Notification processed successfully.", result!.Value);
        }
    }
}