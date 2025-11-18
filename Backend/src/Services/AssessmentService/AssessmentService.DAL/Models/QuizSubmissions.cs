using System;
using System.Collections.Generic;

namespace AssessmentService.DAL.Models;

public partial class QuizSubmissions
{
    public Guid Id { get; set; }

    public int QuizId { get; set; }

    public Guid UserId { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public int? Score { get; set; }

    public string? SubmittedData { get; set; }

    public virtual Quizzes Quiz { get; set; } = null!;
}
