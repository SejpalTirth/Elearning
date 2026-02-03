
namespace DTOs._4AssessmentService
{
    public class AddQuestionResponseDto
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public int? QuestionId { get; set; }
    }

}
