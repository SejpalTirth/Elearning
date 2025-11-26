namespace AssessmentService.DAL.Models;

public partial class QuizSubmission
{
    public Guid Id { get; set; }

    public int QuizId { get; set; }

    public Guid UserId { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public int? Score { get; set; }

    public string? SubmittedData { get; set; }

    public virtual Quiz Quiz { get; set; } = null!;
}
