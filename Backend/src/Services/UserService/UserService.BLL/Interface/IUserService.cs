using DTOs._3UserService;
using static UserServiceImpl;

namespace UserService.BLL.Interface
{
    public interface IUserService
    {
        Task<List<UserDto>> GetAll();
        Task<UserDto?> GetById(Guid id);
        Task<UserDto> Create(CreateUserRequest request);
        Task<UpdateUserRoleResult> UpdateUserRoleAsync(UpdateUserRoleRequest request);
        Task<bool> CompleteProfileAsync(CompleteProfileDto dto);
    }

}
