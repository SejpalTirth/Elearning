using AutoFixture;
using GatewayService.DAL.Models;
using GatewayService.DAL.Repo;
using Microsoft.Extensions.DependencyInjection;

namespace LMS.Tests.GatewayService
{
    public class RefreshTokenRepositoryTests : BaseTest
    {
        private readonly RefreshTokenRepository _repo;
        private readonly Fixture _fixture;

        public RefreshTokenRepositoryTests()
        {
            // -----------------------------
            // AutoFixture Setup
            // -----------------------------
            _fixture = new Fixture();

            // Fix circular references for EF entities (User <-> RefreshToken)
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));

            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            // -----------------------------
            // Repository Under Test
            // -----------------------------
            _repo = new RefreshTokenRepository(GatewayContext);

            // -----------------------------
            // Seed required user
            // -----------------------------
            var user = _fixture.Build<User>()
                .With(u => u.Email, "tester@mail.com")
                .Create();

            GatewayContext.Users.Add(user);
            GatewayContext.SaveChanges();
        }

        protected override void ConfigureGatewayDependencies(IServiceCollection services)
        {
            // No special dependencies needed for this repository right now.
        }

        private User GetSeedUser() => GatewayContext.Users.First();

        // -----------------------------------------------------
        // ADD TOKEN
        // -----------------------------------------------------
        [Fact]
        public async Task AddToken_ShouldStoreToken()
        {
            var user = GetSeedUser();

            var token = _fixture.Build<RefreshToken>()
                .With(t => t.Token, "abc123")
                .With(t => t.UserId, user.Id)
                .With(t => t.RevokedAt, (DateTime?)null)
                .With(t => t.ExpiresAt, DateTime.UtcNow.AddHours(1))
                .Create();

            await _repo.AddTokenAsync(token);

            Assert.True(GatewayContext.RefreshTokens.Any(t => t.Token == "abc123"));
        }

        // -----------------------------------------------------
        // GET BY TOKEN
        // -----------------------------------------------------
        [Fact]
        public async Task GetByToken_ShouldReturnTokenWithUser()
        {
            var user = GetSeedUser();

            var token = _fixture.Build<RefreshToken>()
                .With(t => t.Token, "xyz999")
                .With(t => t.UserId, user.Id)
                .With(t => t.User, user)                    // IMPORTANT FIX
                .With(t => t.RevokedAt, (DateTime?)null)
                .With(t => t.ExpiresAt, DateTime.UtcNow.AddHours(1))
                .Create();

            GatewayContext.RefreshTokens.Add(token);
            GatewayContext.SaveChanges();

            var res = await _repo.GetByTokenAsync("xyz999");

            Assert.NotNull(res);
            Assert.Equal("xyz999", res!.Token);
            Assert.NotNull(res.User);
            Assert.Equal("tester@mail.com", res.User.Email);     // Now passes
        }


        // -----------------------------------------------------
        // REVOKE TOKEN
        // -----------------------------------------------------
        [Fact]
        public async Task RevokeToken_ShouldSetRevokedAt()
        {
            var user = GetSeedUser();

            var token = _fixture.Build<RefreshToken>()
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

            // 🔥 VALID TOKEN (should be returned)
            var valid = _fixture.Build<RefreshToken>()
                .With(t => t.Token, "valid")
                .With(t => t.UserId, user.Id)
                .With(t => t.User, null as User)         // IMPORTANT
                .With(t => t.ExpiresAt, DateTime.UtcNow.AddHours(1))
                .With(t => t.RevokedAt, (DateTime?)null)
                .Create();

            // ❌ EXPIRED TOKEN (should be excluded)
            var expired = _fixture.Build<RefreshToken>()
                .With(t => t.Token, "expired")
                .With(t => t.UserId, user.Id)
                .With(t => t.User, null as User)         // IMPORTANT
                .With(t => t.ExpiresAt, DateTime.UtcNow.AddHours(-1))
                .With(t => t.RevokedAt, (DateTime?)null)
                .Create();

            // ❌ REVOKED TOKEN (should be excluded)
            var revoked = _fixture.Build<RefreshToken>()
                .With(t => t.Token, "revoked")
                .With(t => t.UserId, user.Id)
                .With(t => t.User, null as User)         // IMPORTANT
                .With(t => t.ExpiresAt, DateTime.UtcNow.AddHours(1))
                .With(t => t.RevokedAt, DateTime.UtcNow)
                .Create();

            // save all tokens
            GatewayContext.RefreshTokens.AddRange(valid, expired, revoked);
            GatewayContext.SaveChanges();

            // act
            var list = await _repo.GetActiveTokensForUserAsync(user.Id);

            // assert
            Assert.Single(list);
            Assert.Equal("valid", list.First().Token);
        }

    }
}
