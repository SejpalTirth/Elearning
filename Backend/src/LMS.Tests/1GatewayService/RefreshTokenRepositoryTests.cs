using GatewayService.DAL.Data;
using GatewayService.DAL.Models;
using GatewayService.DAL.Repo;
using Microsoft.EntityFrameworkCore;

namespace LMS.Tests.GatewayService
{
    public class RefreshTokenRepositoryTests
    {
        private readonly GatewayServiceContext _context;
        private readonly RefreshTokenRepository _repo;

        public RefreshTokenRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<GatewayServiceContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new GatewayServiceContext(options);
            _repo = new RefreshTokenRepository(_context);

            // Seed a user for FK
            _context.Users.Add(new User
            {
                Id = Guid.NewGuid(),
                Email = "tester@mail.com"
            });

            _context.SaveChanges();
        }

        private User GetSeedUser() => _context.Users.First();

        // -----------------------------------------------------
        // ADD TOKEN
        // -----------------------------------------------------
        [Fact]
        public async Task AddToken_ShouldStoreToken()
        {
            var user = GetSeedUser();

            var token = new RefreshToken
            {
                Token = "abc123",
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };

            await _repo.AddTokenAsync(token);

            Assert.True(_context.RefreshTokens.Any(t => t.Token == "abc123"));
        }

        // -----------------------------------------------------
        // GET BY TOKEN
        // -----------------------------------------------------
        [Fact]
        public async Task GetByToken_ShouldReturnTokenWithUser()
        {
            var user = GetSeedUser();

            var token = new RefreshToken
            {
                Token = "xyz999",
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };

            _context.RefreshTokens.Add(token);
            _context.SaveChanges();

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

            var token = new RefreshToken
            {
                Token = "rev123",
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };

            _context.RefreshTokens.Add(token);
            _context.SaveChanges();

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

            var valid = new RefreshToken
            {
                Token = "valid",
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };

            var expired = new RefreshToken
            {
                Token = "expired",
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddHours(-1)
            };

            var revoked = new RefreshToken
            {
                Token = "revoked",
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                RevokedAt = DateTime.UtcNow
            };

            _context.RefreshTokens.AddRange(valid, expired, revoked);
            _context.SaveChanges();

            var list = await _repo.GetActiveTokensForUserAsync(user.Id);

            Assert.Single(list);
            Assert.Equal("valid", list.First().Token);
        }
    }
}
