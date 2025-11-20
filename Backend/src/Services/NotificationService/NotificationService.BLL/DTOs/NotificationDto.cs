namespace NotificationService.BLL.Models
{
    public class NotificationDto
    {
        public string Title { get; set; }
        public string? Body { get; set; }
        public DateTime? SentAt { get; set; }
    }
}
