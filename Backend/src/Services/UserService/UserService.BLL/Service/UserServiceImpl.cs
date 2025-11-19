using AutoMapper;
using UserService.BLL.DTOs;
using UserService.BLL.Interface;
using UserService.DAL.Models;
using UserService.DAL.Repo;

namespace UserService.BLL.Service
{
    public class UserServiceImpl : IUserService
    {
        private readonly IUserRepository _repo;
        private readonly IMapper _mapper;

        public UserServiceImpl(IUserRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<List<UserDto>> GetAll()
        {
            var users = await _repo.GetAllAsync();
            return _mapper.Map<List<UserDto>>(users);
        }

        public async Task<UserDto?> GetById(Guid id)
        {
            var user = await _repo.GetByIdAsync(id);
            return user == null ? null : _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> Create(CreateUserRequest request)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                Name = request.Name,
                PasswordHash = request.Password,
                Role = request.Role,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            await _repo.AddAsync(user);
            await _repo.SaveAsync();

            return _mapper.Map<UserDto>(user);
        }

        public async Task<bool> Delete(Guid id)
        {
            var user = await _repo.GetByIdAsync(id);
            if (user == null) return false;

            await _repo.DeleteAsync(user);
            await _repo.SaveAsync();

            return true;
        }
    }
}
