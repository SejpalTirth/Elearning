namespace AssessmentService.BLL.DTOs
{
    public class CreateQuizDto
    {
        public int ModuleId { get; set; }
        public string Title { get; set; } = null!;
        public int TimeLimitMinutes { get; set; } = 10;
    }
}
