using System.ComponentModel.DataAnnotations;

namespace DTOs._2CourseService
{
    public class EnrolledCourseResponseDto
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; } = string.Empty;
    }
}
