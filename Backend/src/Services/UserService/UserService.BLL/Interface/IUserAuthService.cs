using UserService.BLL.DTOs;

namespace UserService.BLL.Interface
{
    public interface IUserAuthService
    {
        Task<UserAuthDto?> GetUserAuthorizationAsync(Guid userId);
        Task<bool> AssignRoleAsync(AssignRoleRequest request);
        Task<bool> AssignPermissionAsync(AssignPermissionRequest request);
        Task<List<string>> GetRolesAsync(Guid userId);
        Task<List<string>> GetPermissionsAsync(Guid userId);
    }
}
