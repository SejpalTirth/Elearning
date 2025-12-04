using AutoFixture;
using AutoFixture.AutoMoq;
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
        private readonly IFixture _fixture;
        private readonly Mock<IUserRepository> _repoMock;
        private readonly IMapper _mapper;
        private readonly UserServiceImpl _service;

        public UserServiceImplTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            // Fix AutoFixture recursion by switching behavior
            _fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));

            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            // AutoMapper config
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<User, UserDto>();
            });

            _mapper = config.CreateMapper();
            _repoMock = new Mock<IUserRepository>();
            _service = new UserServiceImpl(_repoMock.Object, _mapper);
        }


        // -----------------------------------------------------
        // GET ALL
        // -----------------------------------------------------
        [Fact]
        public async Task GetAll_ShouldReturnMappedList()
        {
            var users = _fixture.Build<User>()
                .With(u => u.Email, "a@mail.com")
                .CreateMany(1)
                .ToList();

            _repoMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(users);

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

            var user = _fixture.Build<User>()
                .With(u => u.Id, id)
                .With(u => u.Email, "x@mail.com")
                .Create();

            _repoMock.Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(user);

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
            var req = _fixture.Build<CreateUserRequest>()
                .With(r => r.Email, "test@mail.com")
                .With(r => r.Name, "Kira")
                .With(r => r.Role, "Student")
                .Create();

            User? captured = null;

            _repoMock.Setup(r => r.AddAsync(It.IsAny<User>()))
                .Callback<User>(u => captured = u)
                .Returns(Task.CompletedTask);

            _repoMock.Setup(r => r.SaveAsync())
                .Returns(Task.CompletedTask);

            var result = await _service.Create(req);

            Assert.NotNull(result);
            Assert.Equal("test@mail.com", result.Email);

            Assert.NotNull(captured);   // verify repo received correct model
            Assert.Equal("test@mail.com", captured!.Email);
        }

        // -----------------------------------------------------
        // DELETE
        // -----------------------------------------------------
        [Fact]
        public async Task Delete_ShouldRemove_WhenExists()
        {
            var user = _fixture.Create<User>();

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
