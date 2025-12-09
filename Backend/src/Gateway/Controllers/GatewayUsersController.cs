using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace Gateway.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class GatewayUsersController : ControllerBase
    {
        private readonly HttpClient _http;
        private const string BASE_USERS = "api/users";
        private const string BASE_ROLES = "api/roles";

        public GatewayUsersController(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("UserService");
        }

        private void ForwardAuth(HttpRequestMessage req)
        {
            if (Request.Headers.TryGetValue("Authorization", out var token))
                req.Headers.TryAddWithoutValidation("Authorization", token.ToString());
        }

        private async Task<IActionResult> Forward(HttpRequestMessage req)
        {
            ForwardAuth(req);

            var res = await _http.SendAsync(req);
            var raw = await res.Content.ReadAsStringAsync();

            if (!raw.Trim().StartsWith("{") && !raw.Trim().StartsWith("["))
                return StatusCode((int)res.StatusCode, new { message = raw });

            return Content(raw, "application/json");
        }

        // ------------------- USERS -------------------

        [HttpGet]
        public Task<IActionResult> GetAllUsers() =>
            Forward(new HttpRequestMessage(HttpMethod.Get, BASE_USERS));

        [HttpGet("{id:guid}")]
        public Task<IActionResult> GetUser(Guid id) =>
            Forward(new HttpRequestMessage(HttpMethod.Get, $"{BASE_USERS}/{id}"));

        [HttpDelete("{id:guid}")]
        public Task<IActionResult> DeleteUser(Guid id) =>
            Forward(new HttpRequestMessage(HttpMethod.Delete, $"{BASE_USERS}/{id}"));

        // ------------------- ROLES -------------------

        [HttpGet("roles")]
        public Task<IActionResult> GetAllRoles() =>
            Forward(new HttpRequestMessage(HttpMethod.Get, BASE_ROLES));

        [HttpGet("roles/{userId:guid}")]
        public Task<IActionResult> GetUserRole(Guid userId) =>
            Forward(new HttpRequestMessage(HttpMethod.Get, $"{BASE_ROLES}/{userId}"));

        [HttpPut("roles/update")]
        public Task<IActionResult> UpdateUserRole([FromBody] object dto) =>
            Forward(new HttpRequestMessage(HttpMethod.Put, $"{BASE_ROLES}/update")
            {
                Content = JsonContent.Create(dto)
            });
    }
}
