namespace AssessmentService.DAL.Models;

public partial class Quiz
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public int? TotalMarks { get; set; }

    public int? TimeLimitMinutes { get; set; }

    public int? ModuleId { get; set; }

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    public virtual ICollection<QuizSubmission> QuizSubmissions { get; set; } = new List<QuizSubmission>();
}
