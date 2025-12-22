namespace CourseService.BLL.DTOs
{
    public class UpdateCourseRequestDto
    {
        public int CourseId { get; set; }
        public UpdateCourseDto Course { get; set; } = new();
    }
}
