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

            // Prevent recursion (User → RefreshTokens → User…)
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));

            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _repo = new UserRepository(UserContext);
        }

        private User CreateUser(string email = "test@mail.com", string role = "Student")
        {
            return _fixture.Build<User>()
                .With(u => u.Id, Guid.NewGuid())
                .With(u => u.Email, email)
                .With(u => u.Role, role)
                .Without(u => u.RefreshTokens)
                .Create();
        }

        // ============================================================
        // GET BY ID
        // ============================================================
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
        public async Task GetById_ShouldReturnNull_WhenNotFound()
        {
            var result = await _repo.GetByIdAsync(Guid.NewGuid());
            Assert.Null(result);
        }

        // ============================================================
        // GET BY EMAIL
        // ============================================================
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
        public async Task GetByEmail_ShouldReturnNull_WhenMissing()
        {
            var result = await _repo.GetByEmailAsync("wrong@mail.com");
            Assert.Null(result);
        }

        // ============================================================
        // GET ALL
        // ============================================================
        [Fact]
        public async Task GetAll_ShouldReturnAllUsers()
        {
            UserContext.Users.Add(CreateUser("a@mail.com"));
            UserContext.Users.Add(CreateUser("b@mail.com"));
            await UserContext.SaveChangesAsync();

            var list = await _repo.GetAllAsync();

            Assert.Equal(2, list.Count);
        }

        // ============================================================
        // ADD
        // ============================================================
        [Fact]
        public async Task Add_ShouldInsertUser()
        {
            var user = CreateUser("insert@mail.com");

            await _repo.AddAsync(user);
            await _repo.SaveAsync();

            Assert.True(UserContext.Users.Any(u => u.Email == "insert@mail.com"));
        }

        // ============================================================
        // UPDATE
        // ============================================================
        [Fact]
        public async Task Update_ShouldModifyUser()
        {
            var user = CreateUser("update@mail.com");
            UserContext.Users.Add(user);
            await UserContext.SaveChangesAsync();

            user.Name = "Updated Name";

            await _repo.UpdateAsync(user);
            await _repo.SaveAsync();

            var updated = UserContext.Users.First(u => u.Email == "update@mail.com");
            Assert.Equal("Updated Name", updated.Name);
        }      

        // ============================================================
        // COUNT BY ROLE
        // ============================================================
        [Fact]
        public async Task CountByRole_ShouldReturnCorrectCount()
        {
            UserContext.Users.Add(CreateUser("a@mail.com", role: "Admin"));
            UserContext.Users.Add(CreateUser("b@mail.com", role: "Admin"));
            UserContext.Users.Add(CreateUser("c@mail.com", role: "Student"));
            await UserContext.SaveChangesAsync();

            var count = await _repo.CountByRoleAsync("Admin");

            Assert.Equal(2, count);
        }

        [Fact]
        public async Task CountByRole_ShouldReturnZero_WhenNoMatch()
        {
            var count = await _repo.CountByRoleAsync("NoRole");
            Assert.Equal(0, count);
        }
    }
}
