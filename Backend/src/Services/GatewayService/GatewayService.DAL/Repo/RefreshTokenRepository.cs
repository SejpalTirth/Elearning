using GatewayService.DAL.Data;
using GatewayService.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace GatewayService.DAL.Repo
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly GatewayServiceContext _ctx;
        public RefreshTokenRepository(GatewayServiceContext ctx) { _ctx = ctx; }

        public async Task<RefreshToken?> GetByTokenAsync(string token) =>
            await _ctx.RefreshTokens.Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token);

        public async Task AddTokenAsync(RefreshToken token)
        {
            token.Id = Guid.NewGuid();
            token.CreatedAt = DateTime.UtcNow;
            _ctx.RefreshTokens.Add(token);
            await _ctx.SaveChangesAsync();
        }

        public async Task RevokeTokenAsync(RefreshToken token)
        {
            token.RevokedAt = DateTime.UtcNow;
            _ctx.RefreshTokens.Update(token);
            await _ctx.SaveChangesAsync();
        }

        public async Task<IEnumerable<RefreshToken>> GetActiveTokensForUserAsync(Guid userId) =>
            await _ctx.RefreshTokens.Where(t => t.UserId == userId && t.RevokedAt == null && t.ExpiresAt > DateTime.UtcNow).ToListAsync();

        public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
    }
}
