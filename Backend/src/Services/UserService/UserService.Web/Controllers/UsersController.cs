using Microsoft.AspNetCore.Mvc;
using UserService.BLL.Interface;
using UserService.BLL.DTOs;

namespace UserService.Web.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserAuthService _service;

        public UsersController(IUserAuthService service)
        {
            _service = service;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(Guid id)
        {
            var user = await _service.GetUserAuthorizationAsync(id);
            return user == null ? NotFound() : Ok(user);
        }

        [HttpPost("complete-profile")]
        public async Task<IActionResult> CompleteProfile([FromBody] CompleteProfileDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _service.CompleteUserProfileAsync(dto);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { success = true });
        }
    }
}
