using NotificationService.BLL.Models;

namespace NotificationService.BLL.DTOs
{
    public class TriggerNotificationDto
    {
        public Guid UserId { get; set; }
        public NotificationType Type { get; set; }
        public required string Email { get; set; }
        public Dictionary<string, string> Data { get; set; } = new();
    }
}
