using System.ComponentModel.DataAnnotations;

namespace Gateway.Contracts.Course
{
    public class Course
    {
        [Required(ErrorMessage = "Course Title is required")]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Course category is required")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Instructor's UserId is required")]
        public string InstructorUserId { get; set; } = string.Empty;

        [MinLength(1, ErrorMessage = "At least one module is required")]
        public List<Module> Modules { get; set; } = new();
    }

    public class Module
    {
        [Required(ErrorMessage = "Module Title is required")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Module Content is required")]
        public string Content { get; set; } = string.Empty;
    }
}
