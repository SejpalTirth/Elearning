namespace DTOs._4AssessmentService
{
    public class CourseQuizStatusResponseDto
    {
        public int CourseId { get; set; }

        public bool AllQuizzesCreated { get; set; }

        public List<QuizModuleStatusDto> Modules { get; set; } = new();

        public int? NextPendingModuleId { get; set; }

        public string? Error { get; set; }
    }

}
