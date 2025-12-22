using UserService.BLL.DTOs;
using static UserServiceImpl;

namespace UserService.BLL.Interface
{
    public interface IUserService
    {
        Task<List<UserDto>> GetAll();
        Task<UserDto?> GetById(Guid id);
        Task<UserDto> Create(CreateUserRequest request);
        Task<bool> Delete(Guid id);
        Task<UpdateUserRoleResult> UpdateUserRoleAsync(UpdateUserRoleRequest request);
        Task<bool> CompleteProfileAsync(CompleteProfileDto dto);
    }

}
