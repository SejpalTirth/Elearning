using GatewayService.DAL.Data;
using GatewayService.DAL.Models;
using GatewayService.DAL.Repo;
using Microsoft.EntityFrameworkCore;

namespace LMS.Tests.GatewayService
{
    public class UserRepositoryTests
    {
        private readonly GatewayServiceContext _context;
        private readonly UserRepository _repo;

        public UserRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<GatewayServiceContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new GatewayServiceContext(options);
            _repo = new UserRepository(_context);
        }

        // -----------------------------------------------------
        // ADD USER
        // -----------------------------------------------------
        [Fact]
        public async Task AddUser_ShouldAssignId_AndSave()
        {
            var user = new User
            {
                Email = "new@mail.com"
            };

            var saved = await _repo.AddUserAsync(user);

            Assert.NotEqual(Guid.Empty, saved.Id);
            Assert.Equal("new@mail.com", saved.Email);
            Assert.True(_context.Users.Any(u => u.Email == "new@mail.com"));
        }

        // -----------------------------------------------------
        // GET BY EMAIL
        // -----------------------------------------------------
        [Fact]
        public async Task GetByEmail_ShouldReturnUser()
        {
            var user = new User
            {
                Email = "findme@mail.com"
            };

            _context.Users.Add(user);
            _context.SaveChanges();

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
            var user = new User
            {
                Email = "byid@mail.com"
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            var fetched = await _repo.GetByIdAsync(user.Id);

            Assert.NotNull(fetched);
            Assert.Equal("byid@mail.com", fetched!.Email);
        }
    }
}
