namespace Gateway.DTOs
{
    public class QuizDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public List<QuestionDto>? Questions { get; set; }
    }

    public class QuestionDto
    {
        public int Id { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public List<AnswerDto>? Answers { get; set; }
    }

    public class AnswerDto
    {
        public int Id { get; set; }
        public string AnswerText { get; set; } = string.Empty;
    }
}
