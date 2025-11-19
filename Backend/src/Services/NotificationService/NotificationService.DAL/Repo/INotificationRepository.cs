using NotificationService.DAL.Models;

namespace NotificationService.DAL.Repo
{
    public interface INotificationRepository
    {
        Task<NotificationTemplate?> GetTemplateByNameAsync(string name);
        Task SaveNotificationAsync(Notification notification);
        Task<List<Notification>> GetUserNotificationsAsync(Guid userId);
    }
}
