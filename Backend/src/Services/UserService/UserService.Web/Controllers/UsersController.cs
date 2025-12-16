using Microsoft.AspNetCore.Mvc;
using UserService.BLL.DTOs;
using UserService.BLL.Interface;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        return Ok(await _userService.GetAll());
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest("userId cannot be empty.");

        var user = await _userService.GetById(id);
        return user == null ? NotFound() : Ok(user);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest("userId cannot be empty.");

        var result = await _userService.Delete(id);
        return result ? Ok() : NotFound();
    }

    [HttpPost("complete-profile")]
    public async Task<IActionResult> CompleteProfile([FromBody] CompleteProfileDto dto)
    {
        var success = await _userService.CompleteProfileAsync(dto);

        if (!success)
            return NotFound("User not found.");

        return Ok(new { message = "Profile updated successfully" });
    }
}
