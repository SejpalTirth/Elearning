namespace CourseService.DAL.Models;

public partial class Enrollment
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public string UserId { get; set; } = null!;

    public DateTime EnrolledAt { get; set; }

    public virtual Course Course { get; set; } = null!;
}
