using Microsoft.EntityFrameworkCore;
using UserService.BLL.DTOs;
using UserService.BLL.Service;
using UserService.DAL.Models;

namespace LMS.Tests.UserService
{
    public class UserAuthServiceTests
    {
        private readonly UserContext _context;
        private readonly UserAuthService _service;

        public UserAuthServiceTests()
        {
            var options = new DbContextOptionsBuilder<UserContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .EnableSensitiveDataLogging()
                .Options;

            _context = new UserContext(options);

            // Seed roles
            var adminRole = new Role { Id = 1, Name = "Admin" };
            var teacherRole = new Role { Id = 2, Name = "Teacher" };

            // Seed permissions
            var perm1 = new Permission { Id = 1, Name = "Create" };
            var perm2 = new Permission { Id = 2, Name = "Edit" };

            // Seed user (NO navigation props!)
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@mail.com",
                Name = "Kira",
                IsActive = true
            };

            _context.Users.Add(user);

            _context.Roles.AddRange(adminRole, teacherRole);
            _context.Permissions.AddRange(perm1, perm2);

            _context.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = teacherRole.Id
            });

            _context.RolePermissions.Add(new RolePermission
            {
                RoleId = teacherRole.Id,
                PermissionId = perm1.Id
            });

            _context.SaveChanges();

            _service = new UserAuthService(_context);
        }

        // -----------------------------------------------------
        // ASSIGN ROLE
        // -----------------------------------------------------
        [Fact]
        public async Task AssignRole_ShouldAssign_WhenNotExists()
        {
            var user = _context.Users.First();

            var request = new AssignRoleRequest
            {
                UserId = user.Id,
                RoleId = 1 // admin
            };

            var result = await _service.AssignRoleAsync(request);

            Assert.True(result);
            Assert.True(_context.UserRoles.Any(ur =>
                ur.UserId == user.Id && ur.RoleId == 1));
        }

        // -----------------------------------------------------
        // ASSIGN PERMISSION
        // -----------------------------------------------------
        [Fact]
        public async Task AssignPermission_ShouldAssign_WhenNotExists()
        {
            var request = new AssignPermissionRequest
            {
                RoleId = 2, // teacher
                PermissionId = 2 // edit
            };

            var result = await _service.AssignPermissionAsync(request);

            Assert.True(result);
            Assert.True(_context.RolePermissions.Any(rp =>
                rp.RoleId == 2 && rp.PermissionId == 2));
        }

        // -----------------------------------------------------
        // GET ROLES
        // -----------------------------------------------------
        [Fact]
        public async Task GetRoles_ShouldReturnList()
        {
            var user = _context.Users.First();

            var res = await _service.GetRolesAsync(user.Id);

            Assert.Single(res);
            Assert.Contains("Teacher", res);
        }

        // -----------------------------------------------------
        // GET PERMISSIONS
        // -----------------------------------------------------
        [Fact]
        public async Task GetPermissions_ShouldReturnList()
        {
            var user = _context.Users.First();

            var res = await _service.GetPermissionsAsync(user.Id);

            Assert.Single(res);
            Assert.Contains("Create", res);
        }

        // -----------------------------------------------------
        // COMPLETE PROFILE
        // -----------------------------------------------------
        [Fact]
        public async Task CompleteProfile_ShouldReturnSuccess()
        {
            var user = _context.Users.First();

            var dto = new CompleteProfileDto
            {
                UserId = user.Id,
                Name = "Updated",
                RoleId = 2 // Teacher
            };

            var result = await _service.CompleteUserProfileAsync(dto);

            Assert.True(result.Success);
        }

        [Fact]
        public async Task CompleteProfile_ShouldRejectAdminRole()
        {
            var user = _context.Users.First();

            var dto = new CompleteProfileDto
            {
                UserId = user.Id,
                Name = "New Name",
                RoleId = 1 // Admin role is blocked
            };

            var result = await _service.CompleteUserProfileAsync(dto);

            Assert.False(result.Success);
            Assert.Equal("Cannot assign Admin role", result.Message);
        }
    }
}
