using GatewayService.DAL.Data;
using GatewayService.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace GatewayService.DAL.Repo
{
    public class UserRepository : IUserRepository
    {
        private readonly GatewayServiceContext _ctx;
        public UserRepository(GatewayServiceContext ctx) { _ctx = ctx; }

        public async Task<User?> GetByEmailAsync(string email) =>
            await _ctx.Users.FirstOrDefaultAsync(u => u.Email == email);

        public async Task<User> AddUserAsync(User user)
        {
            user.Id = Guid.NewGuid();
            user.CreatedAt = DateTime.UtcNow;
            _ctx.Users.Add(user);
            await _ctx.SaveChangesAsync();
            return user;
        }

        public async Task<User?> GetByIdAsync(Guid id) =>
            await _ctx.Users.FindAsync(id);

        public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
    }
}
