using System.Text;
using System.Text.Json;

namespace Gateway.UserContext
{
    public static class GatewayForwarder
    {
        public static void AttachUserContext(
            HttpContext context,
            HttpRequestMessage request)
        {
            if (!context.User.Identity?.IsAuthenticated ?? true)
                return;

            var userContext = GatewayUserContextBuilder.Build(context.User);

            var json = JsonSerializer.Serialize(userContext);
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

            request.Headers.TryAddWithoutValidation("X-User-Context", base64);
        }
    }

}
