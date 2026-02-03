namespace DTOs._4AssessmentService
{
    public class CreateQuizResponseDto
    {
        public bool AlreadyExists { get; set; }

        public int QuizId { get; set; }

        public string Title { get; set; } = string.Empty;

        public int ModuleId { get; set; }

        public string Message { get; set; } = string.Empty;
    }

}
