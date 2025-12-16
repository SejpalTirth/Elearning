using System.ComponentModel.DataAnnotations;

namespace Gateway.Contracts.Users
{
    public class CompleteProfileRequest
    {
        [Required(ErrorMessage = "UserId is required")]
        public Guid UserId { get; set; }
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }= string.Empty;
        [Required(ErrorMessage = "Role is required")]
        public string Role { get; set; } = "Pending";
    }
}
