using Microsoft.EntityFrameworkCore;
using NotificationService.DAL.Data;
using NotificationService.DAL.Models;
using NotificationService.DAL.Repo;

namespace LMS.Tests.NotificationService
{
    public class NotificationRepositoryTests
    {
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

            db.NotificationTemplates.Add(new NotificationTemplate
            {
                Name = "Welcome",
                Subject = "Hello",
                BodyTemplate = "Body..."
            });

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

            var note = new Notification
            {
                UserId = Guid.NewGuid(),
                Title = "Test",
                Body = "Message",
                SentAt = DateTime.UtcNow
            };

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

            db.Notifications.AddRange(
                new Notification { UserId = userId, Title = "Old", SentAt = DateTime.UtcNow.AddHours(-1) },
                new Notification { UserId = userId, Title = "New", SentAt = DateTime.UtcNow }
            );

            await db.SaveChangesAsync();

            var result = await repo.GetUserNotificationsAsync(userId);

            Assert.Equal("New", result[0].Title);
            Assert.Equal("Old", result[1].Title);
        }
    }
}
