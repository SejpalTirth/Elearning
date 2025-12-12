namespace CourseService.BLL.DTOs
{
    public class CourseQuizStatusDto
    {
        public int CourseId { get; set; }
        public bool AllQuizzesCreated { get; set; }
        public List<object>? Modules { get; set; }
        public int? NextPendingModuleId { get; set; }
    }
}
