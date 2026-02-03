using DTOs._6NotificationService;

namespace DTOs._6NotificationService
{
    public class TriggerNotificationDto
    {
        public Guid UserId { get; set; }
        public NotificationType Type { get; set; }
        public string Email { get; set; }
        public Dictionary<string, string> Data { get; set; } = new();
    }
}
