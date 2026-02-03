namespace DTOs._5ProgresService
{
    public class ProgresDto
    {
        public int CourseId { get; set; }
        public int? ModuleId { get; set; }
        public bool IsCompleted { get; set; }
        public decimal ProgresPercent { get; set; }
    }
}
