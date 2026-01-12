using GatewayService.BLL.DTOs;
using GatewayService.DAL.Models;

namespace GatewayService.BLL.Interface
{
    public interface IAuthService
    {
       Task<ExternalSignInResultDto> SignInExternalAsync(
            string provider,
            string providerUserId,
            string email,
            string name
        );

        Task<TokenResponseDto?> RefreshTokenAsync(string refreshToken);
        Task RevokeRefreshTokenAsync(string refreshToken);
        Task<Guid> RegisterLocalAsync(string email, string encryptedPassword);
        Task<TokenResponseDto?> LoginLocalAsync(string email,string encryptedPassword);
    }
}
