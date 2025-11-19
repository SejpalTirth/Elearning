using System;
using System.Collections.Generic;

namespace AssessmentService.DAL.Models;

public partial class Quizzes
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public string Title { get; set; } = null!;

    public int? TotalMarks { get; set; }

    public int? TimeLimitMinutes { get; set; }

    public virtual ICollection<Questions> Questions { get; set; } = new List<Questions>();

    public virtual ICollection<QuizSubmissions> QuizSubmissions { get; set; } = new List<QuizSubmissions>();
}
