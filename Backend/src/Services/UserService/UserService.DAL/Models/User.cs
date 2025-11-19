using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace UserService.DAL.Models;

[Index("Email", Name = "UQ__Users__A9D105346C2116B7", IsUnique = true)]
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

    [StringLength(50)]
    public string? Role { get; set; }

    public bool? IsActive { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    [ForeignKey("UserId")]
    [InverseProperty("Users")]
    public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();

    [ForeignKey("UserId")]
    [InverseProperty("Users")]
    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();

}
