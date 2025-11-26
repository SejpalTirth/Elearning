namespace AssessmentService.BLL.DTOs
{
    public class QuizResultDto
    {
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalMarks { get; set; }
        public int ObtainedMarks { get; set; }
        public decimal Percentage { get; set; }
        public bool Passed { get; set; }
        public bool AlreadyPassed { get; set; }
        public string StatusMessage { get; set; } = string.Empty;
        public Guid SubmissionId { get; set; }

    }
}
