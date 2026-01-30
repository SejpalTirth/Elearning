namespace CourseService.BLL.DTOs
{
    public class TriggerNotificationDto
    {
        public Guid UserId { get; set; }
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
