using AutoFixture;
using UserService.DAL.Models;
using UserService.DAL.Repo;

namespace LMS.Tests.UserService
{
    public class UserRepositoryTests : BaseTest
    {
        private readonly UserRepository _repo;
        private readonly Fixture _fixture;

        public UserRepositoryTests()
        {
            _fixture = new Fixture();

            // Prevent circular recursion
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _repo = new UserRepository(UserContext);
        }

        private User CreateUser(string email = "test@mail.com") =>
            _fixture.Build<User>()
                .With(u => u.Id, Guid.NewGuid())
                .With(u => u.Email, email)
                .Without(u => u.RefreshTokens)
                .Create();

        [Fact]
        public async Task GetById_ShouldReturnUser_WhenExists()
        {
            var user = CreateUser();
            UserContext.Users.Add(user);
            await UserContext.SaveChangesAsync();

            var result = await _repo.GetByIdAsync(user.Id);

            Assert.NotNull(result);
            Assert.Equal(user.Email, result!.Email);
        }

        [Fact]
        public async Task GetById_ShouldReturnNull_WhenMissing()
        {
            var result = await _repo.GetByIdAsync(Guid.NewGuid());
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByEmail_ShouldReturnUser()
        {
            var user = CreateUser("unique@mail.com");
            UserContext.Users.Add(user);
            await UserContext.SaveChangesAsync();

            var result = await _repo.GetByEmailAsync("unique@mail.com");

            Assert.NotNull(result);
            Assert.Equal("unique@mail.com", result!.Email);
        }

        [Fact]
        public async Task GetAll_ShouldReturnUsers()
        {
            UserContext.Users.Add(CreateUser("a@mail.com"));
            UserContext.Users.Add(CreateUser("b@mail.com"));
            await UserContext.SaveChangesAsync();

            var list = await _repo.GetAllAsync();

            Assert.Equal(2, list.Count);
        }

        [Fact]
        public async Task Add_ShouldInsertUser()
        {
            var user = CreateUser("insert@mail.com");

            await _repo.AddAsync(user);
            await _repo.SaveAsync();

            Assert.True(UserContext.Users.Any(u => u.Email == "insert@mail.com"));
        }

        [Fact]
        public async Task Update_ShouldModifyUser()
        {
            var user = CreateUser("update@mail.com");
            UserContext.Users.Add(user);
            await UserContext.SaveChangesAsync();

            user.Name = "Updated Name";

            await _repo.UpdateAsync(user);
            await _repo.SaveAsync();

            Assert.Equal("Updated Name",
                UserContext.Users.First(u => u.Email == "update@mail.com").Name);
        }

        [Fact]
        public async Task Delete_ShouldRemoveUser()
        {
            var user = CreateUser("delete@mail.com");
            UserContext.Users.Add(user);
            await UserContext.SaveChangesAsync();

            await _repo.DeleteAsync(user);
            await _repo.SaveAsync();

            Assert.False(UserContext.Users.Any(u => u.Email == "delete@mail.com"));
        }
    }
}
