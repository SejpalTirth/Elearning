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

    public async Task<UpdateUserRoleResult> UpdateUserRoleAsync(UpdateUserRoleRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId);

        if (user == null)
            return UpdateUserRoleResult.UserNotFound;

        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Id == request.RoleId);

        if (role == null)
            return UpdateUserRoleResult.RoleNotFound;

        // Same role check
        if (string.Equals(
            user.Role,
            role.Name,
            StringComparison.OrdinalIgnoreCase))
        {
            return UpdateUserRoleResult.SameRole;
        }

        // 🚨 CRITICAL RULE: At least one Admin must remain
        if (string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(role.Name, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            var adminCount = await _context.Users
                .CountAsync(u => u.Role == "Admin");

            if (adminCount <= 1)
            {
                return UpdateUserRoleResult.LastAdminCannotBeRemoved;
            }
        }

        user.Role = role.Name;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return UpdateUserRoleResult.Success;
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
    public async Task<bool> CompleteProfileAsync(CompleteProfileDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.UserId))
            return false;

        var userId = Guid.Parse(dto.UserId);

        var user = await _repo.GetByIdAsync(userId);
        if (user == null)
            return false;

        user.Name = dto.Name;
        user.Role = dto.Role;
        user.UpdatedAt = DateTime.UtcNow;

        await _repo.SaveAsync();
        return true;
    }

    public enum UpdateUserRoleResult
    {
        Success,
        SameRole,
        UserNotFound,
        RoleNotFound,
        LastAdminCannotBeRemoved
    }
}
