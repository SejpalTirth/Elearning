namespace CourseService.BLL.DTOs
{
    public class CourseResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int CategoryId { get; set; }

        public Guid InstructorId { get; set; }
        public string InstructorName { get; set; }

        public List<ModuleSummaryDto> Modules { get; set; }
    }
}
