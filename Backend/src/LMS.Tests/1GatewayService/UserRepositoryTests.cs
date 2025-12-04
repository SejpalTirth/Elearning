using AutoFixture;
using GatewayService.DAL.Models;
using GatewayService.DAL.Repo;
using Microsoft.Extensions.DependencyInjection;

namespace LMS.Tests.GatewayService
{
    public class UserRepositoryTests : BaseTest
    {
        private readonly UserRepository _repo;
        private readonly Fixture _fixture;

        public UserRepositoryTests()
        {
            // -----------------------------
            // AutoFixture Setup
            // -----------------------------
            _fixture = new Fixture();

            // Fix circular references (User <-> Tokens)
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));

            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            // -----------------------------
            // Repository Under Test
            // -----------------------------
            _repo = new UserRepository(GatewayContext);
        }

        protected override void ConfigureGatewayDependencies(IServiceCollection services)
        {
            // Nothing special needed for UserRepository now
        }

        // -----------------------------------------------------
        // ADD USER
        // -----------------------------------------------------
        [Fact]
        public async Task AddUser_ShouldAssignId_AndSave()
        {
            // AutoFixture builds user but we override Email
            var user = _fixture.Build<User>()
                .With(u => u.Email, "new@mail.com")
                .Without(u => u.RefreshTokens)   // Avoid random token list
                .Create();

            var saved = await _repo.AddUserAsync(user);

            Assert.NotEqual(Guid.Empty, saved.Id);
            Assert.Equal("new@mail.com", saved.Email);
            Assert.True(GatewayContext.Users.Any(u => u.Email == "new@mail.com"));
        }

        // -----------------------------------------------------
        // GET BY EMAIL
        // -----------------------------------------------------
        [Fact]
        public async Task GetByEmail_ShouldReturnUser()
        {
            var user = _fixture.Build<User>()
                .With(u => u.Email, "findme@mail.com")
                .Without(u => u.RefreshTokens)
                .Create();

            GatewayContext.Users.Add(user);
            GatewayContext.SaveChanges();

            var fetched = await _repo.GetByEmailAsync("findme@mail.com");

            Assert.NotNull(fetched);
            Assert.Equal("findme@mail.com", fetched!.Email);
        }

        // -----------------------------------------------------
        // GET BY ID
        // -----------------------------------------------------
        [Fact]
        public async Task GetById_ShouldReturnUser()
        {
            var user = _fixture.Build<User>()
                .With(u => u.Email, "byid@mail.com")
                .Without(u => u.RefreshTokens)
                .Create();

            GatewayContext.Users.Add(user);
            GatewayContext.SaveChanges();

            var fetched = await _repo.GetByIdAsync(user.Id);

            Assert.NotNull(fetched);
            Assert.Equal("byid@mail.com", fetched!.Email);
        }
    }
}
