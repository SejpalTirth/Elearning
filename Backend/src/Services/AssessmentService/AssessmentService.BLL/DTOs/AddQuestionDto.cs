using System.ComponentModel.DataAnnotations;

namespace AssessmentService.BLL.DTOs
{
    public class AddQuestionDto
    {
        [Range(1, int.MaxValue)]
        public int QuizId { get; set; }

        [Required]
        public CreateQuestionDto Question { get; set; } = null!;
    }
}
