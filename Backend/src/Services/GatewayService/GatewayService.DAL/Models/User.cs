using System;
using System.Collections.Generic;

namespace GatewayService.DAL.Models;

public partial class User
{
    public Guid Id { get; set; }

    public string Email { get; set; } = null!;

    public string? Name { get; set; }

    public string? Ssoprovider { get; set; }

    public string? SsoproviderId { get; set; }

    public string? PasswordHash { get; set; }

    public string? Role { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
