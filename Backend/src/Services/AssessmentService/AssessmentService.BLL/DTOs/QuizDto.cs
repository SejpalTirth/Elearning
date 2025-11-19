namespace AssessmentService.BLL.DTOs
{
    public class QuizDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public List<QuestionDto> Questions { get; set; } = new();
    }
}
