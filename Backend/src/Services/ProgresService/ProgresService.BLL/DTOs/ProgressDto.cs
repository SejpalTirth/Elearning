namespace ProgressService.BLL.Models
{
    public class ProgressDto
    {
        public int CourseId { get; set; }
        public int? ModuleId { get; set; }
        public bool IsCompleted { get; set; }
        public decimal ProgressPercent { get; set; }
    }
}
