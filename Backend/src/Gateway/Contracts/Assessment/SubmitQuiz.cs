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
        [Range(1, int.MaxValue, ErrorMessage = "QuestionId must be greater than 0.")]
        public int QuestionId { get; set; }
        [Range(0,3, ErrorMessage = "SelectedAnswerId must be a valid index of the options list.")]
        public int SelectedAnswerId { get; set; }
    }
}
