namespace NotificationService.BLL.DTOs
{
    public class EmailRequest
    {        
        public Guid UserId { get; set; }
        public required string Subject { get; set; }
        public required string Body { get; set; }
    }
}
