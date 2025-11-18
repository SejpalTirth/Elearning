namespace AssessmentService.BLL.DTOs
{
    public class SubmitQuizDto
    {
        public int QuizId { get; set; }
        public Guid UserId { get; set; }
        public List<SubmitAnswerDto> Answers { get; set; } = new();
    }

    public class SubmitAnswerDto
    {
        public int QuestionId { get; set; }
        public int SelectedAnswerId { get; set; }
    }
}
