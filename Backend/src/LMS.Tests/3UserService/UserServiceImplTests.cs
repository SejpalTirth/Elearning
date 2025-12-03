using AutoMapper;
using Moq;
using UserService.BLL.DTOs;
using UserService.BLL.Service;
using UserService.DAL.Models;
using UserService.DAL.Repo;

namespace LMS.Tests.UserService
{
    public class UserServiceImplTests
    {
        private readonly Mock<IUserRepository> _repoMock;
        private readonly IMapper _mapper;
        private readonly UserServiceImpl _service;

        public UserServiceImplTests()
        {
            _repoMock = new Mock<IUserRepository>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<User, UserDto>();
            });

            _mapper = config.CreateMapper();
            _service = new UserServiceImpl(_repoMock.Object, _mapper);
        }

        // -----------------------------------------------------
        // GET ALL
        // -----------------------------------------------------
        [Fact]
        public async Task GetAll_ShouldReturnMappedList()
        {
            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<User>
            {
                new User { Id = Guid.NewGuid(), Email = "a@mail.com" }
            });

            var result = await _service.GetAll();

            Assert.Single(result);
            Assert.Equal("a@mail.com", result[0].Email);
        }

        // -----------------------------------------------------
        // GET BY ID
        // -----------------------------------------------------
        [Fact]
        public async Task GetById_ShouldReturnMappedUser()
        {
            var id = Guid.NewGuid();

            _repoMock.Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(new User { Id = id, Email = "x@mail.com" });

            var result = await _service.GetById(id);

            Assert.NotNull(result);
            Assert.Equal("x@mail.com", result!.Email);
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

        // -----------------------------------------------------
        // CREATE
        // -----------------------------------------------------
        [Fact]
        public async Task Create_ShouldAddUser_AndReturnDto()
        {
            var req = new CreateUserRequest
            {
                Email = "test@mail.com",
                Name = "Kira",
                Password = "123",
                Role = "Student"
            };

            User? addedUser = null;

            _repoMock.Setup(r => r.AddAsync(It.IsAny<User>()))
                .Callback<User>(u => addedUser = u)
                .Returns(Task.CompletedTask);

            _repoMock.Setup(r => r.SaveAsync())
                .Returns(Task.CompletedTask);

            var result = await _service.Create(req);

            Assert.NotNull(result);
            Assert.Equal("test@mail.com", result.Email);
            Assert.NotNull(addedUser);
        }

        // -----------------------------------------------------
        // DELETE
        // -----------------------------------------------------
        [Fact]
        public async Task Delete_ShouldRemove_WhenExists()
        {
            var user = new User { Id = Guid.NewGuid() };

            _repoMock.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
            _repoMock.Setup(r => r.DeleteAsync(user)).Returns(Task.CompletedTask);

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
    }
}
