namespace Gateway.Contracts.Course
{
    public partial class Course
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int CategoryId { get; set; }
        public Guid? InstructorUserId { get; set; }
        public bool IsDeleted { get; set; } = false;
        public bool IsDraft { get; set; } = true;
        public virtual Category Category { get; set; } = null!;
        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public virtual ICollection<Module> Modules { get; set; } = new List<Module>();
    }
    public partial class Category
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
    }
    public partial class Enrollment
    {
        public int Id { get; set; }

        public int CourseId { get; set; }

        public string UserId { get; set; } = null!;

        public DateTime EnrolledAt { get; set; }

        public virtual Course Course { get; set; } = null!;
    }
    public partial class Module
    {
        public int Id { get; set; }

        public string Title { get; set; } = null!;

        public string Content { get; set; } = null!;

        public int CourseId { get; set; }

        public virtual Course Course { get; set; } = null!;
    }
}
