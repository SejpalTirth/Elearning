using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using DTOs._3UserService;
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

            var result = await _controller.UpdateUserRole(req);

            Assert.IsType<OkObjectResult>(result);
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
        public async Task UpdateUserRole_ShouldReturnBadRequest_WhenRoleNotFound()
        {
            var req = _fixture.Create<UpdateUserRoleRequest>();

            _serviceMock
                .Setup(s => s.UpdateUserRoleAsync(req))
                .ReturnsAsync(UpdateUserRoleResult.RoleNotFound);

            var result = await _controller.UpdateUserRole(req);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateUserRole_ShouldReturnBadRequest_WhenLastAdminRemoved()
        {
            var req = _fixture.Create<UpdateUserRoleRequest>();

            _serviceMock
                .Setup(s => s.UpdateUserRoleAsync(req))
                .ReturnsAsync(UpdateUserRoleResult.LastAdminCannotBeRemoved);

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

            var result = await _controller.GetAllRoles();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsAssignableFrom<IEnumerable<object>>(ok.Value);

            Assert.Equal(3, list.Count());
        }

        [Fact]
        public async Task GetAllRoles_ShouldReturnEmptyList_WhenNoRolesExist()
        {
            var result = await _controller.GetAllRoles();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsAssignableFrom<IEnumerable<object>>(ok.Value);

            Assert.Empty(list);
        }
    }
}
