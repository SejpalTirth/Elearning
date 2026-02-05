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
        private readonly GatewayUser _otherUser;

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
            // Seed users
            // -----------------------------
            _seedUser = new GatewayUser
            {
                Id = Guid.NewGuid(),
                Email = "tester@mail.com",
                Name = "Test User",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _otherUser = new GatewayUser
            {
                Id = Guid.NewGuid(),
                Email = "other@mail.com",
                Name = "Other User",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            GatewayContext.Users.AddRange(_seedUser, _otherUser);
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
        public async Task AddToken_ShouldStoreToken_AndSetIdAndCreatedAt()
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

            var persisted = GatewayContext.RefreshTokens.SingleOrDefault(t => t.Token == "abc123");
            Assert.NotNull(persisted);
            Assert.NotEqual(Guid.Empty, persisted!.Id);
            Assert.NotEqual(default, persisted.CreatedAt);
            Assert.Equal(user.Id, persisted.UserId);
        }

        // -----------------------------------------------------
        // GET BY TOKEN — Should include User
        // -----------------------------------------------------
        [Fact]
        public async Task GetByToken_ShouldReturnTokenWithUser()
        {
            var user = GetSeedUser();

            var token = _fixture.Build<RefreshToken>()
                .Without(t => t.User)
                .With(t => t.Token, "xyz999")
                .With(t => t.UserId, user.Id)
                .With(t => t.RevokedAt, (DateTime?)null)
                .With(t => t.ExpiresAt, DateTime.UtcNow.AddHours(1))
                .Create();

            token.User = user; // explicit assignment for clarity

            GatewayContext.RefreshTokens.Add(token);
            GatewayContext.SaveChanges();

            var res = await _repo.GetByTokenAsync("xyz999");

            Assert.NotNull(res);
            Assert.Equal("xyz999", res!.Token);
            Assert.NotNull(res.User);
            Assert.Equal("tester@mail.com", res.User.Email);
        }

        [Fact]
        public async Task GetByToken_Missing_ShouldReturnNull()
        {
            var res = await _repo.GetByTokenAsync("non-existent-token");
            Assert.Null(res);
        }

        // -----------------------------------------------------
        // REVOKE TOKEN
        // -----------------------------------------------------
        [Fact]
        public async Task RevokeToken_ShouldSetRevokedAt_Persisted()
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

            // Act
            await _repo.RevokeTokenAsync(token);

            // Reload from DB to ensure it's persisted
            var persisted = GatewayContext.RefreshTokens.Single(t => t.Token == "rev123");
            Assert.NotNull(persisted.RevokedAt);
            Assert.True(persisted.RevokedAt.Value <= DateTime.UtcNow && persisted.RevokedAt.Value > DateTime.UtcNow.AddMinutes(-1));
        }

        // -----------------------------------------------------
        // GET ACTIVE TOKENS
        // -----------------------------------------------------
        [Fact]
        public async Task GetActiveTokens_ShouldReturnOnlyValidTokens_AndExcludeOtherUsers()
        {
            var user = GetSeedUser();

            // VALID TOKEN for seed user
            var valid = _fixture.Build<RefreshToken>()
                .Without(t => t.User)
                .With(t => t.Token, "valid")
                .With(t => t.UserId, user.Id)
                .With(t => t.ExpiresAt, DateTime.UtcNow.AddHours(1))
                .With(t => t.RevokedAt, (DateTime?)null)
                .Create();

            // EXPIRED TOKEN for seed user
            var expired = _fixture.Build<RefreshToken>()
                .Without(t => t.User)
                .With(t => t.Token, "expired")
                .With(t => t.UserId, user.Id)
                .With(t => t.ExpiresAt, DateTime.UtcNow.AddHours(-1))
                .With(t => t.RevokedAt, (DateTime?)null)
                .Create();

            // REVOKED TOKEN for seed user
            var revoked = _fixture.Build<RefreshToken>()
                .Without(t => t.User)
                .With(t => t.Token, "revoked")
                .With(t => t.UserId, user.Id)
                .With(t => t.ExpiresAt, DateTime.UtcNow.AddHours(1))
                .With(t => t.RevokedAt, DateTime.UtcNow)
                .Create();

            // VALID TOKEN for other user (should not be returned for seed user)
            var otherValid = _fixture.Build<RefreshToken>()
                .Without(t => t.User)
                .With(t => t.Token, "othervalid")
                .With(t => t.UserId, _otherUser.Id)
                .With(t => t.ExpiresAt, DateTime.UtcNow.AddHours(1))
                .With(t => t.RevokedAt, (DateTime?)null)
                .Create();

            GatewayContext.RefreshTokens.AddRange(valid, expired, revoked, otherValid);
            GatewayContext.SaveChanges();

            var list = await _repo.GetActiveTokensForUserAsync(user.Id);

            Assert.Single(list); // only "valid" should remain for seed user
            Assert.Equal("valid", list.First().Token);

            // Also assert other user's token is present when queried
            var otherList = await _repo.GetActiveTokensForUserAsync(_otherUser.Id);
            Assert.Single(otherList);
            Assert.Equal("othervalid", otherList.First().Token);
        }

        [Fact]
        public async Task SaveChangesAsync_ShouldReturnCompletedTask()
        {
            // Ensure method exists and works - call it after an add
            var token = _fixture.Build<RefreshToken>()
                .Without(t => t.User)
                .With(t => t.Token, "temp-save")
                .With(t => t.UserId, GetSeedUser().Id)
                .With(t => t.ExpiresAt, DateTime.UtcNow.AddMinutes(5))
                .Create();

            GatewayContext.RefreshTokens.Add(token);
            await _repo.SaveChangesAsync();

            var persisted = GatewayContext.RefreshTokens.SingleOrDefault(t => t.Token == "temp-save");
            Assert.NotNull(persisted);
        }
    }
}