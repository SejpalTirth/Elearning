using AutoFixture;
using Microsoft.EntityFrameworkCore;
using NotificationService.DAL.Data;
using NotificationService.DAL.Models;
using NotificationService.DAL.Repo;

namespace LMS.Tests.NotificationService
{
    public class NotificationRepositoryTests
    {
        private readonly Fixture _fixture;

        public NotificationRepositoryTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        private NotificationDbContext GetDb()
        {
            var options = new DbContextOptionsBuilder<NotificationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new NotificationDbContext(options);
        }

        // -----------------------------------------
        // GetTemplateByNameAsync
        // -----------------------------------------
        [Fact]
        public async Task GetTemplateByNameAsync_ShouldReturnTemplate()
        {
            var db = GetDb();
            var repo = new NotificationRepository(db);

            var template = _fixture.Build<NotificationTemplate>()
                                   .With(x => x.Name, "Welcome")
                                   .With(x => x.Subject, "Hello")
                                   .With(x => x.BodyTemplate, "Body...")
                                   .Create();

            db.NotificationTemplates.Add(template);
            await db.SaveChangesAsync();

            var result = await repo.GetTemplateByNameAsync("Welcome");

            Assert.NotNull(result);
            Assert.Equal("Hello", result!.Subject);
        }

        // -----------------------------------------
        // SaveNotificationAsync
        // -----------------------------------------
        [Fact]
        public async Task SaveNotificationAsync_ShouldSaveNotification()
        {
            var db = GetDb();
            var repo = new NotificationRepository(db);

            var note = _fixture.Build<Notification>()
                               .With(n => n.SentAt, DateTime.UtcNow)
                               .Create();

            await repo.SaveNotificationAsync(note);

            Assert.Single(db.Notifications);
        }

        // -----------------------------------------
        // GetUserNotificationsAsync
        // -----------------------------------------
        [Fact]
        public async Task GetUserNotificationsAsync_ShouldReturnSortedNotifications()
        {
            var db = GetDb();
            var repo = new NotificationRepository(db);

            Guid userId = Guid.NewGuid();

            var oldNotif = _fixture.Build<Notification>()
                                   .With(n => n.UserId, userId)
                                   .With(n => n.Title, "Old")
                                   .With(n => n.SentAt, DateTime.UtcNow.AddHours(-1))
                                   .Create();

            var newNotif = _fixture.Build<Notification>()
                                   .With(n => n.UserId, userId)
                                   .With(n => n.Title, "New")
                                   .With(n => n.SentAt, DateTime.UtcNow)
                                   .Create();

            db.Notifications.AddRange(oldNotif, newNotif);
            await db.SaveChangesAsync();

            var result = await repo.GetUserNotificationsAsync(userId);

            Assert.Equal("New", result[0].Title);
            Assert.Equal("Old", result[1].Title);
        }
    }
}
