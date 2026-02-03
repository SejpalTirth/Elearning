using DTOs._6NotificationService;
using DTOs._6NotificationService;

namespace NotificationService.BLL.Interface
{
    public interface INotificationService
    {
        Task SendEmailAsync(Guid userId, string subject, string body);
        Task SendEmailByTemplateAsync(Guid userId, string templateName, object model);
        Task<List<NotificationDto>> GetUserNotificationsAsync(Guid userId);
        Task SendEmailDirectAsync(string email, string subject, string body);
        Task HandleTriggeredNotificationAsync(TriggerNotificationDto dto);
    }
}
