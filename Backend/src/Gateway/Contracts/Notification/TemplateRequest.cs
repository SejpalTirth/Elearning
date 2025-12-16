using System.ComponentModel.DataAnnotations;

namespace Gateway.Contracts.Notification
{
    public class TemplateRequest
    {
        [Required(ErrorMessage = "UserId is required")]
        public Guid UserId { get; set; }
        public string TemplateName { get; set; }
        public object Model { get; set; }
    }
}
