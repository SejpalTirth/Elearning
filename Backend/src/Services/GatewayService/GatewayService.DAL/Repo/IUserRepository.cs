using GatewayService.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GatewayService.DAL.Repo
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User> AddUserAsync(User user);
        Task<User?> GetByIdAsync(Guid id);
        Task SaveChangesAsync();
    }
}
