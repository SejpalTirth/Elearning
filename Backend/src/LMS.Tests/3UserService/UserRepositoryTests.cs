using Microsoft.EntityFrameworkCore;
using UserService.DAL.Models;
using UserService.DAL.Repo;

namespace LMS.Tests.UserService
{
    public class UserRepositoryTests
    {
        private readonly UserContext _context;
        private readonly UserRepository _repo;

        public UserRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<UserContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new UserContext(options);
            _repo = new UserRepository(_context);
        }

        private User CreateUser(string email = "test@mail.com")
        {
            return new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                Name = "Kira",
                PasswordHash = "pass123",
                Role = "Student",
                IsActive = true
            };
        }

        // -----------------------------------------------------
        // GET BY ID
        // -----------------------------------------------------
        [Fact]
        public async Task GetById_ShouldReturnUser_WhenExists()
        {
            var user = CreateUser();
            _context.Users.Add(user);
            _context.SaveChanges();

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

        // -----------------------------------------------------
        // GET BY EMAIL
        // -----------------------------------------------------
        [Fact]
        public async Task GetByEmail_ShouldReturnUser()
        {
            var user = CreateUser("unique@mail.com");
            _context.Users.Add(user);
            _context.SaveChanges();

            var result = await _repo.GetByEmailAsync("unique@mail.com");

            Assert.NotNull(result);
            Assert.Equal("unique@mail.com", result!.Email);
        }

        [Fact]
        public async Task GetByEmail_ShouldReturnNull_WhenNotFound()
        {
            var result = await _repo.GetByEmailAsync("none@mail.com");

            Assert.Null(result);
        }

        // -----------------------------------------------------
        // GET ALL
        // -----------------------------------------------------
        [Fact]
        public async Task GetAll_ShouldReturnAllUsers()
        {
            _context.Users.Add(CreateUser("a@mail.com"));
            _context.Users.Add(CreateUser("b@mail.com"));
            _context.SaveChanges();

            var list = await _repo.GetAllAsync();

            Assert.Equal(2, list.Count);
        }

        // -----------------------------------------------------
        // ADD USER
        // -----------------------------------------------------
        [Fact]
        public async Task Add_ShouldInsertUser_WhenSaved()
        {
            var user = CreateUser("insert@mail.com");

            await _repo.AddAsync(user);
            await _repo.SaveAsync();

            Assert.True(_context.Users.Any(u => u.Email == "insert@mail.com"));
        }

        // -----------------------------------------------------
        // UPDATE USER
        // -----------------------------------------------------
        [Fact]
        public async Task Update_ShouldModifyUser_WhenSaved()
        {
            var user = CreateUser("update@mail.com");
            _context.Users.Add(user);
            _context.SaveChanges();

            user.Name = "Updated Name";

            await _repo.UpdateAsync(user);
            await _repo.SaveAsync();

            var updated = _context.Users.First(u => u.Email == "update@mail.com");
            Assert.Equal("Updated Name", updated.Name);
        }

        // -----------------------------------------------------
        // DELETE USER
        // -----------------------------------------------------
        [Fact]
        public async Task Delete_ShouldRemoveUser_WhenSaved()
        {
            var user = CreateUser("delete@mail.com");
            _context.Users.Add(user);
            _context.SaveChanges();

            await _repo.DeleteAsync(user);
            await _repo.SaveAsync();

            Assert.False(_context.Users.Any(u => u.Email == "delete@mail.com"));
        }
    }
}
