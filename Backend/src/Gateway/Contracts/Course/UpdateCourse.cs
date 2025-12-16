using System.ComponentModel.DataAnnotations;

namespace Gateway.Contracts.Course
{
    public class UpdateCourse
    {
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int CategoryId { get; set; }

        public List<UpdateModule> Modules { get; set; } = new();
    }
    public class UpdateModule
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;
    }
}
