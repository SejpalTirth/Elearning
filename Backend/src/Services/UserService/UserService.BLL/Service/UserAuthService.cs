using Microsoft.EntityFrameworkCore;
using UserService.BLL.DTOs;
using UserService.BLL.Interface;
using UserService.DAL.Models;

namespace UserService.BLL.Service
{
    public class UserAuthService : IUserAuthService
    {
        private readonly UserContext _context;

        public UserAuthService(UserContext context)
        {
            _context = context;
        }

        public async Task<UserAuthDto?> GetUserAuthorizationAsync(Guid userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return null;

            var roleIds = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleId)
                .ToListAsync();

            var roles = await _context.Roles
                .Where(r => roleIds.Contains(r.Id))
                .Select(r => r.Name)
                .ToListAsync();

            var permissionIds = await _context.RolePermissions
                .Where(rp => roleIds.Contains(rp.RoleId))
                .Select(rp => rp.PermissionId)
                .Distinct()
                .ToListAsync();

            var permissions = await _context.Permissions
                .Where(p => permissionIds.Contains(p.Id))
                .Select(p => p.Name)
                .ToListAsync();

            return new UserAuthDto
            {
                Id = user.Id,
                Email = user.Email,
                IsActive = (bool)user.IsActive,
                Roles = roles,
                Permissions = permissions
            };
        }

        public async Task<bool> AssignRoleAsync(AssignRoleRequest request)
        {
            bool exists = await _context.UserRoles
                .AnyAsync(ur => ur.UserId == request.UserId && ur.RoleId == request.RoleId);

            if (exists) return true;

            await _context.UserRoles.AddAsync(new UserRole
            {
                UserId = request.UserId,
                RoleId = request.RoleId
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AssignPermissionAsync(AssignPermissionRequest request)
        {
            bool exists = await _context.RolePermissions
                .AnyAsync(rp => rp.RoleId == request.RoleId && rp.PermissionId == request.PermissionId);

            if (exists) return true;

            await _context.RolePermissions.AddAsync(new RolePermission
            {
                RoleId = request.RoleId,
                PermissionId = request.PermissionId
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<string>> GetRolesAsync(Guid userId)
        {
            var roleIds = await _context.UserRoles
                .Where(x => x.UserId == userId)
                .Select(x => x.RoleId)
                .ToListAsync();

            return await _context.Roles
                .Where(r => roleIds.Contains(r.Id))
                .Select(r => r.Name)
                .ToListAsync();
        }

        public async Task<List<string>> GetPermissionsAsync(Guid userId)
        {
            var roleIds = await _context.UserRoles
                .Where(x => x.UserId == userId)
                .Select(x => x.RoleId)
                .ToListAsync();

            var permissionIds = await _context.RolePermissions
                .Where(rp => roleIds.Contains(rp.RoleId))
                .Select(rp => rp.PermissionId)
                .Distinct()
                .ToListAsync();

            return await _context.Permissions
                .Where(p => permissionIds.Contains(p.Id))
                .Select(p => p.Name)
                .ToListAsync();
        }

        public async Task<CompleteProfileResultDto> CompleteUserProfileAsync(CompleteProfileDto dto)
        {
            if (dto.RoleId == 1)
                return new CompleteProfileResultDto
                {
                    Success = false,
                    Message = "Cannot assign Admin role"
                };

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == dto.UserId);
            if (user == null)
                return new CompleteProfileResultDto
                {
                    Success = false,
                    Message = "User not found"
                };

            user.Name = dto.Name;

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == dto.RoleId);
            if (role == null)
                return new CompleteProfileResultDto
                {
                    Success = false,
                    Message = "Invalid RoleId"
                };

            bool exists = await _context.UserRoles
                .AnyAsync(ur => ur.UserId == dto.UserId && ur.RoleId == dto.RoleId);

            if (!exists)
            {
                await _context.UserRoles.AddAsync(new UserRole
                {
                    UserId = dto.UserId,
                    RoleId = dto.RoleId
                });
            }

            user.Role = role.Name;

            await _context.SaveChangesAsync();

            return new CompleteProfileResultDto { Success = true };
        }
    }
}
