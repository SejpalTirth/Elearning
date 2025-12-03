using Moq;
using NotificationService.BLL.Service;
using NotificationService.BLL.Interface;
using NotificationService.DAL.Repo;
using NotificationService.DAL.Models;
using NotificationService.BLL.DTOs;
using NotificationService.BLL.Models;

namespace LMS.Tests.NotificationService
{
    public class NotificationServiceImplTests
    {
        private readonly Mock<INotificationRepository> _repoMock;
        private readonly Mock<IEmailSender> _emailMock;

        private readonly NotificationServiceImpl _service;

        public NotificationServiceImplTests()
        {
            _repoMock = new Mock<INotificationRepository>();
            _emailMock = new Mock<IEmailSender>();

            _service = new NotificationServiceImpl(_repoMock.Object, _emailMock.Object);
        }

        // --------------------------------------------------
        // SendEmailAsync
        // --------------------------------------------------
        [Fact]
        public async Task SendEmailAsync_ShouldSendEmail_AndSaveNotification()
        {
            Guid userId = Guid.NewGuid();

            await _service.SendEmailAsync(userId, "Hello", "Body");

            _emailMock.Verify(x =>
                x.SendAsync("user-email-placeholder", "Hello", "Body"), Times.Once);

            _repoMock.Verify(x => x.SaveNotificationAsync(It.Is<Notification>(n =>
                n.UserId == userId &&
                n.Title == "Hello" &&
                n.Body == "Body" &&
                n.IsRead == false
            )), Times.Once);
        }

        // --------------------------------------------------
        // SendEmailByTemplateAsync
        // --------------------------------------------------
        [Fact]
        public async Task SendEmailByTemplateAsync_ShouldThrow_WhenTemplateNotFound()
        {
            _repoMock.Setup(r => r.GetTemplateByNameAsync("Test")).ReturnsAsync((NotificationTemplate?)null);

            await Assert.ThrowsAsync<Exception>(() =>
                _service.SendEmailByTemplateAsync(Guid.NewGuid(), "Test", new { Name = "Kira" }));
        }

        [Fact]
        public async Task SendEmailByTemplateAsync_ShouldApplyTemplate_AndSendEmail()
        {
            var template = new NotificationTemplate
            {
                Name = "Welcome",
                Subject = "Hello {{Name}}",
                BodyTemplate = "Welcome, {{Name}}!"
            };

            _repoMock.Setup(r => r.GetTemplateByNameAsync("Welcome")).ReturnsAsync(template);

            await _service.SendEmailByTemplateAsync(Guid.NewGuid(), "Welcome", new { Name = "Kira" });

            _emailMock.Verify(x => x.SendAsync(
                "user-email-placeholder",
                "Hello Kira",
                "Welcome, Kira!"
            ), Times.Once);
        }

        // --------------------------------------------------
        // GetUserNotificationsAsync
        // --------------------------------------------------
        [Fact]
        public async Task GetUserNotificationsAsync_ShouldReturnMappedDtos()
        {
            Guid userId = Guid.NewGuid();

            _repoMock.Setup(r => r.GetUserNotificationsAsync(userId))
                .ReturnsAsync(new List<Notification>
                {
                    new Notification { Title = "A", Body = "Body A", SentAt = DateTime.Today }
                });

            var result = await _service.GetUserNotificationsAsync(userId);

            Assert.Single(result);
            Assert.Equal("A", result[0].Title);
            Assert.Equal("Body A", result[0].Body);
        }

        // --------------------------------------------------
        // SendEmailDirectAsync
        // --------------------------------------------------
        [Fact]
        public async Task SendEmailDirectAsync_ShouldSendEmail_AndSaveNotification_WithNullUserId()
        {
            await _service.SendEmailDirectAsync("test@mail.com", "Subject", "Body");

            _emailMock.Verify(x => x.SendAsync("test@mail.com", "Subject", "Body"), Times.Once);

            _repoMock.Verify(x => x.SaveNotificationAsync(It.Is<Notification>(n =>
                n.UserId == null &&
                n.Title == "Subject" &&
                n.Body == "Body"
            )), Times.Once);
        }

        // --------------------------------------------------
        // HandleTriggeredNotificationAsync
        // --------------------------------------------------
        [Fact]
        public async Task HandleTriggeredNotificationAsync_ShouldThrow_WhenUnknownType()
        {
            var dto = new TriggerNotificationDto
            {
                Type = (NotificationType)999
            };

            await Assert.ThrowsAsync<Exception>(() =>
                _service.HandleTriggeredNotificationAsync(dto));
        }

        [Fact]
        public async Task HandleTriggeredNotificationAsync_ShouldSendUsingTemplate()
        {
            var dto = new TriggerNotificationDto
            {
                UserId = Guid.NewGuid(),
                Email = "kira@mail.com",
                Type = NotificationType.Enrollment,
                Data = new Dictionary<string, string>
                {
                    { "UserName", "Kira" },
                    { "CourseName", "Math" }
                }
            };

            var template = new NotificationTemplate
            {
                Name = "EnrollmentEmail",
                Subject = "Hello {{UserName}}",
                BodyTemplate = "Welcome to {{CourseName}}"
            };

            _repoMock.Setup(r => r.GetTemplateByNameAsync("EnrollmentEmail"))
                     .ReturnsAsync(template);

            await _service.HandleTriggeredNotificationAsync(dto);

            _emailMock.Verify(e =>
                e.SendAsync("kira@mail.com", "Hello Kira", "Welcome to Math"), Times.Once);

            _repoMock.Verify(r =>
                r.SaveNotificationAsync(It.IsAny<Notification>()), Times.Once);
        }
    }
}
