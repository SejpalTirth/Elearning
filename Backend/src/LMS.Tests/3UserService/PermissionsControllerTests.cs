using AutoFixture;
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
        private readonly Fixture _fixture;

        public PermissionsControllerTests()
        {
            _serviceMock = new Mock<IUserAuthService>();
            _controller = new PermissionsController(_serviceMock.Object);

            _fixture = new Fixture();

            // Recursion guard (safe default for DTOs)
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        // -----------------------------------------------------
        // ASSIGN PERMISSION
        // -----------------------------------------------------
        [Fact]
        public async Task AssignPermission_ShouldCallService_AndReturnOk()
        {
            var req = _fixture.Build<AssignPermissionRequest>()
                .With(r => r.RoleId, 2)
                .With(r => r.PermissionId, 5)
                .Create();

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

            var permissions = _fixture.CreateMany<string>(2).ToList();

            _serviceMock.Setup(s => s.GetPermissionsAsync(userId))
                .ReturnsAsync(permissions);

            var result = await _controller.GetPermissions(userId) as OkObjectResult;

            Assert.NotNull(result);

            var list = Assert.IsType<List<string>>(result!.Value);
            Assert.Equal(permissions.Count, list.Count);

            // verify same contents
            foreach (var item in permissions)
                Assert.Contains(item, list);
        }
    }
}
