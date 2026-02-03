using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DTOs._3UserService;
using UserService.BLL.Interface;
using UserService.BLL.UserContext;
using DTOs._5ProgresService;

namespace UserService.Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IUserContextAccessor _userContext;

        public UsersController(
            IUserService userService,
            IUserContextAccessor userContext)
        {
            _userService = userService;
            _userContext = userContext;
        }

        // ---------------- ALL USERS ----------------

        [HttpPost("all")]
        [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<UserDto>>> GetAllUsers()
        {
            var result = await _userService.GetAll();
            if(result == null || !result.Any())
                return NotFound("No users found.");
            return Ok(result);
        }

        // ---------------- USER BY ID ----------------

        [HttpPost("by-id")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserDto?>> GetUser()
        {
            var userId = _userContext.Current?.UserId;

            if (userId == null || userId == Guid.Empty)
                return Unauthorized("Invalid user context.");

            var user = await _userService.GetById(userId.Value);
            return user == null ? NotFound() : Ok(user);
        }

        // ---------------- COMPLETE PROFILE ----------------

        [AllowAnonymous]
        [HttpPost("complete-profile")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CompleteProfile(
            [FromBody] CompleteProfileDto dto)
        {
            var success = await _userService.CompleteProfileAsync(dto);

            return success
                ? Ok(new { message = "Profile updated successfully" })
                : NotFound("User not found.");
        }

        [AllowAnonymous]
        [HttpGet("public/{id}")]
        public async Task<IActionResult> GetPublicUser(Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest("Invalid user id.");

            var user = await _userService.GetById(id);

            if (user == null)
                return NotFound();

        return Ok(new PublicUserDto
        {
            UserId = user.Id,
            Name = user.Name ?? "Unknown Instructor"
        });
    }

}
}
