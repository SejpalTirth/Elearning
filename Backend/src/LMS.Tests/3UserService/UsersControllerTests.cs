using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UserService.BLL.DTOs;
using UserService.BLL.Interface;

namespace LMS.Tests.UserService
{
    public class UsersControllerTests
    {
        private readonly UsersController _controller;
        private readonly Mock<IUserService> _userMock;
        private readonly Fixture _fixture;

        public UsersControllerTests()
        {
            _userMock = new Mock<IUserService>();
            _controller = new UsersController(_userMock.Object);

            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        // ==================================================================
        // GET USER BY ID
        // ==================================================================
        [Fact]
        public async Task GetUser_ShouldReturnOk_WhenUserExists()
        {
            var dto = _fixture.Build<UserDto>()
                .With(x => x.Email, "test@mail.com")
                .Create();

            _userMock.Setup(s => s.GetById(dto.Id))
                     .ReturnsAsync(dto);

            var result = await _controller.GetUser(dto.Id) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(dto, result!.Value);
        }

        [Fact]
        public async Task GetUser_ShouldReturnNotFound_WhenUserNotExists()
        {
            var id = Guid.NewGuid();

            _userMock.Setup(s => s.GetById(id))
                     .ReturnsAsync((UserDto?)null);

            var result = await _controller.GetUser(id);

            Assert.IsType<NotFoundResult>(result);
        }

        // ==================================================================
        // DELETE USER
        // ==================================================================
        [Fact]
        public async Task Delete_ShouldReturnOk_WhenDeletionSucceeds()
        {
            var id = Guid.NewGuid();

            _userMock.Setup(s => s.Delete(id))
                     .ReturnsAsync(true);

            var result = await _controller.Delete(id);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task Delete_ShouldReturnNotFound_WhenDeletionFails()
        {
            var id = Guid.NewGuid();

            _userMock.Setup(s => s.Delete(id))
                     .ReturnsAsync(false);

            var result = await _controller.Delete(id);

            Assert.IsType<NotFoundResult>(result);
        }

        // ==================================================================
        // GET ALL USERS
        // ==================================================================
        [Fact]
        public async Task GetAllUsers_ShouldReturnListOfUsers()
        {
            var users = _fixture.CreateMany<UserDto>(3).ToList();

            _userMock.Setup(s => s.GetAll())
                     .ReturnsAsync(users);

            var result = await _controller.GetAllUsers() as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(users, result!.Value);
        }

        [Fact]
        public async Task GetAllUsers_ShouldReturnEmptyList_WhenNoUsersFound()
        {
            _userMock.Setup(s => s.GetAll())
                     .ReturnsAsync(new List<UserDto>());

            var result = await _controller.GetAllUsers() as OkObjectResult;

            Assert.NotNull(result);

            var list = Assert.IsType<List<UserDto>>(result!.Value);
            Assert.Empty(list);
        }

        // ==================================================================
        // EDGE CASES (optional but improves coverage robustness)
        // ==================================================================
        [Fact]
        public async Task GetUser_ShouldCallServiceExactlyOnce()
        {
            var id = Guid.NewGuid();
            _userMock.Setup(s => s.GetById(id))
                     .ReturnsAsync((UserDto?)null);

            await _controller.GetUser(id);

            _userMock.Verify(s => s.GetById(id), Times.Once);
        }

        [Fact]
        public async Task Delete_ShouldCallServiceExactlyOnce()
        {
            var id = Guid.NewGuid();
            _userMock.Setup(s => s.Delete(id)).ReturnsAsync(false);

            await _controller.Delete(id);

            _userMock.Verify(s => s.Delete(id), Times.Once);
        }
    }
}
