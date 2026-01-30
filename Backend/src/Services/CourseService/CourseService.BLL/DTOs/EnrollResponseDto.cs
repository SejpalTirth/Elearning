namespace CourseService.BLL.DTOs
{
    public class EnrollResponseDto
    {
        public string? Message { get; set; }
        public List<ModuleSummaryDto> Modules { get; set; } = new List<ModuleSummaryDto>();
    }
}
