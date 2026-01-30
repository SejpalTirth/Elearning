using System.ComponentModel.DataAnnotations;

namespace Gateway.Contracts.Notification
{
    public class EmailRequest
    {
        [Required(ErrorMessage = "UserId is required")]
        public Guid UserId { get; set; }
        public required string Subject { get; set; }
        public required string Body { get; set; }
    }
}
