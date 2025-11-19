namespace ProgressService.BLL.Models
{
    public class ProgressDto
    {
        public int CourseId { get; set; }
        public int? ModuleId { get; set; }
        public decimal ProgressPercent { get; set; }
        public DateTime? LastUpdated { get; set; }
    }
}
