using UserService.BLL.DTOs;

namespace UserService.BLL.Interface
{
    public interface IUserAuthService
    {
        // Existing Auth/User methods
        Task<UserAuthDto?> GetUserAuthorizationAsync(Guid userId);

        // Role Assignment (old method - still available for other flows)
        Task<bool> AssignRoleAsync(AssignRoleRequest request);

        // Permission Assignment
        Task<bool> AssignPermissionAsync(AssignPermissionRequest request);

        // Get user roles (old)
        Task<List<string>> GetRolesAsync(Guid userId);

        // Get permissions based on roles
        Task<List<string>> GetPermissionsAsync(Guid userId);

        // Complete profile + initial role assignment
        Task<CompleteProfileResultDto> CompleteUserProfileAsync(CompleteProfileDto dto);

        // NEW: Replace all user roles with a new role
        Task<bool> UpdateUserRoleAsync(UpdateUserRoleRequest request);

        // NEW: Get roles using clean many-to-many relationship
        Task<List<string>> GetUserRolesAsync(Guid userId);
    }
}
