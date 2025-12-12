using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UserService.DAL.Models;

public partial class User
{
    [Key]
    public Guid Id { get; set; }

    [StringLength(150)]
    public string Email { get; set; } = null!;

    [StringLength(100)]
    public string? Name { get; set; }

    [Column("SSOProvider")]
    [StringLength(50)]
    public string? Ssoprovider { get; set; }

    [Column("SSOProviderId")]
    [StringLength(200)]
    public string? SsoproviderId { get; set; }

    [StringLength(255)]
    public string? PasswordHash { get; set; }

    // SINGLE ROLE stored as string
    [StringLength(50)]
    public string Role { get; set; } = "Student";

    public bool? IsActive { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    // Refresh tokens
    [InverseProperty("User")]
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
