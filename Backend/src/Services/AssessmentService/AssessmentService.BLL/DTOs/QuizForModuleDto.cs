namespace AssessmentService.BLL.DTOs
{
    public class QuizForModuleDto
    {
        public int QuizId { get; set; }
        public int ModuleId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int? TimeLimitMinutes { get; set; }

        public List<QuizQuestionDto> Questions { get; set; } = new();
    }

    public class QuizQuestionDto
    {
        public int QuestionId { get; set; }
        public string Text { get; set; } = string.Empty;
        public int Marks { get; set; }

        public List<QuizOptionDto> Options { get; set; } = new();
    }

    public class QuizOptionDto
    {
        public int AnswerId { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}
