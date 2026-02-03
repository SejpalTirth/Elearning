using DTOs._3UserService;
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
        [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<UserDto>>> GetAllUsers()
        {
            try
            {
                var result = await ForwardPost($"{BASE_USERS}/all", new { });
                if(result is ObjectResult objectResult &&
                    objectResult.Value is List<UserDto> users)
                {
                    return Ok(users);
                }
                return result as ActionResult;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Gateway error",
                    details = ex.Message
                });
            }
        }

        [HttpPost("by-id")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserDto>> GetUser(
            [FromBody] UserIdRequest request)
        {
            try
            {
                var result = await ForwardPost($"{BASE_USERS}/by-id", request);
                if(result is OkObjectResult ok)
                {
                    return Ok(ok.Value);
                }
                return result as ActionResult;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Gateway error",
                    details = ex.Message
                });
            }
        }

        [AllowAnonymous]
        [HttpPost("complete-profile")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public Task<IActionResult> CompleteProfile(
            [FromBody] CompleteProfileRequest dto) =>
            ForwardPost($"{BASE_USERS}/complete-profile", dto);

        // ------------------- ROLES -------------------

        [HttpPost("roles/all")]
        [ProducesResponseType(typeof(List<Roleresponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Roleresponse>>> GetAllRoles()
        {
            try
            {
                var result = await ForwardPost($"{BASE_ROLES}/all", new { });

                return result as ActionResult; 
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Gateway error",
                    details = ex.Message
                });
            }
        }

        [HttpPost("roles/update")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public Task<IActionResult> UpdateUserRole(
            [FromBody] UpdateUserRoleRequest dto) =>
            ForwardPost($"{BASE_ROLES}/update", dto);
    }
}
