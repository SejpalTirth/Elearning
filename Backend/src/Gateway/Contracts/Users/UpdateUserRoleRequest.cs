using System.ComponentModel.DataAnnotations;

namespace Gateway.Contracts.Users
{
    public class UpdateUserRoleRequest
    {
        [Required(ErrorMessage = "UserId is required")]
        public Guid UserId { get; set; }
        [Range(1, 3, ErrorMessage = "RoleId must be greater than 0.")]
        public int RoleId { get; set; }
    }
}
