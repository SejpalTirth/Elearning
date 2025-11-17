using UserService.BLL.DTOs;

namespace UserService.BLL.Interface
{
    public interface IUserService
    {
        Task<List<UserDto>> GetAll();
        Task<UserDto?> GetById(Guid id);
        Task<UserDto> Create(CreateUserRequest request);
        Task<bool> Delete(Guid id);
    }
}
