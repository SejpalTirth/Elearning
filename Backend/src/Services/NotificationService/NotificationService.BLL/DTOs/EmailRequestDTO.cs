namespace NotificationService.BLL.DTOs
{
    public class EmailRequest
    {        public Guid UserId { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
