using AutoFixture;
using GatewayService.DAL.Models;
using GatewayService.DAL.Repo;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace LMS.Tests.GatewayService
{
    public class AuthServiceTests : BaseTest
    {
        private readonly Mock<IUserRepository> _userRepo;
        private readonly Mock<IRefreshTokenRepository> _tokenRepo;
        private readonly Mock<IConfiguration> _config;
        private readonly AuthService _service;
        private readonly Fixture _fixture;

        public AuthServiceTests()
        {
            // -----------------------------
            // AutoFixture Setup
            // -----------------------------
            _fixture = new Fixture();

            // Fix circular reference issue for EF Core models
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));

            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            // -----------------------------
            // Mock Repositories & Config
            // -----------------------------
            _userRepo = new Mock<IUserRepository>();
            _tokenRepo = new Mock<IRefreshTokenRepository>();
            _config = new Mock<IConfiguration>();

            // -----------------------------
            // JWT Config Setup
            // -----------------------------
            _config.Setup(c => c["Jwt:Key"]).Returns("ThisIsASuperSecretKeyForJwtToken12345");
            _config.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
            _config.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");

            // -----------------------------
            // Admin Account Setup
            // -----------------------------
            _config.Setup(c => c["SpecialAccounts:AdminEmail"])
                   .Returns("tirths331@gmail.com");

            // -----------------------------
            // AuthService Instantiation
            // -----------------------------
            _service = new AuthService(_userRepo.Object, _tokenRepo.Object, _config.Object);
        }


        protected override void ConfigureGatewayDependencies(IServiceCollection services)
        {
            // For AuthService specifically, no special gateway dependencies needed now.
            // But future Gateway tests may add IHttpClientFactory mocks here.
        }

        private User CreateUser()
        {
            return _fixture.Build<User>()
                .With(u => u.Email, "user@mail.com")
                .With(u => u.Name, "Test User")
                .With(u => u.Role, "Student")
                .With(u => u.Ssoprovider, "Google")
                .With(u => u.SsoproviderId, "123")
                .Create();
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

            var token = _fixture.Build<RefreshToken>()
                .With(t => t.Token, "valid")
                .With(t => t.User, user)
                .With(t => t.ExpiresAt, DateTime.UtcNow.AddMinutes(10))
                .With(t => t.RevokedAt, (DateTime?)null)   // Important fix
                .Create();

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
            var token = _fixture.Build<RefreshToken>()
                .With(t => t.Token, "expired")
                .With(t => t.ExpiresAt, DateTime.UtcNow.AddMinutes(-10))
                .Create();

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
            var token = _fixture.Build<RefreshToken>()
                .With(t => t.Token, "revoked")
                .With(t => t.ExpiresAt, DateTime.UtcNow.AddMinutes(10))
                .With(t => t.RevokedAt, DateTime.UtcNow)
                .Create();

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
            var token = _fixture.Build<RefreshToken>()
                .With(t => t.Token, "tok123")
                .Create();

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
