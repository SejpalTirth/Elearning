namespace CourseService.BLL.DTOs
{
    public class InstructorDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }   // fetched from UserService
        public string? ProfileImage { get; set; }
    }
}
