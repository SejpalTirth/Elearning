using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.BLL.DTOs;
using UserService.BLL.Interface;
using UserService.DAL.Models;

[ApiController]
[Route("api/roles")]
public class RolesController : ControllerBase
{
    private readonly IUserService _users;
    private readonly UserContext _context;

    public RolesController(IUserService users, UserContext context)
    {
        _users = users;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllRoles()
    {
        var roles = await _context.Roles
            .Select(r => new { r.Id, r.Name })
            .ToListAsync();

        return Ok(roles);
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateUserRole([FromBody] UpdateUserRoleRequest request)
    {
        var result = await _users.UpdateUserRoleAsync(request);
        if (!result)
            return BadRequest("Could not update role");

        return Ok("User role updated successfully");
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUserRole(Guid userId)
    {
        var user = await _users.GetById(userId);
        if (user == null)
            return NotFound();

        return Ok(new List<string> { user.Role });
    }
}
