using GatewayService.BLL.DTOs;
using GatewayService.BLL.Interface;
using GatewayService.DAL.Models;
using GatewayService.DAL.Repo;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IConfiguration _config;

    public AuthService(IUserRepository users, IRefreshTokenRepository refreshTokens, IConfiguration config)
    {
        _users = users;
        _refreshTokens = refreshTokens;
        _config = config;
    }

    private readonly TimeSpan _accessTokenLifetime = TimeSpan.FromMinutes(4);

    public async Task<ExternalSignInResultDto> SignInExternalAsync(string provider, string providerUserId, string email, string name)
    {
        var adminEmail = _config["SpecialAccounts:AdminEmail"] ?? "tirths331@gmail.com";

        var user = await _users.GetByEmailAsync(email);
        bool isNew = false;

        if (user == null)
        {
            isNew = true;

            user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                Name = null,                       // Name will be completed later
                Ssoprovider = provider,
                SsoproviderId = providerUserId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Admin assignment
            if (email.Equals(adminEmail, StringComparison.OrdinalIgnoreCase))
            {
                user.Role = "Admin";
            }

            user = await _users.AddUserAsync(user);
        }
        else
        {
            // Update missing SSO fields
            var changed = false;

            if (string.IsNullOrEmpty(user.Ssoprovider))
            {
                user.Ssoprovider = provider;
                changed = true;
            }

            if (string.IsNullOrEmpty(user.SsoproviderId))
            {
                user.SsoproviderId = providerUserId;
                changed = true;
            }

            if (changed)
            {
                user.UpdatedAt = DateTime.UtcNow;
                await _users.SaveChangesAsync();
            }
        }

        // NEW USER — ask for name + role (unless admin)
        if (isNew && !email.Equals(adminEmail, StringComparison.OrdinalIgnoreCase))
        {
            return new ExternalSignInResultDto
            {
                IsNewUser = true,
                UserId = user.Id,
                IsAdmin = false,
                Tokens = null
            };
        }

        // EXISTING USER or ADMIN — issue tokens immediately
        var tokens = await GenerateAndStoreTokensAsync(user);

        return new ExternalSignInResultDto
        {
            IsNewUser = false,
            UserId = user.Id,
            IsAdmin = user.Role == "Admin",
            Tokens = tokens
        };
    }

    public async Task<TokenResponseDto?> RefreshTokenAsync(string refreshToken)
    {
        var tokenEntity = await _refreshTokens.GetByTokenAsync(refreshToken);
        if (tokenEntity == null) return null;
        if (tokenEntity.RevokedAt != null) return null;
        if (tokenEntity.ExpiresAt < DateTime.UtcNow) return null;

        await _refreshTokens.RevokeTokenAsync(tokenEntity);
        return await GenerateAndStoreTokensAsync(tokenEntity.User);
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken)
    {
        var tokenEntity = await _refreshTokens.GetByTokenAsync(refreshToken);
        if (tokenEntity == null) return;

        await _refreshTokens.RevokeTokenAsync(tokenEntity);
    }

    private async Task<TokenResponseDto> GenerateAndStoreTokensAsync(User user)
    {
        var accessToken = CreateJwtToken(user);
        var refreshToken = GenerateSecureToken();

        var refreshEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        await _refreshTokens.AddTokenAsync(refreshEntity);

        return new TokenResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(4)
        };
    }

    private string CreateJwtToken(User user)
    {
        var key = _config["Jwt:Key"];
        var issuer = _config["Jwt:Issuer"];
        var audience = _config["Jwt:Audience"];

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("name", user.Name ?? string.Empty),
            new Claim("role", user.Role ?? "Pending"),
            new Claim("provider", user.Ssoprovider ?? "Unknown")
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(4),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateSecureToken(int size = 64)
    {
        var bytes = new byte[size];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}