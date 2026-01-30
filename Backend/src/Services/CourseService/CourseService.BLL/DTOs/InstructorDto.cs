namespace CourseService.BLL.DTOs
{
    public class InstructorDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public string? ProfileImage { get; set; }
    }
}
