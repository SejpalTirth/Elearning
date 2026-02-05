using AutoFixture;
using AutoFixture.Kernel;
using GatewayService.BLL.Security;
using GatewayService.DAL.Models;
using GatewayService.DAL.Repo;
using Microsoft.Extensions.Configuration;
using Moq;

using GatewayUser = GatewayService.DAL.Models.User;

namespace LMS.Tests.GatewayService
{
    public class AuthServiceTests : BaseTest
    {
        private readonly Mock<IUserRepository> _userRepo;
        private readonly Mock<IRefreshTokenRepository> _tokenRepo;
        private readonly Mock<IConfiguration> _config;
        private readonly Mock<IPasswordDecryptor> _decryptor;
        private readonly Mock<IPasswordHasher> _hasher;
        private readonly AuthService _service;
        private readonly Fixture _fixture;

        public AuthServiceTests()
        {
            _fixture = new Fixture();

            // Prevent recursion for EF navigation props
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _userRepo = new Mock<IUserRepository>();
            _tokenRepo = new Mock<IRefreshTokenRepository>();
            _config = new Mock<IConfiguration>();
            _decryptor = new Mock<IPasswordDecryptor>();
            _hasher = new Mock<IPasswordHasher>();

            // JWT config (required for token creation)
            _config.Setup(c => c["Jwt:Key"]).Returns("ThisIsASuperSecretKeyForJwtToken12345");
            _config.Setup(c => c["Jwt:EncryptionKey"]).Returns("ThisIsAnEncryptionKeyForJweToken98765");
            _config.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
            _config.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");
            _config.Setup(c => c["Jwt:AccessTokenExpiryMinutes"]).Returns("4");
            _config.Setup(c => c["Jwt:RefreshTokenExpiryMinutes"]).Returns("3");

            // Admin email
            _config.Setup(c => c["SpecialAccounts:AdminEmail"])
                   .Returns("admin@mail.com");

            // Default decrypt/hash behavior
            _decryptor.Setup(d => d.Decrypt(It.IsAny<string>())).Returns<string>(s => s == "enc:plain" ? "plain" : s);
            _hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns<string>(p => "HASH_" + p);
            _hasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>()))
                   .Returns<string, string>((plain, hash) => hash == "HASH_" + plain);

