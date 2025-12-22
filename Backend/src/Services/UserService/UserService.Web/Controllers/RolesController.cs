using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.BLL.DTOs;
using UserService.BLL.Interface;
using UserService.BLL.UserContext;
using UserService.DAL.Models;
using static UserServiceImpl;

namespace UserService.Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/roles")]
    public class RolesController : ControllerBase
    {
        private readonly IUserService _users;
        private readonly UserContext _context;
        private readonly IUserContextAccessor _userContext;

        public RolesController(
            IUserService users,
            UserContext context,
            IUserContextAccessor userContext)
        {
            _users = users;
            _context = context;
            _userContext = userContext;
        }

        // ---------------- ALL ROLES ----------------

        [HttpPost("all")]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _context.Roles
                .Select(r => new { r.Id, r.Name })
                .ToListAsync();

            return Ok(roles);
        }

        // ---------------- UPDATE ROLE ----------------

        [HttpPost("update")]
        public async Task<IActionResult> UpdateUserRole(
            [FromBody] UpdateUserRoleRequest request)
        {
            var result = await _users.UpdateUserRoleAsync(request);

            return result switch
            {
                UpdateUserRoleResult.Success =>
                    Ok(new { message = "Role updated successfully" }),

                UpdateUserRoleResult.SameRole =>
                    BadRequest(new { message = "User already has this role" }),

                UpdateUserRoleResult.RoleNotFound =>
                    BadRequest(new { message = "Invalid role selected" }),

                UpdateUserRoleResult.UserNotFound =>
                    NotFound(new { message = "User not found" }),

                UpdateUserRoleResult.LastAdminCannotBeRemoved =>
                    BadRequest(new { message = "There should be atleast one Admin on the panel." }),

                _ =>
                    StatusCode(500, new { message = "Failed to update role" })
            };
        }

        // ---------------- USER ROLE ----------------

        [HttpPost("user")]
        public async Task<IActionResult> GetUserRole()
        {
            var userId = _userContext.Current?.UserId;

            if (userId == null || userId == Guid.Empty)
                return Unauthorized("Invalid user context.");

            var user = await _users.GetById(userId.Value);
            if (user == null)
                return NotFound();

            return Ok(new List<string> { user.Role });
        }
    }
}
