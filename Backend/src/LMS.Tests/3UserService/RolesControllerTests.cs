using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserService.BLL.DTOs;
using UserService.BLL.Interface;
using UserService.BLL.UserContext;
using UserService.DAL.Models;
using UserService.Web.Controllers;
using static UserServiceImpl;

namespace LMS.Tests.UserService
{
    public class RolesControllerTests
    {
        private readonly Mock<IUserService> _serviceMock;
        private readonly Mock<IUserContextAccessor> _userContextMock;
        private readonly RolesController _controller;
        private readonly UserContext _context;
        private readonly Fixture _fixture;

        public RolesControllerTests()
        {
            _fixture = new Fixture();

            _fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));

            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _serviceMock = new Mock<IUserService>();
            _userContextMock = new Mock<IUserContextAccessor>();

            _context = new UserContext(
                new DbContextOptionsBuilder<UserContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .Options
            );

            _controller = new RolesController(
                _serviceMock.Object,
                _context,
                _userContextMock.Object
            );
        }

        // ============================================================
        // UPDATE USER ROLE
        // ============================================================

        [Fact]
        public async Task UpdateUserRole_ShouldReturnOk_WhenSuccess()
        {
            var req = _fixture.Create<UpdateUserRoleRequest>();

            _serviceMock
                .Setup(s => s.UpdateUserRoleAsync(req))
                .ReturnsAsync(UpdateUserRoleResult.Success);

            var result = await _controller.UpdateUserRole(req) as OkObjectResult;

            Assert.NotNull(result);
        }

        [Fact]
        public async Task UpdateUserRole_ShouldReturnBadRequest_WhenSameRole()
        {
            var req = _fixture.Create<UpdateUserRoleRequest>();

            _serviceMock
                .Setup(s => s.UpdateUserRoleAsync(req))
                .ReturnsAsync(UpdateUserRoleResult.SameRole);

            var result = await _controller.UpdateUserRole(req);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateUserRole_ShouldReturnNotFound_WhenUserMissing()
        {
            var req = _fixture.Create<UpdateUserRoleRequest>();

            _serviceMock
                .Setup(s => s.UpdateUserRoleAsync(req))
                .ReturnsAsync(UpdateUserRoleResult.UserNotFound);

            var result = await _controller.UpdateUserRole(req);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        // ============================================================
        // GET USER ROLE
        // ============================================================

        [Fact]
        public async Task GetUserRole_ShouldReturnRole_WhenUserExists()
        {
            var userId = Guid.NewGuid();

            _userContextMock.Setup(x => x.Current)
                .Returns(new UserContextDto { UserId = userId });

            _serviceMock.Setup(s => s.GetById(userId))
                .ReturnsAsync(new UserDto
                {
                    Id = userId,
                    Role = "Admin"
                });

            var result = await _controller.GetUserRole() as OkObjectResult;

            Assert.NotNull(result);

            var roles = Assert.IsType<List<string>>(result!.Value);
            Assert.Single(roles);
            Assert.Equal("Admin", roles[0]);
        }

        [Fact]
        public async Task GetUserRole_ShouldReturnUnauthorized_WhenUserContextInvalid()
        {
            _userContextMock.Setup(x => x.Current)
                .Returns((UserContextDto?)null);

            var result = await _controller.GetUserRole();

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task GetUserRole_ShouldReturnNotFound_WhenUserMissing()
        {
            var userId = Guid.NewGuid();

            _userContextMock.Setup(x => x.Current)
                .Returns(new UserContextDto { UserId = userId });

            _serviceMock.Setup(s => s.GetById(userId))
                .ReturnsAsync((UserDto?)null);

            var result = await _controller.GetUserRole();

            Assert.IsType<NotFoundResult>(result);
        }

        // ============================================================
        // GET ALL ROLES
        // ============================================================

        [Fact]
        public async Task GetAllRoles_ShouldReturnAllRoles()
        {
            await _context.Roles.AddRangeAsync(
                new Role { Id = 1, Name = "Admin" },
                new Role { Id = 2, Name = "Student" },
                new Role { Id = 3, Name = "Instructor" }
            );

            await _context.SaveChangesAsync();

            var result = await _controller.GetAllRoles() as OkObjectResult;

            Assert.NotNull(result);

            var list = Assert.IsAssignableFrom<IEnumerable<object>>(result!.Value);
            Assert.Equal(3, list.Count());
        }

        [Fact]
        public async Task GetAllRoles_ShouldReturnEmptyList_WhenNoRolesExist()
        {
            var result = await _controller.GetAllRoles() as OkObjectResult;

            Assert.NotNull(result);

            var list = Assert.IsAssignableFrom<IEnumerable<object>>(result!.Value);
            Assert.Empty(list);
        }
    }
}
