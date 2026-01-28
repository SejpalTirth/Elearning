using GatewayService.BLL.DTOs;
using GatewayService.BLL.Interface;
using GatewayService.BLL.Security;
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
    private readonly TimeSpan _accessTokenLifetime;
    private readonly TimeSpan _refreshTokenLifetime;
    private readonly IPasswordDecryptor _decryptor;
    private readonly IPasswordHasher _hasher;

    public AuthService(
        IUserRepository users,
        IRefreshTokenRepository refreshTokens,
        IConfiguration config,
        IPasswordDecryptor decryptor,
        IPasswordHasher hasher)
    {
        _users = users;
        _refreshTokens = refreshTokens;
        _config = config;
        _decryptor = decryptor;
        _hasher = hasher;   

        _accessTokenLifetime = TimeSpan.FromMinutes(
            int.Parse(_config["Jwt:AccessTokenExpiryMinutes"] ?? "1")
        );

        _refreshTokenLifetime = TimeSpan.FromMinutes(
            int.Parse(_config["Jwt:RefreshTokenExpiryMinutes"] ?? "5")
        );
    }

    // EXTERNAL SIGN-IN
    public async Task<ExternalSignInResultDto> SignInExternalAsync(
        string provider,
        string providerUserId,
        string email,
        string name)
    {
        var adminEmail = _config["SpecialAccounts:AdminEmail"];

        var user = await _users.GetByEmailAsync(email);
        bool isNew = false;

        if (user == null)
        {
            isNew = true;

            user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                Name = null,
                Ssoprovider = provider,
                SsoproviderId = providerUserId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Role = email.Equals(adminEmail, StringComparison.OrdinalIgnoreCase)
                    ? "Admin"
                    : "Pending"
            };

            user = await _users.AddUserAsync(user);
        }

        if (isNew && user.Role != "Admin")
        {
            return new ExternalSignInResultDto
            {
                IsNewUser = true,
                UserId = user.Id,
                IsAdmin = false,
                Tokens = null
            };
        }

        var tokens = await GenerateAndStoreTokensAsync(user);

        return new ExternalSignInResultDto
        {
            IsNewUser = false,
            UserId = user.Id,
            IsAdmin = user.Role == "Admin",
            Tokens = tokens
        };
    }

    // TOKEN GENERATION
    private async Task<TokenResponseDto> GenerateAndStoreTokensAsync(User user)
    {
        var accessToken = CreateEncryptedJwt(user);
        var refreshToken = GenerateSecureToken();

        var refreshEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(_refreshTokenLifetime)
        };

        await _refreshTokens.AddTokenAsync(refreshEntity);

        return new TokenResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.Add(_accessTokenLifetime)
        };
    }

    // REFRESH TOKEN
    public async Task<TokenResponseDto?> RefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return null;

        var tokenEntity = await _refreshTokens.GetByTokenAsync(refreshToken);

        if (tokenEntity == null ||
            tokenEntity.RevokedAt != null ||
            tokenEntity.ExpiresAt < DateTime.UtcNow)
            return null;

        var user = tokenEntity.User ?? await _users.GetByIdAsync(tokenEntity.UserId);
        if (user == null)
            return null;

        var newAccessToken = CreateEncryptedJwt(user);

        return new TokenResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = tokenEntity.Token,
            ExpiresAt = tokenEntity.ExpiresAt
        };
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken)
    {
        var tokenEntity = await _refreshTokens.GetByTokenAsync(refreshToken);
        if (tokenEntity == null) return;

        await _refreshTokens.RevokeTokenAsync(tokenEntity);
    }

    // JWE CREATION
    private string CreateEncryptedJwt(User user)
    {
        var issuer = _config["Jwt:Issuer"];
        var audience = _config["Jwt:Audience"];

        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)
        );

        var rawEncryptionKey = Encoding.UTF8.GetBytes(
            _config["Jwt:EncryptionKey"]!
        );

        var derivedEncryptionKey = SHA256.HashData(rawEncryptionKey);

        var encryptionKey = new SymmetricSecurityKey(derivedEncryptionKey);


        var claims = new ClaimsIdentity(new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("name", user.Name ?? string.Empty),
            new Claim("role", user.Role ?? "Pending"),
            new Claim("provider", user.Ssoprovider ?? "Unknown")
        });

        var signingCredentials = new SigningCredentials(
            signingKey,
            SecurityAlgorithms.HmacSha256
        );

        var encryptingCredentials = new EncryptingCredentials(
            encryptionKey,
            SecurityAlgorithms.Aes256KW,
            SecurityAlgorithms.Aes256CbcHmacSha512
        );

        var handler = new JwtSecurityTokenHandler();

        var token = handler.CreateJwtSecurityToken(
            issuer: issuer,
            audience: audience,
            subject: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.Add(_accessTokenLifetime),
            issuedAt: DateTime.UtcNow,
            signingCredentials: signingCredentials,
            encryptingCredentials: encryptingCredentials,
            claimCollection: null
        );

        return handler.WriteToken(token);
    }

    // UTIL
    private static string GenerateSecureToken(int size = 64)
    {
        var bytes = new byte[size];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    public async Task<Guid> RegisterLocalAsync(
    string email,
    string encryptedPassword)
    {
        var existing = await _users.GetByEmailAsync(email);
        if (existing != null)
            throw new InvalidOperationException("User already exists");

        var decrypted = _decryptor.Decrypt(encryptedPassword);
        var hash = _hasher.Hash(decrypted);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = hash,
            Role = "Pending",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _users.AddUserAsync(user);

        return user.Id;
    }


    public async Task<TokenResponseDto?> LoginLocalAsync(
    string email,
    string encryptedPassword)
    {
        var user = await _users.GetByEmailAsync(email);
        if (user == null || user.PasswordHash == null)
            return null;

        var decrypted = _decryptor.Decrypt(encryptedPassword);
        Console.WriteLine(decrypted);

        if (!_hasher.Verify(decrypted, user.PasswordHash))
            return null;

        return await GenerateAndStoreTokensAsync(user);
    }

}
