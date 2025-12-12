using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class GatewayUsersController : BaseGatewayController
    {
        private readonly HttpClient _http;
        private const string BASE_USERS = "api/users";
        private const string BASE_ROLES = "api/roles";

        public GatewayUsersController(IHttpClientFactory factory)
            : base(factory.CreateClient("UserService"))
        {
            _http = factory.CreateClient("UserService");
        }

        // ------------------- USERS -------------------

        [HttpGet]
        public Task<IActionResult> GetAllUsers() =>
            ForwardGet($"{BASE_USERS}");

        [HttpGet("{id:guid}")]
        public Task<IActionResult> GetUser(Guid id) =>
            ForwardGet($"{BASE_USERS}/{id}");

        [HttpDelete("{id:guid}")]
        public Task<IActionResult> DeleteUser(Guid id) =>
            ForwardDelete($"{BASE_USERS}/{id}");

        // NEW: Complete Profile
        [HttpPost("complete-profile")]
        public Task<IActionResult> CompleteProfile([FromBody] object dto) =>
            ForwardPost($"{BASE_USERS}/complete-profile", dto);

        // ------------------- ROLES -------------------

        [HttpGet("roles")]
        public Task<IActionResult> GetAllRoles() =>
            ForwardGet($"{BASE_ROLES}");

        [HttpGet("roles/{userId:guid}")]
        public Task<IActionResult> GetUserRole(Guid userId) =>
            ForwardGet($"{BASE_ROLES}/{userId}");

        [HttpPut("roles/update")]
        public Task<IActionResult> UpdateUserRole([FromBody] object dto) =>
            ForwardPut($"{BASE_ROLES}/update", dto);
    }
}
