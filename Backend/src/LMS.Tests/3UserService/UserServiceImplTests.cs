using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserService.BLL.DTOs;
using UserService.DAL.Models;
using UserService.DAL.Repo;

namespace LMS.Tests.UserService
{
    public class UserServiceImplTests
    {
        private readonly IFixture _fixture;
        private readonly Mock<IUserRepository> _repoMock;
        private readonly UserContext _context;
        private readonly IMapper _mapper;
        private readonly UserServiceImpl _service;

        public UserServiceImplTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));

            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<User, UserDto>();
            });

            _mapper = config.CreateMapper();

            var options = new DbContextOptionsBuilder<UserContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new UserContext(options);

            _repoMock = new Mock<IUserRepository>();

            _service = new UserServiceImpl(_repoMock.Object, _context, _mapper);
        }

        // -------------------------------------------------
        // GET ALL
        // -------------------------------------------------
        [Fact]
        public async Task GetAll_ShouldReturnMappedList()
        {
            var users = _fixture.CreateMany<User>(2).ToList();

            _repoMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(users);

            var result = await _service.GetAll();

            Assert.Equal(2, result.Count);
        }

        // -------------------------------------------------
        // GET BY ID
        // -------------------------------------------------
        [Fact]
        public async Task GetById_ShouldReturnUser_WhenExists()
        {
            var id = Guid.NewGuid();
            var user = _fixture.Build<User>()
                .With(x => x.Id, id).Create();

            _repoMock.Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(user);

            var result = await _service.GetById(id);

            Assert.NotNull(result);
            Assert.Equal(id, result!.Id);
        }

        [Fact]
        public async Task GetById_ShouldReturnNull_WhenMissing()
        {
            var id = Guid.NewGuid();

            _repoMock.Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync((User?)null);

            var result = await _service.GetById(id);

            Assert.Null(result);
        }

        // -------------------------------------------------
        // CREATE
        // -------------------------------------------------
        [Fact]
        public async Task Create_ShouldAddUser_AndReturnDto()
        {
            var req = _fixture.Build<CreateUserRequest>()
                .With(r => r.Email, "test@mail.com")
                .Create();

            User? captured = null;

            _repoMock.Setup(r => r.AddAsync(It.IsAny<User>()))
                .Callback<User>(u => captured = u)
                .Returns(Task.CompletedTask);

            _repoMock.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

            var result = await _service.Create(req);

            Assert.Equal("test@mail.com", result.Email);
            Assert.Equal("test@mail.com", captured!.Email);
        }

        // -------------------------------------------------
        // DELETE
        // -------------------------------------------------
        [Fact]
        public async Task Delete_ShouldReturnTrue_WhenExists()
        {
            var user = _fixture.Create<User>();

            _repoMock.Setup(r => r.GetByIdAsync(user.Id))
                .ReturnsAsync(user);

            _repoMock.Setup(r => r.DeleteAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            _repoMock.Setup(r => r.SaveAsync())
                .Returns(Task.CompletedTask);

            var result = await _service.Delete(user.Id);

            Assert.True(result);
        }

        [Fact]
        public async Task Delete_ShouldReturnFalse_WhenMissing()
        {
            var id = Guid.NewGuid();

            _repoMock.Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync((User?)null);

            var result = await _service.Delete(id);

            Assert.False(result);
        }


        // =================================================================
        // UPDATE USER ROLE (MOST IMPORTANT LOGIC)
        // =================================================================

        [Fact]
        public async Task UpdateUserRole_ShouldReturnUserNotFound_WhenUserMissing()
        {
            var req = new UpdateUserRoleRequest { UserId = Guid.NewGuid(), RoleId = 1 };

            var result = await _service.UpdateUserRoleAsync(req);

            Assert.Equal(UserServiceImpl.UpdateUserRoleResult.UserNotFound, result);
        }

        [Fact]
        public async Task UpdateUserRole_ShouldReturnRoleNotFound_WhenRoleMissing()
        {
            var user = new User { Id = Guid.NewGuid(), Role = "Student" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var req = new UpdateUserRoleRequest { UserId = user.Id, RoleId = 999 };

            var result = await _service.UpdateUserRoleAsync(req);

            Assert.Equal(UserServiceImpl.UpdateUserRoleResult.RoleNotFound, result);
        }

        [Fact]
        public async Task UpdateUserRole_ShouldReturnSameRole_WhenRoleMatchesCurrent()
        {
            var role = new Role { Id = 1, Name = "Student" };
            _context.Roles.Add(role);
            var user = new User { Id = Guid.NewGuid(), Role = "Student" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var req = new UpdateUserRoleRequest { UserId = user.Id, RoleId = 1 };

            var result = await _service.UpdateUserRoleAsync(req);

            Assert.Equal(UserServiceImpl.UpdateUserRoleResult.SameRole, result);
        }

        [Fact]
        public async Task UpdateUserRole_ShouldReturnLastAdminCannotBeRemoved_WhenOnlyAdminDemoted()
        {
            var role = new Role { Id = 2, Name = "Student" };
            _context.Roles.Add(role);

            var admin = new User { Id = Guid.NewGuid(), Role = "Admin" };
            _context.Users.Add(admin);
            await _context.SaveChangesAsync();

            var req = new UpdateUserRoleRequest { UserId = admin.Id, RoleId = 2 };

            var result = await _service.UpdateUserRoleAsync(req);

            Assert.Equal(UserServiceImpl.UpdateUserRoleResult.LastAdminCannotBeRemoved, result);
        }

        [Fact]
        public async Task UpdateUserRole_ShouldSucceed_WhenMultipleAdmins()
        {
            var role = new Role { Id = 2, Name = "Student" };
            _context.Roles.Add(role);

            var admin1 = new User { Id = Guid.NewGuid(), Role = "Admin" };
            var admin2 = new User { Id = Guid.NewGuid(), Role = "Admin" };
            _context.Users.AddRange(admin1, admin2);
            await _context.SaveChangesAsync();

            var req = new UpdateUserRoleRequest { UserId = admin1.Id, RoleId = 2 };

            var result = await _service.UpdateUserRoleAsync(req);

            Assert.Equal(UserServiceImpl.UpdateUserRoleResult.Success, result);
            Assert.Equal("Student", admin1.Role);
        }

        [Fact]
        public async Task UpdateUserRole_ShouldUpdateNormalUserRole()
        {
            var role = new Role { Id = 3, Name = "Instructor" };
            _context.Roles.Add(role);

            var user = new User { Id = Guid.NewGuid(), Role = "Student" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var req = new UpdateUserRoleRequest { UserId = user.Id, RoleId = 3 };

            var result = await _service.UpdateUserRoleAsync(req);

            Assert.Equal(UserServiceImpl.UpdateUserRoleResult.Success, result);
            Assert.Equal("Instructor", user.Role);
        }
    }
}
