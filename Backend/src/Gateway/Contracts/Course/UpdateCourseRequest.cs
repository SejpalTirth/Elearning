namespace Gateway.Contracts.Course
{
    public class UpdateCourseRequest
    {
        public int CourseId { get; set; }
        public UpdateCourse Course { get; set; } = new();
    }

}
