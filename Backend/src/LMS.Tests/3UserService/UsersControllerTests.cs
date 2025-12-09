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
        }

        // -----------------------------------------------------
        // GET USER
        // -----------------------------------------------------
        [Fact]
        public async Task GetUser_ShouldReturnUser_WhenExists()
        {
            var dto = _fixture.Build<UserDto>()
                .With(x => x.Email, "test@mail.com")
                .Create();

            _userMock.Setup(s => s.GetById(dto.Id))
                .Returns(Task.FromResult((UserDto?)dto));

            var result = await _controller.GetUser(dto.Id) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(dto, result!.Value);
        }

        [Fact]
        public async Task GetUser_ShouldReturnNotFound_WhenMissing()
        {
            var id = Guid.NewGuid();

            _userMock.Setup(s => s.GetById(id))
                .Returns(Task.FromResult((UserDto?)null));

            var result = await _controller.GetUser(id);

            Assert.IsType<NotFoundResult>(result);
        }

        // -----------------------------------------------------
        // DELETE USER
        // -----------------------------------------------------
        [Fact]
        public async Task Delete_ShouldReturnOk_WhenUserExists()
        {
            var id = Guid.NewGuid();

            _userMock.Setup(s => s.Delete(id))
                .Returns(Task.FromResult(true));

            var result = await _controller.Delete(id);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task Delete_ShouldReturnNotFound_WhenUserNotFound()
        {
            var id = Guid.NewGuid();

            _userMock.Setup(s => s.Delete(id))
                .Returns(Task.FromResult(false));

            var result = await _controller.Delete(id);

            Assert.IsType<NotFoundResult>(result);
        }

        // -----------------------------------------------------
        // GET ALL USERS
        // -----------------------------------------------------
        [Fact]
        public async Task GetAllUsers_ShouldReturnUsers()
        {
            var users = _fixture.CreateMany<UserDto>(3).ToList();

            _userMock.Setup(s => s.GetAll())
                .Returns(Task.FromResult(users));

            var result = await _controller.GetAllUsers() as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(users, result!.Value);
        }

        [Fact]
        public async Task GetAllUsers_ShouldReturnEmptyList_WhenNoUsers()
        {
            _userMock.Setup(s => s.GetAll())
                .Returns(Task.FromResult(new List<UserDto>()));

            var result = await _controller.GetAllUsers() as OkObjectResult;

            Assert.NotNull(result);
            Assert.Empty((List<UserDto>)result!.Value!);
        }
    }
}
