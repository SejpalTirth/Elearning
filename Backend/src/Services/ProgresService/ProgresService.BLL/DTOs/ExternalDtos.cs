namespace ProgresService.BLL.DTOs
{
    public class UserDto
    {
        public required string Email { get; set; }
        public required string Name { get; set; }
    }

    public class ModuleDto
    {
        public required string Title { get; set; }
    }

    public class CourseDto
    {
        public required string Title { get; set; }
    }
}
