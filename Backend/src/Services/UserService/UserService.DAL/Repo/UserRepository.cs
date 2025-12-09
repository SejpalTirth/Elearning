using Microsoft.EntityFrameworkCore;
using UserService.DAL.Models;

namespace UserService.DAL.Repo
{
    public class UserRepository : IUserRepository
    {
        private readonly UserContext _context;

        public UserRepository(UserContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(Guid id)
            => await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

        public async Task<User?> GetByEmailAsync(string email)
            => await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        public async Task<List<User>> GetAllAsync()
            => await _context.Users.ToListAsync();

        public async Task AddAsync(User user)
            => await _context.Users.AddAsync(user);

        public Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(User user)
        {
            _context.Users.Remove(user);
            return Task.CompletedTask;
        }

        public async Task<int> CountByRoleAsync(string role)
            => await _context.Users.CountAsync(u => u.Role == role);

        public async Task SaveAsync()
            => await _context.SaveChangesAsync();
    }
}
