namespace AssessmentService.BLL.DTOs
{
    public class QuestionDto
    {
        public int Id { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public List<AnswerDto> Answers { get; set; } = new();
    }
}
