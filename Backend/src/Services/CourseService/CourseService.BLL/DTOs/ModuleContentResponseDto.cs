using System.ComponentModel.DataAnnotations;

namespace CourseService.BLL.DTOs
{
    public class ModuleContentResponseDto
    {
        public int Id { get; set; }
        [Required]
        public required string Title { get; set; }
        [Required]
        public required string Content { get; set; }

        // From AssessmentService
        public int QuizId { get; set; }
    }
}
