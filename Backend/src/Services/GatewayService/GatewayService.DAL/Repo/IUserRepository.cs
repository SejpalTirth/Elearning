using GatewayService.DAL.Models;

namespace GatewayService.DAL.Repo
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(Guid id);
        Task<bool> ExistsByEmailAsync(string email);
        Task<User> AddUserAsync(User user);
        Task SaveChangesAsync();
    }
}
