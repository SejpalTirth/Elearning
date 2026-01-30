namespace NotificationService.BLL.DTOs
{
    public class TemplateRequestDTO
    {
        public Guid UserId { get; set; }
        public required string TemplateName { get; set; }
        public object? Model { get; set; }
    }
}
