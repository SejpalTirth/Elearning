using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using Moq;
using DTOs._3UserService;
using UserService.BLL.Interface;
using UserService.BLL.UserContext;
using UserService.Web.Controllers;

namespace LMS.Tests.UserService
{
    public class UsersControllerTests
    {
        private readonly UsersController _controller;
        private readonly Mock<IUserService> _userMock;
        private readonly Mock<IUserContextAccessor> _userContextMock;
        private readonly Fixture _fixture;

        public UsersControllerTests()
        {
            _userMock = new Mock<IUserService>();
            _userContextMock = new Mock<IUserContextAccessor>();

            _controller = new UsersController(
                _userMock.Object,
                _userContextMock.Object
            );

            _fixture = new Fixture();
            _fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        // ==================================================================
        // GET USER (FROM CONTEXT)
        // ==================================================================

        [Fact]
        public async Task GetUser_ShouldReturnOk_WhenUserExists()
        {
            var userId = Guid.NewGuid();

            _userContextMock.Setup(x => x.Current)
                .Returns(new UserContextDto { UserId = userId });

            var dto = _fixture.Build<UserDto>()
                .With(x => x.Id, userId)
                .Create();

            _userMock.Setup(s => s.GetById(userId))
                .ReturnsAsync(dto);

            var actionResult = await _controller.GetUser();
            var okResult = actionResult.Result as OkObjectResult;

            Assert.NotNull(okResult);
            Assert.Equal(dto, okResult!.Value);
        }

        [Fact]
        public async Task GetAllUsers_ShouldReturnListOfUsers()
        {
            var users = _fixture.CreateMany<UserDto>(3).ToList();

            _userMock.Setup(s => s.GetAll())
                .ReturnsAsync(users);

            var actionResult = await _controller.GetAllUsers();
            var okResult = actionResult.Result as OkObjectResult;

            Assert.NotNull(okResult);
            Assert.Equal(users, okResult!.Value);
        }



        [Fact]
        public async Task GetUser_ShouldReturnUnauthorized_WhenContextInvalid()
        {
            _userContextMock.Setup(x => x.Current)
                .Returns((UserContextDto?)null);

            var result = await _controller.GetUser();

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task GetUser_ShouldReturnNotFound_WhenUserMissing()
        {
            var userId = Guid.NewGuid();

            _userContextMock.Setup(x => x.Current)
                .Returns(new UserContextDto { UserId = userId });

            _userMock.Setup(s => s.GetById(userId))
                .ReturnsAsync((UserDto?)null);

            var result = await _controller.GetUser();

            Assert.IsType<NotFoundResult>(result);
        }        

        [Fact]
        public async Task GetAllUsers_ShouldReturnNotFound_WhenNoUsersFound()
        {
            _userMock.Setup(s => s.GetAll())
                .ReturnsAsync(new List<UserDto>());

            var result = await _controller.GetAllUsers();

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        // ==================================================================
        // COMPLETE PROFILE
        // ==================================================================

        [Fact]
        public async Task CompleteProfile_ShouldReturnOk_WhenSuccess()
        {
            var dto = _fixture.Create<CompleteProfileDto>();

            _userMock.Setup(s => s.CompleteProfileAsync(dto))
                .ReturnsAsync(true);

            var result = await _controller.CompleteProfile(dto);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task CompleteProfile_ShouldReturnNotFound_WhenUserMissing()
        {
            var dto = _fixture.Create<CompleteProfileDto>();

            _userMock.Setup(s => s.CompleteProfileAsync(dto))
                .ReturnsAsync(false);

            var result = await _controller.CompleteProfile(dto);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        // ==================================================================
        // GET PUBLIC USER
        // ==================================================================

        [Fact]
        public async Task GetPublicUser_ShouldReturnOk_WhenUserExists()
        {
            var userId = Guid.NewGuid();

            _userMock.Setup(s => s.GetById(userId))
                .ReturnsAsync(new UserDto
                {
                    Id = userId,
                    Name = "John"
                });

            var result = await _controller.GetPublicUser(userId) as OkObjectResult;

            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetPublicUser_ShouldReturnBadRequest_WhenIdEmpty()
        {
            var result = await _controller.GetPublicUser(Guid.Empty);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetPublicUser_ShouldReturnNotFound_WhenUserMissing()
        {
            var userId = Guid.NewGuid();

            _userMock.Setup(s => s.GetById(userId))
                .ReturnsAsync((UserDto?)null);

            var result = await _controller.GetPublicUser(userId);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
