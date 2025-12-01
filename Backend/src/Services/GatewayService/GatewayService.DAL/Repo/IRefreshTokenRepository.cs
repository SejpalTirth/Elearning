using GatewayService.DAL.Models;

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