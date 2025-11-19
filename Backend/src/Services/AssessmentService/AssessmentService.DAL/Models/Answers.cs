using System;
using System.Collections.Generic;

namespace AssessmentService.DAL.Models;

public partial class Answers
{
    public int Id { get; set; }

    public int QuestionId { get; set; }

    public string AnswerText { get; set; } = null!;

    public bool IsCorrect { get; set; }

    public virtual Questions Question { get; set; } = null!;
}
