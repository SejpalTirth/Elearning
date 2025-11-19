// Services/AuthService.cs
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

    // Access token lifetime
    private readonly TimeSpan _accessTokenLifetime = TimeSpan.FromMinutes(4);

    public AuthService(IUserRepository users, IRefreshTokenRepository refreshTokens, IConfiguration config)
    {
        _users = users;
        _refreshTokens = refreshTokens;
        _config = config;
    }

    // create or get user, then issue tokens
    public async Task<TokenResponseDto> SignInExternalAsync(string provider, string providerUserId, string email, string name)
    {
        var user = await _users.GetByEmailAsync(email);
        if (user == null)
        {
            user = new User
            {
                Email = email,
                Name = name,
                Ssoprovider = provider,
                SsoproviderId = providerUserId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            };
            user = await _users.AddUserAsync(user);
        }
        else
        {
            // update SSO fields if missing
            if (string.IsNullOrEmpty(user.Ssoprovider))
            {
                user.Ssoprovider = provider;
                user.SsoproviderId = providerUserId;
                user.UpdatedAt = DateTime.UtcNow;
                await _users.SaveChangesAsync();
            }
        }

        return await GenerateAndStoreTokensAsync(user);
    }

    // single-use refresh token flow:
    public async Task<TokenResponseDto?> RefreshTokenAsync(string refreshToken)
    {
        var tokenEntity = await _refreshTokens.GetByTokenAsync(refreshToken);
        if (tokenEntity == null) return null;
        if (tokenEntity.RevokedAt != null) return null;
        if (tokenEntity.ExpiresAt < DateTime.UtcNow) return null;

        // Revoke old refresh token (single-use)
        await _refreshTokens.RevokeTokenAsync(tokenEntity);

        // issue new access + refresh
        var user = tokenEntity.User;
        return await GenerateAndStoreTokensAsync(user);
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
        var expiresAt = DateTime.UtcNow.Add(_accessTokenLifetime);

        var refreshToken = GenerateSecureToken();

        var refreshEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7), // still set longer, but single-use enforced
            CreatedAt = DateTime.UtcNow,
            RevokedAt = null
        };

        await _refreshTokens.AddTokenAsync(refreshEntity);

        return new TokenResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt
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
            new Claim("role", user.Role ?? "User"),
            new Claim("provider", user.Ssoprovider ?? string.Empty)
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
