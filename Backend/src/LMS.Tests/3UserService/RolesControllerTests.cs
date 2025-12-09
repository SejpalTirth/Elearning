using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserService.BLL.DTOs;
using UserService.BLL.Interface;
using UserService.DAL.Models;

namespace LMS.Tests.UserService
{
    public class RolesControllerTests
    {
        private readonly Mock<IUserService> _serviceMock;
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

            _serviceMock = new Mock<IUserService>();

            // Fresh isolated DB per test class instance
            var options = new DbContextOptionsBuilder<UserContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new UserContext(options);

            _controller = new RolesController(_serviceMock.Object, _context);
        }

        // -----------------------------------------------------
        // UPDATE USER ROLE
        // -----------------------------------------------------
        [Fact]
        public async Task UpdateUserRole_ShouldCallService_AndReturnOk()
        {
            var req = _fixture.Create<UpdateUserRoleRequest>();

            _serviceMock.Setup(s => s.UpdateUserRoleAsync(req))
                .Returns(Task.FromResult(true));

            var result = await _controller.UpdateUserRole(req) as OkObjectResult;

            _serviceMock.Verify(s => s.UpdateUserRoleAsync(req), Times.Once);

            Assert.NotNull(result);
            Assert.Equal("User role updated successfully", result!.Value);
        }

        [Fact]
        public async Task UpdateUserRole_ShouldReturnBadRequest_WhenServiceFails()
        {
            var req = _fixture.Create<UpdateUserRoleRequest>();

            _serviceMock.Setup(s => s.UpdateUserRoleAsync(req))
                .Returns(Task.FromResult(false));

            var result = await _controller.UpdateUserRole(req) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal("Could not update role", result!.Value);
        }

        // -----------------------------------------------------
        // GET USER ROLE
        // -----------------------------------------------------
        [Fact]
        public async Task GetUserRole_ShouldReturnRole_WhenUserExists()
        {
            var userId = Guid.NewGuid();
            var userDto = _fixture.Build<UserDto>()
                .With(x => x.Role, "Student")
                .Create();

            _serviceMock.Setup(s => s.GetById(userId))
                .Returns(Task.FromResult((UserDto?)userDto));

            var result = await _controller.GetUserRole(userId) as OkObjectResult;

            Assert.NotNull(result);

            var list = Assert.IsType<List<string>>(result!.Value);
            Assert.Single(list);
            Assert.Equal("Student", list[0]);
        }

        [Fact]
        public async Task GetUserRole_ShouldReturnNotFound_WhenUserNotExists()
        {
            var userId = Guid.NewGuid();

            _serviceMock.Setup(s => s.GetById(userId))
                .Returns(Task.FromResult((UserDto?)null));

            var result = await _controller.GetUserRole(userId);

            Assert.IsType<NotFoundResult>(result);
        }

        // -----------------------------------------------------
        // GET ALL ROLES
        // -----------------------------------------------------
        [Fact]
        public async Task GetAllRoles_ShouldReturnRoles()
        {
            // Seed the in-memory database with roles
            var role1 = new Role { Id = 1, Name = "Admin" };
            var role2 = new Role { Id = 2, Name = "Student" };
            var role3 = new Role { Id = 3, Name = "Instructor" };

            _context.Roles.AddRange(role1, role2, role3);
            await _context.SaveChangesAsync();

            var result = await _controller.GetAllRoles() as OkObjectResult;

            Assert.NotNull(result);
            
            var roles = Assert.IsType<List<object>>(result!.Value);
            Assert.Equal(3, roles.Count);
        }

        [Fact]
        public async Task GetAllRoles_ShouldReturnEmptyList_WhenNoRoles()
        {
            var result = await _controller.GetAllRoles() as OkObjectResult;

            Assert.NotNull(result);

            var roles = Assert.IsType<List<object>>(result!.Value);
            Assert.Empty(roles);
        }
    }
}
