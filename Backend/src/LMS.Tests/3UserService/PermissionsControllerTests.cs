using Microsoft.AspNetCore.Mvc;
using Moq;
using UserService.BLL.DTOs;
using UserService.BLL.Interface;
using UserService.Web.Controllers;

namespace LMS.Tests.UserService
{
    public class PermissionsControllerTests
    {
        private readonly Mock<IUserAuthService> _serviceMock;
        private readonly PermissionsController _controller;

        public PermissionsControllerTests()
        {
            _serviceMock = new Mock<IUserAuthService>();
            _controller = new PermissionsController(_serviceMock.Object);
        }

        // -----------------------------------------------------
        // ASSIGN PERMISSION
        // -----------------------------------------------------
        [Fact]
        public async Task AssignPermission_ShouldCallService_AndReturnOk()
        {
            var req = new AssignPermissionRequest
            {
                RoleId = 2,
                PermissionId = 5
            };

            var result = await _controller.AssignPermission(req) as OkObjectResult;

            _serviceMock.Verify(s => s.AssignPermissionAsync(req), Times.Once);

            Assert.NotNull(result);
            Assert.Equal("Permission Assigned", result!.Value);
        }

        // -----------------------------------------------------
        // GET PERMISSIONS
        // -----------------------------------------------------
        [Fact]
        public async Task GetPermissions_ShouldReturnList()
        {
            var userId = Guid.NewGuid();

            _serviceMock.Setup(s => s.GetPermissionsAsync(userId))
                .ReturnsAsync(new List<string> { "Create", "Edit" });

            var result = await _controller.GetPermissions(userId) as OkObjectResult;

            Assert.NotNull(result);
            var list = Assert.IsType<List<string>>(result!.Value);
            Assert.Equal(2, list.Count);
            Assert.Contains("Create", list);
        }
    }
}
