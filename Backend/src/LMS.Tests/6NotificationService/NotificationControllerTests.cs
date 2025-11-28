using Microsoft.AspNetCore.Mvc;
using Moq;
using NotificationService.BLL.DTOs;
using NotificationService.BLL.Interface;
using NotificationService.BLL.Models;
using NotificationService.Web.Controllers;

namespace LMS.Tests.NotificationService
{
    public class NotificationControllerTests
    {
        private readonly Mock<INotificationService> _serviceMock;
        private readonly NotificationController _controller;

        public NotificationControllerTests()
        {
            _serviceMock = new Mock<INotificationService>();
            _controller = new NotificationController(_serviceMock.Object);
        }

        // -----------------------------------------------------
        // SEND
        // -----------------------------------------------------
        [Fact]
        public async Task Send_ShouldInvokeService_AndReturnOk()
        {
            var req = new EmailRequest
            {
                UserId = Guid.NewGuid(),
                Subject = "Hello",
                Body = "Body"
            };

            var result = await _controller.Send(req) as OkObjectResult;

            _serviceMock.Verify(s =>
                s.SendEmailAsync(req.UserId, req.Subject, req.Body),
                Times.Once);

            Assert.NotNull(result);
            Assert.Equal("Sent", result!.Value);
        }

        // -----------------------------------------------------
        // SEND TEMPLATE
        // -----------------------------------------------------
        [Fact]
        public async Task SendTemplate_ShouldInvokeService_AndReturnOk()
        {
            var req = new TemplateRequest
            {
                UserId = Guid.NewGuid(),
                TemplateName = "Welcome",
                Model = new { Name = "Kira" }
            };

            var result = await _controller.SendTemplate(req) as OkObjectResult;

            _serviceMock.Verify(s =>
                s.SendEmailByTemplateAsync(req.UserId, req.TemplateName, req.Model),
                Times.Once);

            Assert.NotNull(result);
            Assert.Equal("Sent using template", result!.Value);
        }

        // -----------------------------------------------------
        // GET USER NOTIFICATIONS
        // -----------------------------------------------------
        [Fact]
        public async Task GetUserNotifications_ShouldReturnNotifications()
        {
            var userId = Guid.NewGuid();

            _serviceMock.Setup(s => s.GetUserNotificationsAsync(userId))
                .ReturnsAsync(new List<NotificationDto>
                {
                    new NotificationDto { Title = "T1", Body = "B1" }
                });

            var result = await _controller.GetUserNotifications(userId) as OkObjectResult;

            Assert.NotNull(result);
            var list = Assert.IsType<List<NotificationDto>>(result!.Value);
            Assert.Single(list);
            Assert.Equal("T1", list[0].Title);
        }

        // -----------------------------------------------------
        // TEST EMAIL ENDPOINT
        // -----------------------------------------------------
        [Fact]
        public async Task Test_ShouldSendEmailDirect_AndReturnOk()
        {
            var result = await _controller.Test() as OkObjectResult;

            _serviceMock.Verify(s =>
                s.SendEmailDirectAsync(
                    "tirths331@outlook.com",
                    "Test Email",
                    "This is a successful test email from Notification Service!"
                ),
                Times.Once);

            Assert.NotNull(result);
            Assert.Equal("Test email sent!", result!.Value);
        }

        // -----------------------------------------------------
        // TRIGGER NOTIFICATION
        // -----------------------------------------------------
        [Fact]
        public async Task TriggerNotification_ShouldCallService_AndReturnOk()
        {
            var dto = new TriggerNotificationDto
            {
                UserId = Guid.NewGuid(),
                Email = "test@mail.com",
                Type = NotificationType.Enrollment,
                Data = new Dictionary<string, string> { { "UserName", "Kira" } }
            };

            var result = await _controller.TriggerNotification(dto) as OkObjectResult;

            _serviceMock.Verify(s =>
                s.HandleTriggeredNotificationAsync(dto),
                Times.Once);

            Assert.NotNull(result);
            Assert.Equal("Notification processed", result!.Value);
        }
    }
}
