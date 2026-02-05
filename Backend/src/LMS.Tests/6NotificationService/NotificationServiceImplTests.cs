using AutoFixture;
using Moq;
using NotificationService.BLL.Service;
using NotificationService.BLL.Interface;
using NotificationService.DAL.Repo;
using NotificationService.DAL.Models;
using DTOs._6NotificationService;

namespace LMS.Tests.NotificationService
{
    public class NotificationServiceImplTests
    {
        private readonly Mock<INotificationRepository> _repoMock;
        private readonly Mock<IEmailSender> _emailMock;
        private readonly NotificationServiceImpl _service;

        private readonly Fixture _fixture;

        public NotificationServiceImplTests()
        {
            _repoMock = new Mock<INotificationRepository>();
            _emailMock = new Mock<IEmailSender>();

            _service = new NotificationServiceImpl(_repoMock.Object, _emailMock.Object);

            _fixture = new Fixture();
            _fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        // --------------------------------------------------
        // SendEmailAsync
        // --------------------------------------------------
        [Fact]
        public async Task SendEmailAsync_ShouldSendEmail_AndSaveNotification()
        {
            Guid userId = Guid.NewGuid();
            string subject = "Hello";
            string body = "Body";

            await _service.SendEmailAsync(userId, subject, body);

            _emailMock.Verify(x =>
                x.SendAsync("user-email-placeholder", subject, body), Times.Once);

            _repoMock.Verify(x => x.SaveNotificationAsync(It.Is<Notification>(n =>
                n.UserId == userId &&
                n.Title == subject &&
                n.Body == body &&
                n.IsRead == false
            )), Times.Once);
        }

        // --------------------------------------------------
        // SendEmailByTemplateAsync
        // --------------------------------------------------
        [Fact]
        public async Task SendEmailByTemplateAsync_ShouldThrow_WhenTemplateNotFound()
        {
            _repoMock.Setup(r => r.GetTemplateByNameAsync("Test"))
                     .ReturnsAsync((NotificationTemplate?)null);

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

            _repoMock.Setup(r => r.GetTemplateByNameAsync("Welcome"))
                     .ReturnsAsync(template);

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

            var notification = _fixture.Build<Notification>()
                                       .With(n => n.Title, "A")
                                       .With(n => n.Body, "Body A")
                                       .With(n => n.SentAt, DateTime.UtcNow)
                                       .Create();

            _repoMock.Setup(r => r.GetUserNotificationsAsync(userId))
                     .ReturnsAsync(new List<Notification> { notification });

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
            var dto = _fixture.Build<TriggerNotificationDto>()
                              .With(d => d.Email, "kira@mail.com")
                              .With(d => d.Type, NotificationType.Enrollment)
                              .With(d => d.Data, new Dictionary<string, string>
                              {
                                  { "UserName", "Kira" },
                                  { "CourseName", "Math" }
                              })
                              .Create();

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

            _repoMock.Verify(r => r.SaveNotificationAsync(It.IsAny<Notification>()), Times.Once);
        }
    }
}
