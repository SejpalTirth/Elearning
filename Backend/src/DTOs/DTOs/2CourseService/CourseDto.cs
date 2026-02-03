namespace DTOs._2CourseService
{
    public class ModuleDto
    {
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
    }

    public class CourseDto
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int CategoryId { get; set; }
        public string InstructorUserId { get; set; } = null!;
        public List<ModuleDto> Modules { get; set; } = new();
    }
}
