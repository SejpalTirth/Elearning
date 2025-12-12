using AutoFixture;
using GatewayService.DAL.Models;
using GatewayService.DAL.Repo;
using Microsoft.Extensions.DependencyInjection;
using GatewayUser = GatewayService.DAL.Models.User;

namespace LMS.Tests.GatewayService
{
    public class RefreshTokenRepositoryTests : BaseTest
    {
        private readonly RefreshTokenRepository _repo;
        private readonly Fixture _fixture;
        private readonly GatewayUser _seedUser;

        public RefreshTokenRepositoryTests()
        {
            // -----------------------------
            // AutoFixture Setup
            // -----------------------------
            _fixture = new Fixture();

            // Prevent circular navigation loops
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            // -----------------------------
            // Repository Under Test
            // -----------------------------
            _repo = new RefreshTokenRepository(GatewayContext);

            // -----------------------------
            // Seed a single user (static)
            // -----------------------------
            _seedUser = new GatewayUser
            {
                Id = Guid.NewGuid(),
                Email = "tester@mail.com",
                Name = "Test User",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            GatewayContext.Users.Add(_seedUser);
            GatewayContext.SaveChanges();
        }

        protected override void ConfigureGatewayDependencies(IServiceCollection services)
        {
            // No extra dependencies needed
        }

        private GatewayUser GetSeedUser() => _seedUser;

        // -----------------------------------------------------
        // ADD TOKEN
        // -----------------------------------------------------
        [Fact]
        public async Task AddToken_ShouldStoreToken()
        {
            var user = GetSeedUser();

            var token = _fixture.Build<RefreshToken>()
                .Without(t => t.User) // avoid EF tracking issues
                .With(t => t.Token, "abc123")
                .With(t => t.UserId, user.Id)
                .With(t => t.RevokedAt, (DateTime?)null)
                .With(t => t.ExpiresAt, DateTime.UtcNow.AddHours(1))
                .Create();

            await _repo.AddTokenAsync(token);

            Assert.True(GatewayContext.RefreshTokens.Any(t => t.Token == "abc123"));
        }

        // -----------------------------------------------------
        // GET BY TOKEN — Should include User
        // -----------------------------------------------------
        [Fact]
        public async Task GetByToken_ShouldReturnTokenWithUser()
        {
            var user = GetSeedUser();

            var token = _fixture.Build<RefreshToken>()
                .Without(t => t.User) // manually set below
                .With(t => t.Token, "xyz999")
                .With(t => t.UserId, user.Id)
                .With(t => t.RevokedAt, (DateTime?)null)
                .With(t => t.ExpiresAt, DateTime.UtcNow.AddHours(1))
                .Create();

            token.User = user; // explicit assignment

            GatewayContext.RefreshTokens.Add(token);
            GatewayContext.SaveChanges();

            var res = await _repo.GetByTokenAsync("xyz999");

            Assert.NotNull(res);
            Assert.Equal("xyz999", res!.Token);
            Assert.NotNull(res.User);
            Assert.Equal("tester@mail.com", res.User.Email);
        }


        // -----------------------------------------------------
        // REVOKE TOKEN
        // -----------------------------------------------------
        [Fact]
        public async Task RevokeToken_ShouldSetRevokedAt()
        {
            var user = GetSeedUser();

            var token = _fixture.Build<RefreshToken>()
                .Without(t => t.User)
                .With(t => t.Token, "rev123")
                .With(t => t.UserId, user.Id)
                .With(t => t.RevokedAt, (DateTime?)null)
                .With(t => t.ExpiresAt, DateTime.UtcNow.AddHours(1))
                .Create();

            GatewayContext.RefreshTokens.Add(token);
            GatewayContext.SaveChanges();

            await _repo.RevokeTokenAsync(token);

            Assert.NotNull(token.RevokedAt);
        }

        // -----------------------------------------------------
        // GET ACTIVE TOKENS
        // -----------------------------------------------------
        [Fact]
        public async Task GetActiveTokens_ShouldReturnOnlyValidTokens()
        {
            var user = GetSeedUser();

            // VALID TOKEN
            var valid = _fixture.Build<RefreshToken>()
                .Without(t => t.User)
                .With(t => t.Token, "valid")
                .With(t => t.UserId, user.Id)
                .With(t => t.ExpiresAt, DateTime.UtcNow.AddHours(1))
                .With(t => t.RevokedAt, (DateTime?)null)
                .Create();

            // EXPIRED TOKEN
            var expired = _fixture.Build<RefreshToken>()
                .Without(t => t.User)
                .With(t => t.Token, "expired")
                .With(t => t.UserId, user.Id)
                .With(t => t.ExpiresAt, DateTime.UtcNow.AddHours(-1))
                .With(t => t.RevokedAt, (DateTime?)null)
                .Create();

            // REVOKED TOKEN
            var revoked = _fixture.Build<RefreshToken>()
                .Without(t => t.User)
                .With(t => t.Token, "revoked")
                .With(t => t.UserId, user.Id)
                .With(t => t.ExpiresAt, DateTime.UtcNow.AddHours(1))
                .With(t => t.RevokedAt, DateTime.UtcNow)
                .Create();

            GatewayContext.RefreshTokens.AddRange(valid, expired, revoked);
            GatewayContext.SaveChanges();

            var list = await _repo.GetActiveTokensForUserAsync(user.Id);

            Assert.Single(list); // only "valid" should remain
            Assert.Equal("valid", list.First().Token);
        }
    }
}
