namespace DTOs._6NotificationService
{
    public class EmailRequestDTO
    {        
        public Guid UserId { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
