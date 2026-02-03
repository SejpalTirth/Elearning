namespace DTOs._2CourseService
{
    public class UserInfoDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? Name { get; set; }
    }
}
