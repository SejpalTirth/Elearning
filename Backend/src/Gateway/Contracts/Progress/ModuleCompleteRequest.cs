using System.ComponentModel.DataAnnotations;

namespace Gateway.Contracts.Progress
{
    public class ModuleCompleteRequest
    {
        [Required(ErrorMessage = "UserId is required")]
        public Guid UserId { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "ModuleId must be greater than 0.")]
        public int ModuleId { get; set; }
    }
}
