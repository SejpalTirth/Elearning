namespace CourseService.BLL.DTOs
{
    public class ModuleDto
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    public class CourseDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string InstructorUserId { get; set; } = string.Empty;
        public List<ModuleDto> Modules { get; set; } = new();
    }
}
