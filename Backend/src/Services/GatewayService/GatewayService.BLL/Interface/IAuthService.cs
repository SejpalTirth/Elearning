using GatewayService.BLL.DTOs;

namespace GatewayService.BLL.Interface
{
    public interface IAuthService
    {
        // Called from Gateway controller after external provider callback info
        Task<ExternalSignInResultDto> SignInExternalAsync(string provider, string providerUserId, string email, string name);

        // Use refresh token to get new access token (single-use refresh tokens)
        Task<TokenResponseDto?> RefreshTokenAsync(string refreshToken);

        // (Optional) Revoke a refresh token
        Task RevokeRefreshTokenAsync(string refreshToken);
    }

}