using System.ComponentModel.DataAnnotations;

namespace Gateway.Contracts.Assessment
{
    public class SubmitQuiz
    {
        [Range(1, int.MaxValue, ErrorMessage = "QuizId must be greater than 0.")]
        public int QuizId { get; set; }
        [Required(ErrorMessage = "UserId is required.")]
        public Guid UserId { get; set; }
        [MinLength(1, ErrorMessage = "At least one answer must be provided.")]
        public List<SubmitAnswer> Answers { get; set; } = new();
    }
    public class SubmitAnswer
    {
        public int QuestionId { get; set; }
        public int SelectedAnswerId { get; set; }
    }
}
