using System.ComponentModel.DataAnnotations;

namespace Gateway.Contracts.Assessment
{
    public class CreateQuestion
    {
        [Required(ErrorMessage = "Question is required")]
        public string Question { get; set; } = null!;

        [Range(1, int.MaxValue, ErrorMessage = "Marks should be greater than zero")]
        public int Marks { get; set; } = 1;

        [MinLength(4, ErrorMessage = "Exactly 4 options are required")]
        [MaxLength(4, ErrorMessage = "Exactly 4 options are required")]
        public List<string> Options { get; set; } = new();

        [Range(0, 3, ErrorMessage = "CorrectAnswerIndex must be a valid index of the options list")]
        public int CorrectAnswerIndex { get; set; }
    }
}
