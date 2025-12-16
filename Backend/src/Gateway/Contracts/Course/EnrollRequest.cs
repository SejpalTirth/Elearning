using System.ComponentModel.DataAnnotations;

namespace Gateway.Contracts.Course
{
    public class EnrollRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "CourseId must be greater than 0.")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "UserId is required.")]
        public Guid UserId { get; set; }
    }
}
