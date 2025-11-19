using Microsoft.EntityFrameworkCore;
using NotificationService.DAL.Data;
using NotificationService.DAL.Models;

namespace NotificationService.DAL.Repo
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly NotificationDbContext _context;

        public NotificationRepository(NotificationDbContext context)
        {
            _context = context;
        }

        public async Task<NotificationTemplate?> GetTemplateByNameAsync(string name)
        {
            return await _context.NotificationTemplates
                .FirstOrDefaultAsync(t => t.Name == name);
        }

        public async Task SaveNotificationAsync(Notification notification)
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(Guid userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.SentAt)
                .ToListAsync();
        }
    }
}
