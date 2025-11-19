using GatewayService.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GatewayService.DAL.Repo
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task AddTokenAsync(RefreshToken token);
        Task RevokeTokenAsync(RefreshToken token);
        Task<IEnumerable<RefreshToken>> GetActiveTokensForUserAsync(Guid userId);
        Task SaveChangesAsync();
    }
}
