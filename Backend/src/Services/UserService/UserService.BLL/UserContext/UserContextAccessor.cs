using Microsoft.AspNetCore.Http;
using System.Text;
using System.Text.Json;

namespace UserService.BLL.UserContext
{
    public class UserContextAccessor : IUserContextAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContextAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public UserContextDto? Current
        {
            get
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null)
                    return null;

                if (!httpContext.Request.Headers.TryGetValue("X-User-Context", out var header))
                    return null;

                var json = Encoding.UTF8.GetString(Convert.FromBase64String(header!));
                return JsonSerializer.Deserialize<UserContextDto>(json);
            }
        }
    }
}
