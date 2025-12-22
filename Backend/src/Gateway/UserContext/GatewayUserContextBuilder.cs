using Gateway.Contracts.Identity;
using System.Security.Claims;

namespace Gateway.UserContext
{
    public static class GatewayUserContextBuilder
    {
        public static GatewayUserContextDto Build(ClaimsPrincipal user)
        {
            var claims = user.Claims.ToList();

            return new GatewayUserContextDto
            {
                UserId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? "",
                Email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? "",
                UserName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "",
                Roles = claims
                    .Where(c => c.Type == ClaimTypes.Role)
                    .Select(c => c.Value)
                    .ToList(),
                Claims = claims
                    .GroupBy(c => c.Type)
                    .ToDictionary(g => g.Key, g => g.First().Value)
            };
        }
    }

}
