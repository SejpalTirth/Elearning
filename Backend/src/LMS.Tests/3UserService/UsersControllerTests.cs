using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Text.Json;
using UserService.BLL.DTOs;
using UserService.BLL.Interface;
using UserService.Web.Controllers;

namespace LMS.Tests.UserService
{
    public class UsersControllerTests
    {
        private readonly UsersController _controller;
        private readonly Mock<IUserAuthService> _authMock;
        private readonly Mock<IUserService> _userMock;
        private readonly Fixture _fixture;

        public UsersControllerTests()
        {
            _authMock = new Mock<IUserAuthService>();
            _userMock = new Mock<IUserService>();

            _controller = new UsersController(_authMock.Object, _userMock.Object);

            _fixture = new Fixture();
        }

        // -----------------------------------------------------
        // GET USER
        // -----------------------------------------------------
        [Fact]
        public async Task GetUser_ShouldReturnUser_WhenExists()
        {
            var dto = _fixture.Build<UserAuthDto>()
                .With(x => x.Email, "test@mail.com")
                .Create();

            _authMock.Setup(s => s.GetUserAuthorizationAsync(dto.Id))
                .ReturnsAsync(dto);

            var result = await _controller.GetUser(dto.Id) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(dto, result!.Value);
        }

        [Fact]
        public async Task GetUser_ShouldReturnNotFound_WhenMissing()
        {
            var id = Guid.NewGuid();

            _authMock.Setup(s => s.GetUserAuthorizationAsync(id))
                .ReturnsAsync((UserAuthDto?)null);

            var result = await _controller.GetUser(id);

            Assert.IsType<NotFoundResult>(result);
        }

        // -----------------------------------------------------
        // COMPLETE PROFILE
        // -----------------------------------------------------
        [Fact]
        public async Task CompleteProfile_ShouldReturnOk_WhenSuccess()
        {
            var dto = _fixture.Build<CompleteProfileDto>()
                .With(x => x.RoleId, 2)
                .With(x => x.Name, "Kira")
                .Create();

            _authMock.Setup(s => s.CompleteUserProfileAsync(dto))
                .ReturnsAsync(new CompleteProfileResultDto { Success = true });

            var result = await _controller.CompleteProfile(dto) as OkObjectResult;

            Assert.NotNull(result);

            // Convert anonymous object → JSON → Dictionary
            var json = JsonSerializer.Serialize(result!.Value);
            var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json)!;

            Assert.True(bool.Parse(dict["success"]!.ToString()!));
        }


        [Fact]
        public async Task CompleteProfile_ShouldReturnBadRequest_WhenValidationFails()
        {
            var dto = new CompleteProfileDto(); // invalid

            _controller.ModelState.AddModelError("Name", "Required");

            var result = await _controller.CompleteProfile(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task CompleteProfile_ShouldReturnBadRequest_WhenServiceFails()
        {
            var dto = _fixture.Build<CompleteProfileDto>()
                .With(x => x.RoleId, 1)
                .With(x => x.Name, "Test")
                .Create();

            var failResult = new CompleteProfileResultDto
            {
                Success = false,
                Message = "Cannot assign Admin"
            };

            _authMock.Setup(s => s.CompleteUserProfileAsync(dto))
                .ReturnsAsync(failResult);

            var result = await _controller.CompleteProfile(dto) as BadRequestObjectResult;

            Assert.NotNull(result);

            // Convert anonymous object → JSON → Dictionary
            var json = JsonSerializer.Serialize(result!.Value);
            var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json)!;

            Assert.Equal("Cannot assign Admin", dict["message"]!.ToString());
        }

    }
}
