using AutoFixture;
using GatewayService.DAL.Models;
using GatewayService.DAL.Repo;
using Microsoft.Extensions.DependencyInjection;
using GatewayUser = GatewayService.DAL.Models.User;

namespace LMS.Tests.GatewayService
{
    public class UserRepositoryTests : BaseTest
    {
        private readonly UserRepository _repo;
        private readonly Fixture _fixture;

        public UserRepositoryTests()
        {
            // -----------------------------
            // AutoFixture setup (avoid recursion)
            // -----------------------------
            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            // -----------------------------
            // Repository under test
            // -----------------------------
            _repo = new UserRepository(GatewayContext);
        }

        protected override void ConfigureGatewayDependencies(IServiceCollection services)
        {
            // No special dependencies needed
        }

        // -----------------------------------------------------
        // ADD USER
        // -----------------------------------------------------
        [Fact]
        public async Task AddUser_ShouldAssignId_AndSave()
        {
            var user = _fixture.Build<GatewayUser>()
                .Without(u => u.RefreshTokens)
                .With(u => u.Email, "new@mail.com")
                .With(u => u.Id, Guid.Empty) // ensure repo sets new ID
                .Create();

            var saved = await _repo.AddUserAsync(user);

            Assert.NotEqual(Guid.Empty, saved.Id);
            Assert.Equal("new@mail.com", saved.Email);
            Assert.True(GatewayContext.Users.Any(u => u.Email == "new@mail.com"));
            Assert.NotNull(saved.CreatedAt);
        }

        // -----------------------------------------------------
        // GET BY EMAIL
        // -----------------------------------------------------
        [Fact]
        public async Task GetByEmail_ShouldReturnUser()
        {
            var user = _fixture.Build<GatewayUser>()
                .Without(u => u.RefreshTokens)
                .With(u => u.Email, "findme@mail.com")
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
            var user = _fixture.Build<GatewayUser>()
                .Without(u => u.RefreshTokens)
                .With(u => u.Email, "byid@mail.com")
                .Create();

            GatewayContext.Users.Add(user);
            GatewayContext.SaveChanges();

            var fetched = await _repo.GetByIdAsync(user.Id);

            Assert.NotNull(fetched);
            Assert.Equal("byid@mail.com", fetched!.Email);
        }
    }
}