            _service = new AuthService(
                _userRepo.Object,
                _tokenRepo.Object,
                _config.Object,
                _decryptor.Object,
                _hasher.Object);
        }

        protected override void ConfigureGatewayDependencies(Microsoft.Extensions.DependencyInjection.IServiceCollection services) { }

        private GatewayUser CreateUser(string email = "user@mail.com", string role = "Student")
        {
            return _fixture.Build<GatewayUser>()
                .With(u => u.Email, email)
                .With(u => u.Name, "Test User")
                .With(u => u.Role, role)
                .With(u => u.Ssoprovider, "Google")
                .With(u => u.SsoproviderId, "123")
                .With(u => u.IsActive, true)
                .Create();
        }

        [Fact]
        public async Task SignInExternal_NewUser_ShouldRequireProfileCompletion()
        {
            _userRepo.Setup(r => r.GetByEmailAsync("user@mail.com"))
                     .ReturnsAsync((GatewayUser?)null);

            _userRepo.Setup(r => r.AddUserAsync(It.IsAny<GatewayUser>()))
                     .ReturnsAsync((GatewayUser u) => u);

            var result = await _service.SignInExternalAsync(
                "Google", "abc", "user@mail.com", "Test User");

            Assert.True(result.IsNewUser);
            Assert.Null(result.Tokens);
            Assert.False(result.IsAdmin);
            Assert.NotEqual(Guid.Empty, result.UserId);
            _userRepo.Verify(r => r.AddUserAsync(It.Is<GatewayUser>(x => x.Email == "user@mail.com")), Times.Once);
        }

        [Fact]
        public async Task SignInExternal_NewAdmin_ShouldLoginImmediately()
        {
            var adminEmail = "admin@mail.com";
            _userRepo.Setup(r => r.GetByEmailAsync(adminEmail))
                     .ReturnsAsync((GatewayUser?)null);

            _userRepo.Setup(r => r.AddUserAsync(It.IsAny<GatewayUser>()))
                     .ReturnsAsync((GatewayUser u) =>
                     {
                         // ensure role becomes Admin as per logic
                         u.Role = "Admin";
                         return u;
                     });

            _tokenRepo.Setup(t => t.AddTokenAsync(It.IsAny<RefreshToken>()))
                      .Returns(Task.CompletedTask)
                      .Verifiable();

            var result = await _service.SignInExternalAsync(
                "Google", "xyz", adminEmail, "Admin");

            Assert.False(result.IsNewUser);
            Assert.True(result.IsAdmin);
            Assert.NotNull(result.Tokens);
            Assert.False(string.IsNullOrWhiteSpace(result.Tokens!.AccessToken));
            Assert.False(string.IsNullOrWhiteSpace(result.Tokens!.RefreshToken));
            _tokenRepo.Verify(t => t.AddTokenAsync(It.IsAny<RefreshToken>()), Times.Once);
        }

        [Fact]
        public async Task SignInExternal_ExistingUser_ShouldReturnTokens()
        {
            var existing = CreateUser();

            _userRepo.Setup(r => r.GetByEmailAsync(existing.Email))
                     .ReturnsAsync((GatewayUser?)existing);

            _tokenRepo.Setup(t => t.AddTokenAsync(It.IsAny<RefreshToken>()))
                      .Returns(Task.CompletedTask)
                      .Verifiable();

            var result = await _service.SignInExternalAsync(
                "Google", "123", existing.Email, existing.Name);

            Assert.False(result.IsNewUser);
            Assert.NotNull(result.Tokens);
            _tokenRepo.Verify(t => t.AddTokenAsync(It.IsAny<RefreshToken>()), Times.Once);
        }

        [Fact]
        public async Task RefreshToken_Valid_ShouldReturnNewAccessToken()
        {
            var user = CreateUser();
            var token = _fixture.Build<RefreshToken>()
                .With(t => t.Token, "valid")
                .With(t => t.UserId, user.Id)
                .With(t => t.ExpiresAt, DateTime.UtcNow.AddMinutes(10))
                .With(t => t.RevokedAt, (DateTime?)null)
                .Without(t => t.User)
                .Create();

            _tokenRepo.Setup(r => r.GetByTokenAsync("valid"))
                      .ReturnsAsync((RefreshToken?)token);

            _userRepo.Setup(r => r.GetByIdAsync(user.Id))
                     .ReturnsAsync(user);

            var result = await _service.RefreshTokenAsync("valid");

            Assert.NotNull(result);
            Assert.False(string.IsNullOrWhiteSpace(result!.AccessToken));
            Assert.Equal("valid", result.RefreshToken);
        }

        [Fact]
        public async Task RefreshToken_Expired_ShouldReturnNull()
        {
            var token = _fixture.Build<RefreshToken>()
                .With(t => t.Token, "expired")
                .With(t => t.ExpiresAt, DateTime.UtcNow.AddMinutes(-10))
                .Create();

            _tokenRepo.Setup(r => r.GetByTokenAsync("expired"))
                      .ReturnsAsync((RefreshToken?)token);

            var result = await _service.RefreshTokenAsync("expired");

            Assert.Null(result);
        }

        [Fact]
        public async Task RefreshToken_Revoked_ShouldReturnNull()
        {
            var token = _fixture.Build<RefreshToken>()
                .With(t => t.Token, "revoked")
                .With(t => t.ExpiresAt, DateTime.UtcNow.AddMinutes(10))
                .With(t => t.RevokedAt, DateTime.UtcNow)
                .Create();

            _tokenRepo.Setup(r => r.GetByTokenAsync("revoked"))
                      .ReturnsAsync((RefreshToken?)token);

            var result = await _service.RefreshTokenAsync("revoked");

            Assert.Null(result);
        }

        [Fact]
        public async Task RefreshToken_NullOrWhitespace_ShouldReturnNull()
        {
            var result1 = await _service.RefreshTokenAsync("");
            var result2 = await _service.RefreshTokenAsync("   ");
            Assert.Null(result1);
            Assert.Null(result2);
        }

        [Fact]
        public async Task RevokeRefreshToken_WhenExists_ShouldRevoke()
        {
            var token = _fixture.Build<RefreshToken>()
                .With(t => t.Token, "tok123")
                .Create();

            _tokenRepo.Setup(r => r.GetByTokenAsync("tok123"))
                      .ReturnsAsync((RefreshToken?)token);

            _tokenRepo.Setup(r => r.RevokeTokenAsync(token))
                      .Returns(Task.CompletedTask)
                      .Verifiable();

            await _service.RevokeRefreshTokenAsync("tok123");

            _tokenRepo.Verify(r => r.RevokeTokenAsync(token), Times.Once);
        }

        [Fact]
        public async Task RevokeRefreshToken_NotFound_ShouldDoNothing()
        {
            _tokenRepo.Setup(r => r.GetByTokenAsync("nope"))
                      .ReturnsAsync((RefreshToken?)null);

            await _service.RevokeRefreshTokenAsync("nope");

            _tokenRepo.Verify(r => r.RevokeTokenAsync(It.IsAny<RefreshToken>()), Times.Never);
        }

        [Fact]
        public async Task RegisterLocal_WhenUserAlreadyExists_ShouldThrow()
        {
            _userRepo.Setup(r => r.GetByEmailAsync("exists@mail.com"))
                     .ReturnsAsync(CreateUser("exists@mail.com"));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.RegisterLocalAsync("exists@mail.com", "enc:plain"));
        }

        [Fact]
        public async Task RegisterLocal_ShouldReturnNewUserId()
        {
            _userRepo.Setup(r => r.GetByEmailAsync("new@mail.com"))
                     .ReturnsAsync((GatewayUser?)null);

            _userRepo.Setup(r => r.AddUserAsync(It.IsAny<GatewayUser>()))
                     .ReturnsAsync((GatewayUser u) => u);

            var id = await _service.RegisterLocalAsync("new@mail.com", "enc:plain");

            Assert.NotEqual(Guid.Empty, id);
            _userRepo.Verify(r => r.AddUserAsync(It.Is<GatewayUser>(u => u.Email == "new@mail.com" && u.PasswordHash != null)), Times.Once);
        }

        [Fact]
        public async Task LoginLocal_InvalidCredentials_ShouldReturnNull()
        {
            _userRepo.Setup(r => r.GetByEmailAsync("nouser@mail.com"))
                     .ReturnsAsync((GatewayUser?)null);

            var result = await _service.LoginLocalAsync("nouser@mail.com", "enc:plain");
            Assert.Null(result);

            // user exists but wrong password
            var user = CreateUser("local@mail.com");
            user.PasswordHash = "HASH_other";
            _userRepo.Setup(r => r.GetByEmailAsync("local@mail.com"))
                     .ReturnsAsync(user);

            var result2 = await _service.LoginLocalAsync("local@mail.com", "enc:plain");
            Assert.Null(result2);
        }

        [Fact]
        public async Task LoginLocal_ValidCredentials_ShouldReturnTokens()
        {
            var user = CreateUser("local@mail.com");
            user.PasswordHash = "HASH_plain"; // matches decryptor + hasher.Verify

            _userRepo.Setup(r => r.GetByEmailAsync("local@mail.com"))
                     .ReturnsAsync(user);

            _tokenRepo.Setup(t => t.AddTokenAsync(It.IsAny<RefreshToken>()))
                      .Returns(Task.CompletedTask)
                      .Verifiable();

            var result = await _service.LoginLocalAsync("local@mail.com", "enc:plain");

            Assert.NotNull(result);
            Assert.False(string.IsNullOrWhiteSpace(result!.AccessToken));
            _tokenRepo.Verify(t => t.AddTokenAsync(It.IsAny<RefreshToken>()), Times.Once);
        }
    }
}