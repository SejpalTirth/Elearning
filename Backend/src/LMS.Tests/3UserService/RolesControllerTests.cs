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

            // Fix recursion for DTO/entity graphs
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));

            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _serviceMock = new Mock<IUserService>();

            // Isolated in-memory DB instance
            _context = new UserContext(
                new DbContextOptionsBuilder<UserContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .Options
            );

            _controller = new RolesController(_serviceMock.Object, _context);
        }

        // ============================================================
        // UPDATE USER ROLE
        // ============================================================
        [Fact]
        public async Task UpdateUserRole_ShouldReturnOk_WhenServiceReturnsTrue()
        {
            var req = _fixture.Create<UpdateUserRoleRequest>();

            _serviceMock.Setup(s => s.UpdateUserRoleAsync(req))
                .ReturnsAsync(true);

            var result = await _controller.UpdateUserRole(req) as OkObjectResult;

            _serviceMock.Verify(s => s.UpdateUserRoleAsync(req), Times.Once);

            Assert.NotNull(result);
            Assert.Equal("User role updated successfully", result!.Value);
        }

        [Fact]
        public async Task UpdateUserRole_ShouldReturnBadRequest_WhenServiceReturnsFalse()
        {
            var req = _fixture.Create<UpdateUserRoleRequest>();

            _serviceMock.Setup(s => s.UpdateUserRoleAsync(req))
                .ReturnsAsync(false);

            var result = await _controller.UpdateUserRole(req) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal("Could not update role", result!.Value);
        }

        // ============================================================
        // GET USER ROLE
        // ============================================================
        [Fact]
        public async Task GetUserRole_ShouldReturnRoleList_WhenUserExists()
        {
            var userId = Guid.NewGuid();

            var dto = _fixture.Build<UserDto>()
                .With(x => x.Role, "Admin")
                .Create();

            _serviceMock.Setup(s => s.GetById(userId))
                .ReturnsAsync(dto);

            var result = await _controller.GetUserRole(userId) as OkObjectResult;

            Assert.NotNull(result);

            var roles = Assert.IsType<List<string>>(result!.Value);
            Assert.Single(roles);
            Assert.Equal("Admin", roles[0]);
        }

        [Fact]
        public async Task GetUserRole_ShouldReturnNotFound_WhenUserMissing()
        {
            var userId = Guid.NewGuid();

            _serviceMock.Setup(s => s.GetById(userId))
                .ReturnsAsync((UserDto?)null);

            var result = await _controller.GetUserRole(userId);

            Assert.IsType<NotFoundResult>(result);
        }

        // ============================================================
        // GET ALL ROLES
        // ============================================================
        [Fact]
        public async Task GetAllRoles_ShouldReturnAllRoles()
        {
            var roles = new[]
            {
                new Role { Id = 1, Name = "Admin" },
                new Role { Id = 2, Name = "Student" },
                new Role { Id = 3, Name = "Instructor" }
            };

            await _context.Roles.AddRangeAsync(roles);
            await _context.SaveChangesAsync();

            var result = await _controller.GetAllRoles() as OkObjectResult;

            Assert.NotNull(result);

            var list = result!.Value as System.Collections.IEnumerable;
            Assert.NotNull(list);
            
            // Count the items in the enumerable
            var count = 0;
            foreach (var item in list)
            {
                count++;
            }
            
            Assert.Equal(3, count);
        }

        [Fact]
        public async Task GetAllRoles_ShouldReturnEmptyList_WhenNoRolesExist()
        {
            var result = await _controller.GetAllRoles() as OkObjectResult;

            Assert.NotNull(result);

            var list = result!.Value as System.Collections.IEnumerable;
            Assert.NotNull(list);
            
            // Count the items in the enumerable
            var count = 0;
            foreach (var item in list)
            {
                count++;
            }
            
            Assert.Equal(0, count);
        }
    }
}
