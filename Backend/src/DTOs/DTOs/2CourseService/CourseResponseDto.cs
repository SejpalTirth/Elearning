using System.ComponentModel.DataAnnotations;

namespace DTOs._2CourseService
{
    public class CourseResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int CategoryId { get; set; }
        public Guid InstructorId { get; set; }
        public string InstructorName { get; set; } = null!;
        [Required]
        public List<ModuleSummaryDto> Modules { get; set; } = new();

        public bool IsDraft { get; set; }
        public bool IsDeleted { get; set; }
    }
}
