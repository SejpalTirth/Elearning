namespace DTOs._4AssessmentService
{
    public class QuizSummaryDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    public class QuizDetailDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int TotalMarks { get; set; }
        public List<QuestionDetailDto> Questions { get; set; } = new();
    }

    public class QuestionDetailDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public int Marks { get; set; }
        public List<AnswerOptionDto> Answers { get; set; } = new();
    }

    public class AnswerOptionDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
    }

    public class QuizForModuleDto
    {
        public int QuizId { get; set; }
        public int ModuleId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int TimeLimitMinutes { get; set; }
        public int TotalMarks { get; set; }
        public bool AlreadyPassed { get; set; }
        public List<QuestionDetailDto> Questions { get; set; } = new();
    }
}
