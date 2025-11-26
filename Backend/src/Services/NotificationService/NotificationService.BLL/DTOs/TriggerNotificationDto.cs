using NotificationService.BLL.Models;

namespace NotificationService.BLL.DTOs
{
    public class TriggerNotificationDto
    {
        public Guid UserId { get; set; }
        public NotificationType Type { get; set; }
        public string Email { get; set; } // required for SendAsync
        public Dictionary<string, string> Data { get; set; } = new();
    }
}
