using Gateway.UserContext;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Gateway.Controllers
{
    [ApiController]
    public abstract class BaseGatewayController : ControllerBase
    {
        protected readonly HttpClient _http;

        protected BaseGatewayController(HttpClient http)
        {
            _http = http;
        }

        // -------------------- CORE FORWARD --------------------

        protected async Task<IActionResult> ForwardPost(HttpRequestMessage request)
        {
            ForwardAuth(request);
            ForwardUserContext(request);

            var response = await _http.SendAsync(request);
            var raw = await response.Content.ReadAsStringAsync();

            // Preserve status codes
            return StatusCode(
                (int)response.StatusCode,
                string.IsNullOrWhiteSpace(raw) ? null : raw
            );
        }

        // -------------------- SHORTCUT HELPERS --------------------

        protected Task<IActionResult> ForwardPost(string url, object body)
        {
            var req = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(body)
            };

            return ForwardPost(req);
        }

        protected Task<IActionResult> ForwardGet(string url)
        {
            var req = new HttpRequestMessage(HttpMethod.Get, url);
            return ForwardPost(req);
        }

        protected Task<IActionResult> ForwardPut(string url, object body)
        {
            var req = new HttpRequestMessage(HttpMethod.Put, url)
            {
                Content = JsonContent.Create(body)
            };

            return ForwardPost(req);
        }

        protected Task<IActionResult> ForwardDelete(string url)
        {
            var req = new HttpRequestMessage(HttpMethod.Delete, url);
            return ForwardPost(req);
        }

        // -------------------- HEADER FORWARDING --------------------

        private void ForwardAuth(HttpRequestMessage req)
        {
            if (Request.Headers.TryGetValue("Authorization", out var token))
            {
                req.Headers.Authorization =
                    AuthenticationHeaderValue.Parse(token!);
            }
        }

        private void ForwardUserContext(HttpRequestMessage req)
        {
            if (!User.Identity?.IsAuthenticated ?? true)
                return;

            var userContext = GatewayUserContextBuilder.Build(User);

            var json = JsonSerializer.Serialize(userContext);
            var base64 = Convert.ToBase64String(
                Encoding.UTF8.GetBytes(json)
            );

            req.Headers.TryAddWithoutValidation("X-User-Context", base64);
        }
    }
}
