using AutoFixture;
using GatewayService.BLL.Security;
using GatewayService.DAL.Models;
using GatewayService.DAL.Repo;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using GatewayUser = GatewayService.DAL.Models.User;

namespace LMS.Tests.GatewayService
{
    public class AuthServiceTests : BaseTest
    {
        private readonly Mock<IUserRepository> _userRepo;
        private readonly Mock<IRefreshTokenRepository> _tokenRepo;
        private readonly Mock<IConfiguration> _config;
        private readonly AuthService _service;
        private readonly Fixture _fixture;
        private readonly Mock<IPasswordDecryptor> _decryptor;
        private readonly Mock<IPasswordHasher> _hasher;


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

            // JWT config
            _config.Setup(c => c["Jwt:Key"]).Returns("ThisIsASuperSecretKeyForJwtToken12345");
            _config.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
            _config.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");
            _config.Setup(c => c["Jwt:AccessTokenExpiryMinutes"]).Returns("4");
            _config.Setup(c => c["Jwt:RefreshTokenExpiryMinutes"]).Returns("3");

            // Admin email
            _config.Setup(c => c["SpecialAccounts:AdminEmail"])
                   .Returns("tirths331@gmail.com");

            _service = new AuthService(
                _userRepo.Object,
                _tokenRepo.Object,
                _config.Object,
                _decryptor.Object,
                _hasher.Object
            );
        }

        protected override void ConfigureGatewayDependencies(IServiceCollection services) { }

        private GatewayUser CreateUser(string email = "user@mail.com")
        {
            return _fixture.Build<GatewayUser>()
                .With(u => u.Email, email)
                .With(u => u.Name, "Test User")
                .With(u => u.Role, "Student")
                .With(u => u.Ssoprovider, "Google")
                .With(u => u.SsoproviderId, "123")
                .Create();
        }

        // ---------------- NEW USER (non-admin) ----------------
        [Fact]
        public async Task SignInExternal_NewUser_ShouldRequireProfileCompletion()
        {
            _userRepo.Setup(r => r.GetByEmailAsync("user@mail.com"))
                     .Returns(Task.FromResult((GatewayUser?)null));

            _userRepo.Setup(r => r.AddUserAsync(It.IsAny<GatewayUser>()))
                     .Returns<GatewayUser>(u => Task.FromResult(u));

            var result = await _service.SignInExternalAsync(
                "Google", "abc", "user@mail.com", "Test User");

            Assert.True(result.IsNewUser);
            Assert.Null(result.Tokens);
            Assert.False(result.IsAdmin);
        }

        // ---------------- NEW ADMIN ----------------
        [Fact]
        public async Task SignInExternal_NewAdmin_ShouldLoginImmediately()
        {
            _userRepo.Setup(r => r.GetByEmailAsync("tirths331@gmail.com"))
                     .Returns(Task.FromResult((GatewayUser?)null));

            _userRepo.Setup(r => r.AddUserAsync(It.IsAny<GatewayUser>()))
                     .Returns<GatewayUser>(u => Task.FromResult(u));

            _tokenRepo.Setup(t => t.AddTokenAsync(It.IsAny<RefreshToken>()))
                      .Returns(Task.CompletedTask);

            var result = await _service.SignInExternalAsync(
                "Google", "xyz", "tirths331@gmail.com", "Admin");

            Assert.False(result.IsNewUser);
            Assert.True(result.IsAdmin);
            Assert.NotNull(result.Tokens);
        }

        // ---------------- EXISTING USER ----------------
        [Fact]
        public async Task SignInExternal_ExistingUser_ShouldReturnTokens()
        {
            var existing = CreateUser();

            _userRepo.Setup(r => r.GetByEmailAsync(existing.Email))
                     .Returns(Task.FromResult((GatewayUser?)existing));

            _userRepo.Setup(r => r.SaveChangesAsync())
                     .Returns(Task.CompletedTask);

            _tokenRepo.Setup(t => t.AddTokenAsync(It.IsAny<RefreshToken>()))
                      .Returns(Task.CompletedTask);

            var result = await _service.SignInExternalAsync(
                "Google", "123", existing.Email, existing.Name);

            Assert.False(result.IsNewUser);
            Assert.NotNull(result.Tokens);
        }

        // ---------------- REFRESH VALID ----------------
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
                      .Returns(Task.FromResult((RefreshToken?)token));

            _userRepo.Setup(r => r.GetByIdAsync(user.Id))
                     .Returns(Task.FromResult((GatewayUser?)user));

            var result = await _service.RefreshTokenAsync("valid");

            Assert.NotNull(result);
            Assert.NotEmpty(result!.AccessToken);
        }

        // ---------------- REFRESH EXPIRED ----------------
        [Fact]
        public async Task RefreshToken_Expired_ShouldReturnNull()
        {
            var token = _fixture.Build<RefreshToken>()
                .With(t => t.Token, "expired")
                .With(t => t.ExpiresAt, DateTime.UtcNow.AddMinutes(-10))
                .Create();

            _tokenRepo.Setup(r => r.GetByTokenAsync("expired"))
                      .Returns(Task.FromResult((RefreshToken?)token));

            var result = await _service.RefreshTokenAsync("expired");

            Assert.Null(result);
        }

        // ---------------- REFRESH REVOKED ----------------
        [Fact]
        public async Task RefreshToken_Revoked_ShouldReturnNull()
        {
            var token = _fixture.Build<RefreshToken>()
                .With(t => t.Token, "revoked")
                .With(t => t.ExpiresAt, DateTime.UtcNow.AddMinutes(10))
                .With(t => t.RevokedAt, DateTime.UtcNow)
                .Create();

            _tokenRepo.Setup(r => r.GetByTokenAsync("revoked"))
                      .Returns(Task.FromResult((RefreshToken?)token));

            var result = await _service.RefreshTokenAsync("revoked");

            Assert.Null(result);
        }

        // ---------------- REVOKE EXISTS ----------------
        [Fact]
        public async Task RevokeRefreshToken_WhenExists_ShouldRevoke()
        {
            var token = _fixture.Build<RefreshToken>()
                .With(t => t.Token, "tok123")
                .Create();

            _tokenRepo.Setup(r => r.GetByTokenAsync("tok123"))
                      .Returns(Task.FromResult((RefreshToken?)token));

            _tokenRepo.Setup(r => r.RevokeTokenAsync(token))
                      .Returns(Task.CompletedTask);

            await _service.RevokeRefreshTokenAsync("tok123");

            _tokenRepo.Verify(r => r.RevokeTokenAsync(token), Times.Once);
        }

        // ---------------- REVOKE MISSING ----------------
        [Fact]
        public async Task RevokeRefreshToken_NotFound_ShouldDoNothing()
        {
            _tokenRepo.Setup(r => r.GetByTokenAsync("nope"))
                      .Returns(Task.FromResult((RefreshToken?)null));

            await _service.RevokeRefreshTokenAsync("nope");

            _tokenRepo.Verify(r => r.RevokeTokenAsync(It.IsAny<RefreshToken>()), Times.Never);
        }
    }
}
