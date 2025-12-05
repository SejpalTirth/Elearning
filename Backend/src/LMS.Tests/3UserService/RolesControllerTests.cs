using AutoFixture;
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
        private readonly UserContext _context;
        private readonly Fixture _fixture;

        public RolesControllerTests()
        {
            _fixture = new Fixture();

            // Prevent recursion (clean AutoFixture objects)
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _serviceMock = new Mock<IUserAuthService>();

            // Fresh isolated DB per test class instance
            var options = new DbContextOptionsBuilder<UserContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new UserContext(options);

            _controller = new RolesController(_serviceMock.Object, _context);
        }

        // -----------------------------------------------------
        // ASSIGN ROLE
        // -----------------------------------------------------
        [Fact]
        public async Task AssignRole_ShouldCallService_AndReturnOk()
        {
            var req = _fixture.Create<AssignRoleRequest>();

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
            var userId = Guid.NewGuid();
            var roles = _fixture.CreateMany<string>(3).ToList();

            _serviceMock.Setup(s => s.GetUserRolesAsync(userId))
                .ReturnsAsync(roles);

            var result = await _controller.GetRoles(userId) as OkObjectResult;

            Assert.NotNull(result);

            var list = Assert.IsType<List<string>>(result!.Value);
            Assert.Equal(roles.Count, list.Count);

            foreach (var role in roles)
                Assert.Contains(role, list);
        }
    }
}
