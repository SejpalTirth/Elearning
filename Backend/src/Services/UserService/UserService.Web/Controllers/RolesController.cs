using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.BLL.DTOs;
using UserService.BLL.Interface;
using UserService.DAL.Models;

namespace UserService.Web.Controllers
{
    [ApiController]
    [Route("api/roles")]
    public class RolesController : ControllerBase
    {
        private readonly IUserAuthService _service;
        private readonly UserContext _context;

        public RolesController(IUserAuthService service, UserContext context)
        {
            _service = service;
            _context = context;
        }


        // ----------------------------------------------
        // OLD — Assign a new role (still available)
        // ----------------------------------------------
        [HttpPost("assign")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequest request)
        {
            await _service.AssignRoleAsync(request);
            return Ok("Role assigned successfully");
        }

        // ----------------------------------------------
        // NEW — Replace all existing roles with a new one
        // ----------------------------------------------
        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateUserRole([FromBody] UpdateUserRoleRequest request)
        {
            var result = await _service.UpdateUserRoleAsync(request);

            if (!result)
                return BadRequest("Failed to update user role");

            return Ok("User role updated successfully");
        }

        // ----------------------------------------------
        // Get all roles assigned to user
        // ----------------------------------------------
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetRoles(Guid userId)
        {
            var roles = await _service.GetUserRolesAsync(userId);
            return Ok(roles);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _context.Roles
                .Select(r => new { r.Id, r.Name })
                .ToListAsync();

            return Ok(roles);
        }

    }
}
