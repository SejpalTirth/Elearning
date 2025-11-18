using System;
using System.Collections.Generic;

namespace AssessmentService.DAL.Models;

public partial class Questions
{
    public int Id { get; set; }

    public int QuizId { get; set; }

    public string QuestionText { get; set; } = null!;

    public string QuestionType { get; set; } = null!;

    public int Marks { get; set; }

    public virtual ICollection<Answers> Answers { get; set; } = new List<Answers>();

    public virtual Quizzes Quiz { get; set; } = null!;
}
