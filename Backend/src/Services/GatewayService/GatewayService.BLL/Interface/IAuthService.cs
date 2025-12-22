using GatewayService.BLL.DTOs;

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
    }
}
