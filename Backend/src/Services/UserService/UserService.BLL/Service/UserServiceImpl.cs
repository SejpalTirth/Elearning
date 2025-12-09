using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UserService.BLL.DTOs;
using UserService.BLL.Interface;
using UserService.DAL.Models;
using UserService.DAL.Repo;

public class UserServiceImpl : IUserService
{
    private readonly IUserRepository _repo;
    private readonly UserContext _context;
    private readonly IMapper _mapper;

    public UserServiceImpl(IUserRepository repo, UserContext context, IMapper mapper)
    {
        _repo = repo;
        _context = context;
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

    public async Task<bool> UpdateUserRoleAsync(UpdateUserRoleRequest req)
    {
        var user = await _repo.GetByIdAsync(req.UserId);
        if (user == null) return false;

        // Prevent demoting last admin
        if (user.Role == "Admin")
        {
            var role = await _context.Roles.FindAsync(req.RoleId);
            if (role != null && role.Name != "Admin")
            {
                var adminCount = await _context.Users.CountAsync(u => u.Role == "Admin");
                if (adminCount <= 1)
                    throw new InvalidOperationException("Cannot remove the last admin");
            }
        }

        var newRole = await _context.Roles.FindAsync(req.RoleId);
        if (newRole == null) return false;

        user.Role = newRole.Name;
        user.UpdatedAt = DateTime.UtcNow;

        await _repo.SaveAsync();
        return true;
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
