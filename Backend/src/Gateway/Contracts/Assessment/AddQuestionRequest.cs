using System.ComponentModel.DataAnnotations;

namespace Gateway.Contracts.Assessment
{
    public class AddQuestionRequest
    {
        [Range(1, int.MaxValue, ErrorMessage ="The quiz id should be a positive integer.")]
        public int QuizId { get; set; }

        [Required(ErrorMessage ="Please enter the qestion.")]
        public CreateQuestion Question { get; set; } = null!;
    }
}
