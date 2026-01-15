using Gateway.Contracts.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/users")]
    public class GatewayUsersController : BaseGatewayController
    {
        private const string BASE_USERS = "api/users";
        private const string BASE_ROLES = "api/roles";

        public GatewayUsersController(IHttpClientFactory factory)
            : base(factory.CreateClient("UserService"))
        {
        }

        // ------------------- USERS -------------------

        [HttpPost("all")]
        public Task<IActionResult> GetAllUsers() =>
            ForwardPost($"{BASE_USERS}/all", new { });

        [HttpPost("by-id")]
        public Task<IActionResult> GetUser(
            [FromBody] UserIdRequest request) =>
            ForwardPost($"{BASE_USERS}/by-id", request);

        [HttpPost("delete")]
        public Task<IActionResult> DeleteUser(
            [FromBody] UserIdRequest request) =>
            ForwardPost($"{BASE_USERS}/delete", request);

        [AllowAnonymous]
        [HttpPost("complete-profile")]
        public Task<IActionResult> CompleteProfile(
            [FromBody] CompleteProfileRequest dto) =>
            ForwardPost($"{BASE_USERS}/complete-profile", dto);

        // ------------------- ROLES -------------------

        [HttpPost("roles/all")]
        public Task<IActionResult> GetAllRoles() =>
            ForwardPost($"{BASE_ROLES}/all", new { });

        [HttpPost("roles/user")]
        public Task<IActionResult> GetUserRole(
            [FromBody] UserIdRequest request) =>
            ForwardPost($"{BASE_ROLES}/user", request);

        [HttpPost("roles/update")]
        public Task<IActionResult> UpdateUserRole(
            [FromBody] UpdateUserRoleRequest dto) =>
            ForwardPost($"{BASE_ROLES}/update", dto);
    }
}
