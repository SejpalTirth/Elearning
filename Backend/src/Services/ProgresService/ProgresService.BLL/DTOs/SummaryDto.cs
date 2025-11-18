namespace ProgressService.BLL.Models
{
    public class SummaryDto
    {
        public int TotalCourses { get; set; }
        public int CompletedCourses { get; set; }
        public decimal AverageProgress { get; set; }
    }
}
