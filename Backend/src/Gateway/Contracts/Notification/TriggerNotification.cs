using System.ComponentModel.DataAnnotations;

namespace Gateway.Contracts.Notification
{
    public class TriggerNotification
    {
        [Required(ErrorMessage = "UserId is required")]
        public Guid UserId { get; set; }
        [Range(0,2, ErrorMessage = "Invalid Notification Type")]
        public NotificationType Type { get; set; }
        public required string Email { get; set; }
        public Dictionary<string, string> Data { get; set; } = new();
    }
    public enum NotificationType
    {
        Enrollment = 0,
        ModuleCompleted = 1,
        CourseCompleted = 2
    }
}
