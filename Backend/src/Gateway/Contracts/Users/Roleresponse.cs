using System.ComponentModel.DataAnnotations;

namespace Gateway.Contracts.Users
{
    public class Roleresponse
    {
        [Key]
        public int Id { get; set; }

        [StringLength(100)]
        public string Name { get; set; } = null!;

        [StringLength(255)]
        public string? Description { get; set; }
    }

}