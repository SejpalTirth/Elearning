namespace DTOs._6NotificationService
{
    public class TemplateRequestDTO
    {
        public Guid UserId { get; set; }
        public string TemplateName { get; set; }
        public object Model { get; set; }
    }
}
