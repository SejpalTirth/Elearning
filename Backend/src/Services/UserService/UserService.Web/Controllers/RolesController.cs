using Microsoft.AspNetCore.Mvc;
using UserService.BLL.DTOs;
using UserService.BLL.Interface;

namespace UserService.Web.Controllers
{
    [ApiController]
    [Route("api/roles")]
    public class RolesController : ControllerBase
    {
        private readonly IUserAuthService _service;

        public RolesController(IUserAuthService service)
        {
            _service = service;
        }

        [HttpPost("assign")]
        public async Task<IActionResult> AssignRole(AssignRoleRequest request)
        {
            await _service.AssignRoleAsync(request);
            return Ok("Role Assigned");
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetRoles(Guid userId)
        {
            return Ok(await _service.GetRolesAsync(userId));
        }
    }
}
