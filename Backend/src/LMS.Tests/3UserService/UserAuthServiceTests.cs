using AutoFixture;
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
        private readonly Fixture _fixture;

        private readonly Role _adminRole;
        private readonly Role _teacherRole;
        private readonly Permission _permCreate;
        private readonly Permission _permEdit;
        private readonly User _seedUser;

        public UserAuthServiceTests()
        {
            _fixture = new Fixture();

            // Prevent circular navigation exceptions
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            var options = new DbContextOptionsBuilder<UserContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .EnableSensitiveDataLogging()
                .Options;

            _context = new UserContext(options);

            // -----------------------------------------
            // Seed required entities (no navigation!)
            // -----------------------------------------
            _adminRole = new Role { Id = 1, Name = "Admin" };
            _teacherRole = new Role { Id = 2, Name = "Teacher" };

            _permCreate = new Permission { Id = 1, Name = "Create" };
            _permEdit = new Permission { Id = 2, Name = "Edit" };

            _seedUser = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@mail.com",
                Name = "Kira",
                IsActive = true
            };

            _context.Users.Add(_seedUser);

            _context.Roles.AddRange(_adminRole, _teacherRole);
            _context.Permissions.AddRange(_permCreate, _permEdit);

            // Teacher role assigned
            _context.UserRoles.Add(new UserRole
            {
                UserId = _seedUser.Id,
                RoleId = _teacherRole.Id
            });

            // Teacher has "Create" permission
            _context.RolePermissions.Add(new RolePermission
            {
                RoleId = _teacherRole.Id,
                PermissionId = _permCreate.Id
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
            var request = _fixture.Build<AssignRoleRequest>()
                .With(r => r.UserId, _seedUser.Id)
                .With(r => r.RoleId, _adminRole.Id)
                .Create();

            var result = await _service.AssignRoleAsync(request);

            Assert.True(result);
            Assert.Contains(_context.UserRoles,
                ur => ur.UserId == _seedUser.Id && ur.RoleId == _adminRole.Id);
        }

        // -----------------------------------------------------
        // ASSIGN PERMISSION
        // -----------------------------------------------------
        [Fact]
        public async Task AssignPermission_ShouldAssign_WhenNotExists()
        {
            var request = _fixture.Build<AssignPermissionRequest>()
                .With(r => r.RoleId, _teacherRole.Id)
                .With(r => r.PermissionId, _permEdit.Id)
                .Create();

            var result = await _service.AssignPermissionAsync(request);

            Assert.True(result);
            Assert.Contains(_context.RolePermissions,
                rp => rp.RoleId == _teacherRole.Id && rp.PermissionId == _permEdit.Id);
        }

        // -----------------------------------------------------
        // GET ROLES
        // -----------------------------------------------------
        [Fact]
        public async Task GetRoles_ShouldReturnList()
        {
            var res = await _service.GetRolesAsync(_seedUser.Id);

            Assert.Single(res);
            Assert.Contains("Teacher", res);
        }

        // -----------------------------------------------------
        // GET PERMISSIONS
        // -----------------------------------------------------
        [Fact]
        public async Task GetPermissions_ShouldReturnList()
        {
            var res = await _service.GetPermissionsAsync(_seedUser.Id);

            Assert.Single(res);
            Assert.Contains("Create", res);
        }

        // -----------------------------------------------------
        // COMPLETE PROFILE
        // -----------------------------------------------------
        [Fact]
        public async Task CompleteProfile_ShouldReturnSuccess()
        {
            var dto = _fixture.Build<CompleteProfileDto>()
                .With(x => x.UserId, _seedUser.Id)
                .With(x => x.RoleId, _teacherRole.Id)
                .Create();

            var result = await _service.CompleteUserProfileAsync(dto);

            Assert.True(result.Success);
        }

        [Fact]
        public async Task CompleteProfile_ShouldRejectAdminRole()
        {
            var dto = _fixture.Build<CompleteProfileDto>()
                .With(x => x.UserId, _seedUser.Id)
                .With(x => x.RoleId, _adminRole.Id) // forbidden
                .Create();

            var result = await _service.CompleteUserProfileAsync(dto);

            Assert.False(result.Success);
            Assert.Equal("Cannot assign Admin role", result.Message);
        }
    }
}
