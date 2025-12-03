using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserService.BLL.DTOs;
using UserService.BLL.Interface;
using UserService.DAL.Models;
using UserService.Web.Controllers;

namespace LMS.Tests.UserService
{
    public class RolesControllerTests
    {
        private readonly Mock<IUserAuthService> _serviceMock;
        private readonly RolesController _controller;
        private readonly UserContext _fakeContext;

        public RolesControllerTests()
        {
            _serviceMock = new Mock<IUserAuthService>();

            var options = new DbContextOptionsBuilder<UserContext>()
                .UseInMemoryDatabase("RolesControllerTestsDb")
                .Options;

            _fakeContext = new UserContext(options);

            _controller = new RolesController(_serviceMock.Object, _fakeContext);
        }

        // -----------------------------------------------------
        // ASSIGN ROLE
        // -----------------------------------------------------
        [Fact]
        public async Task AssignRole_ShouldCallService_AndReturnOk()
        {
            var req = new AssignRoleRequest
            {
                UserId = Guid.NewGuid(),
                RoleId = 3
            };

            var result = await _controller.AssignRole(req) as OkObjectResult;

            _serviceMock.Verify(s => s.AssignRoleAsync(req), Times.Once);

            Assert.NotNull(result);
            Assert.Equal("Role assigned successfully", result!.Value);
        }

        // -----------------------------------------------------
        // GET ROLES
        // -----------------------------------------------------
        [Fact]
        public async Task GetRoles_ShouldReturnList()
        {
            var id = Guid.NewGuid();

            _serviceMock.Setup(s => s.GetUserRolesAsync(id))
                .ReturnsAsync(new List<string> { "Teacher", "Admin" });

            var result = await _controller.GetRoles(id) as OkObjectResult;

            Assert.NotNull(result);
            var list = Assert.IsType<List<string>>(result!.Value);
            Assert.Equal(2, list.Count);
        }
    }
}
