using System.ComponentModel.DataAnnotations;

namespace DTOs._4AssessmentService
{
    public class AddQuestionDto
    {
        [Range(1, int.MaxValue)]
        public int QuizId { get; set; }

        [Required]
        public CreateQuestionDto Question { get; set; } = null!;
    }
}
