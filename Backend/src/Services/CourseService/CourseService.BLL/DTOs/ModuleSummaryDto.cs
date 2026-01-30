namespace CourseService.BLL.DTOs
{
    public class ModuleSummaryDto
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public string? Content { get; set; }
    }
}
