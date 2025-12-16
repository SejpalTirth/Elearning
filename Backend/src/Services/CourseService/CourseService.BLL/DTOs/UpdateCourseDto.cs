namespace CourseService.BLL.DTOs
{
    public class UpdateModuleDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    public class UpdateCourseDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public List<UpdateModuleDto> Modules { get; set; } = new();
    }
}
