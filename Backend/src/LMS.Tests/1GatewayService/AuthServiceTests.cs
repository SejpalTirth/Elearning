using GatewayService.DAL.Models;
using GatewayService.DAL.Repo;
using Microsoft.Extensions.Configuration;
using Moq;

namespace LMS.Tests.GatewayService
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepo;
        private readonly Mock<IRefreshTokenRepository> _tokenRepo;
        private readonly Mock<IConfiguration> _config;
        private readonly AuthService _service;

        public AuthServiceTests()
        {
            _userRepo = new Mock<IUserRepository>();
            _tokenRepo = new Mock<IRefreshTokenRepository>();
            _config = new Mock<IConfiguration>();

            // JWT config
            _config.Setup(c => c["Jwt:Key"]).Returns("ThisIsASuperSecretKeyForJwtToken12345");
            _config.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
            _config.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");

            // Admin email
            _config.Setup(c => c["SpecialAccounts:AdminEmail"])
                   .Returns("tirths331@gmail.com");

            _service = new AuthService(_userRepo.Object, _tokenRepo.Object, _config.Object);
        }

        private User CreateUser()
        {
            return new User
            {
                Id = Guid.NewGuid(),
                Email = "user@mail.com",
                Name = "Test User",
                Role = "Student",
                Ssoprovider = "Google",
                SsoproviderId = "123"
            };
        }

        // -----------------------------------------------------
        // NEW USER (NOT ADMIN)
        // -----------------------------------------------------
        [Fact]
        public async Task SignInExternal_ShouldReturn_NewUserProfileRequired()
        {
            _userRepo.Setup(r => r.GetByEmailAsync("user@mail.com"))
                     .ReturnsAsync((User?)null);

            _userRepo.Setup(r => r.AddUserAsync(It.IsAny<User>()))
                     .ReturnsAsync((User u) => u);

            var result = await _service.SignInExternalAsync(
                "Google", "abc", "user@mail.com", "Test User");

            Assert.True(result.IsNewUser);
            Assert.Null(result.Tokens);
            Assert.False(result.IsAdmin);
        }

        // -----------------------------------------------------
        // NEW ADMIN USER → Immediate login
        // -----------------------------------------------------
        [Fact]
        public async Task SignInExternal_ShouldRecognizeAdmin_AndReturnTokens()
        {
            _userRepo.Setup(r => r.GetByEmailAsync("tirths331@gmail.com"))
                     .ReturnsAsync((User?)null);

            _userRepo.Setup(r => r.AddUserAsync(It.IsAny<User>()))
                     .ReturnsAsync((User u) => u);

            _tokenRepo.Setup(t => t.AddTokenAsync(It.IsAny<RefreshToken>()))
                      .Returns(Task.CompletedTask);

            var result = await _service.SignInExternalAsync(
                "Google", "111", "tirths331@gmail.com", "Tirth");

            Assert.False(result.IsNewUser);
            Assert.True(result.IsAdmin);
            Assert.NotNull(result.Tokens);
            Assert.NotEmpty(result.Tokens.AccessToken);
        }

        // -----------------------------------------------------
        // EXISTING USER → Return tokens
        // -----------------------------------------------------
        [Fact]
        public async Task SignInExternal_ShouldReturnTokens_ForExistingUser()
        {
            var existing = CreateUser();

            _userRepo.Setup(r => r.GetByEmailAsync(existing.Email))
                     .ReturnsAsync(existing);

            _tokenRepo.Setup(t => t.AddTokenAsync(It.IsAny<RefreshToken>()))
                      .Returns(Task.CompletedTask);

            var result = await _service.SignInExternalAsync(
                "Google", "222", existing.Email, existing.Name);

            Assert.False(result.IsNewUser);
            Assert.NotNull(result.Tokens);
            Assert.NotEmpty(result.Tokens.AccessToken);
        }

        // -----------------------------------------------------
        // REFRESH TOKEN - Valid
        // -----------------------------------------------------
        [Fact]
        public async Task RefreshToken_ShouldReturnNewTokens_WhenValid()
        {
            var user = CreateUser();

            var token = new RefreshToken
            {
                Token = "valid",
                User = user,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10)
            };

            _tokenRepo.Setup(r => r.GetByTokenAsync("valid"))
                      .ReturnsAsync(token);

            _tokenRepo.Setup(r => r.RevokeTokenAsync(token))
                      .Returns(Task.CompletedTask);

            _tokenRepo.Setup(r => r.AddTokenAsync(It.IsAny<RefreshToken>()))
                      .Returns(Task.CompletedTask);

            var result = await _service.RefreshTokenAsync("valid");

            Assert.NotNull(result);
            Assert.NotEmpty(result!.AccessToken);
            Assert.NotEmpty(result.RefreshToken);
        }

        // -----------------------------------------------------
        // REFRESH TOKEN - Expired
        // -----------------------------------------------------
        [Fact]
        public async Task RefreshToken_ShouldReturnNull_WhenExpired()
        {
            var token = new RefreshToken
            {
                Token = "expired",
                ExpiresAt = DateTime.UtcNow.AddMinutes(-10)
            };

            _tokenRepo.Setup(r => r.GetByTokenAsync("expired"))
                      .ReturnsAsync(token);

            var result = await _service.RefreshTokenAsync("expired");

            Assert.Null(result);
        }

        // -----------------------------------------------------
        // REFRESH TOKEN - Revoked
        // -----------------------------------------------------
        [Fact]
        public async Task RefreshToken_ShouldReturnNull_WhenRevoked()
        {
            var token = new RefreshToken
            {
                Token = "revoked",
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                RevokedAt = DateTime.UtcNow
            };

            _tokenRepo.Setup(r => r.GetByTokenAsync("revoked"))
                      .ReturnsAsync(token);

            var result = await _service.RefreshTokenAsync("revoked");

            Assert.Null(result);
        }

        // -----------------------------------------------------
        // REMOVE REFRESH TOKEN
        // -----------------------------------------------------
        [Fact]
        public async Task RevokeRefreshToken_ShouldRevoke_WhenExists()
        {
            var token = new RefreshToken
            {
                Token = "tok123"
            };

            _tokenRepo.Setup(r => r.GetByTokenAsync("tok123"))
                      .ReturnsAsync(token);

            _tokenRepo.Setup(r => r.RevokeTokenAsync(token))
                      .Returns(Task.CompletedTask);

            await _service.RevokeRefreshTokenAsync("tok123");

            _tokenRepo.Verify(r => r.RevokeTokenAsync(token), Times.Once);
        }

        // -----------------------------------------------------
        // REVOKE NON-EXISTENT TOKEN SHOULD DO NOTHING
        // -----------------------------------------------------
        [Fact]
        public async Task RevokeRefreshToken_ShouldDoNothing_WhenNotFound()
        {
            _tokenRepo.Setup(r => r.GetByTokenAsync("nope"))
                      .ReturnsAsync((RefreshToken?)null);

            await _service.RevokeRefreshTokenAsync("nope");

            _tokenRepo.Verify(r => r.RevokeTokenAsync(It.IsAny<RefreshToken>()), Times.Never);
        }
    }
}
