using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.BLL.DTOs;
using UserService.BLL.Interface;
using System.Security.Claims;
using UserService.BLL.UserContext;

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
        public async Task<IActionResult> GetAllUsers()
        {
            return Ok(await _userService.GetAll());
        }

        // ---------------- USER BY ID ----------------

        [HttpPost("by-id")]
        public async Task<IActionResult> GetUser()
        {
            var userId = _userContext.Current?.UserId;

            if (userId == null || userId == Guid.Empty)
                return Unauthorized("Invalid user context.");

            var user = await _userService.GetById(userId.Value);
            return user == null ? NotFound() : Ok(user);
        }

        // ---------------- DELETE USER ----------------

        [HttpPost("delete")]
        public async Task<IActionResult> Delete()
        {
            var userId = _userContext.Current?.UserId;

            if (userId == null || userId == Guid.Empty)
                return Unauthorized("Invalid user context.");

            var result = await _userService.Delete(userId.Value);
            return result ? Ok() : NotFound();
        }

        // ---------------- COMPLETE PROFILE ----------------

        [HttpPost("complete-profile")]
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
