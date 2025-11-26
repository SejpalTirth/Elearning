namespace AssessmentService.BLL.DTOs
{
    public class CreateQuestionDto
    {
        public string Text { get; set; } = null!;
        public int Marks { get; set; } = 1;
        public List<string> Options { get; set; } = new();
        public int CorrectAnswerIndex { get; set; }
    }
}
