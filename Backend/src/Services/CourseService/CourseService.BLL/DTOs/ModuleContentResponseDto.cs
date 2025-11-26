namespace CourseService.BLL.DTOs
{
    public class ModuleContentResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

        // From AssessmentService
        public int QuizId { get; set; }
    }
}
