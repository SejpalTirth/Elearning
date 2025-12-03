using Microsoft.AspNetCore.Mvc;
using Moq;
using UserService.BLL.DTOs;
using UserService.BLL.Interface;
using UserService.Web.Controllers;

namespace LMS.Tests.UserService
{
    public class UsersControllerTests
    {
        private readonly Mock<IUserAuthService> _serviceMock;
        private readonly UsersController _controller;
        private readonly Mock<IUserService> _userServiceMock;

        public UsersControllerTests()
        {
            _serviceMock = new Mock<IUserAuthService>();
            _userServiceMock = new Mock<IUserService>();

            _controller = new UsersController(
                _serviceMock.Object,
                _userServiceMock.Object
            );
        }

        // -----------------------------------------------------
        // GET USER
        // -----------------------------------------------------
        [Fact]
        public async Task GetUser_ShouldReturnUser_WhenExists()
        {
            var id = Guid.NewGuid();
            var dto = new UserAuthDto { Id = id, Email = "test@mail.com" };

            _serviceMock.Setup(s => s.GetUserAuthorizationAsync(id))
                .ReturnsAsync(dto);

            var result = await _controller.GetUser(id) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(dto, result!.Value);
        }

        [Fact]
        public async Task GetUser_ShouldReturnNotFound_WhenMissing()
        {
            var id = Guid.NewGuid();

            _serviceMock.Setup(s => s.GetUserAuthorizationAsync(id))
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
            var dto = new CompleteProfileDto
            {
                UserId = Guid.NewGuid(),
                RoleId = 2,
                Name = "Kira"
            };

            _serviceMock.Setup(s => s.CompleteUserProfileAsync(dto))
                .ReturnsAsync(new CompleteProfileResultDto { Success = true });

            var result = await _controller.CompleteProfile(dto) as OkObjectResult;

            Assert.NotNull(result);

            var dict = result!.Value.GetType()
                .GetProperties()
                .ToDictionary(p => p.Name, p => p.GetValue(result.Value));

            Assert.True((bool)dict["success"]);
        }


        [Fact]
        public async Task CompleteProfile_ShouldReturnBadRequest_WhenValidationFails()
        {
            var dto = new CompleteProfileDto();

            _controller.ModelState.AddModelError("Name", "Required");

            var result = await _controller.CompleteProfile(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task CompleteProfile_ShouldReturnBadRequest_WhenServiceFails()
        {
            var dto = new CompleteProfileDto
            {
                UserId = Guid.NewGuid(),
                RoleId = 1,
                Name = "Test"
            };

            _serviceMock.Setup(s => s.CompleteUserProfileAsync(dto))
                .ReturnsAsync(new CompleteProfileResultDto
                {
                    Success = false,
                    Message = "Cannot assign Admin"
                });

            var result = await _controller.CompleteProfile(dto) as BadRequestObjectResult;

            Assert.NotNull(result);

            var dict = result!.Value.GetType()
                .GetProperties()
                .ToDictionary(p => p.Name, p => p.GetValue(result.Value));

            Assert.Equal("Cannot assign Admin", dict["message"]);
        }
    }
}
