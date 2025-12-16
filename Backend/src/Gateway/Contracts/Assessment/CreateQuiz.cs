using System.ComponentModel.DataAnnotations;

namespace Gateway.Contracts.Assessment
{
    public class CreateQuiz
    {
        [Range(1,int.MaxValue, ErrorMessage = "ModuleId must be greater than 0.")]
        public int ModuleId { get; set; }
        [Required(ErrorMessage = "Quiz title is required.")]
        public string Title { get; set; } = null!;
        [Range(1, int.MaxValue, ErrorMessage = "TimeLimitMinutes must be greater than 0.")]
        public int TimeLimitMinutes { get; set; } = 10;
    }
}
