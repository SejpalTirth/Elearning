namespace DTOs._2CourseService
{
    public class UpdateCourseRequestDto
    {
        public int CourseId { get; set; }
        public UpdateCourseDto Course { get; set; } = new();
    }
}
