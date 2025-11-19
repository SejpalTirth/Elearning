using Microsoft.AspNetCore.Mvc;
using UserService.BLL.DTOs;
using UserService.BLL.Interface;

namespace UserService.Web.Controllers
{
    [ApiController]
    [Route("api/permissions")]
    public class PermissionsController : ControllerBase
    {
        private readonly IUserAuthService _service;

        public PermissionsController(IUserAuthService service)
        {
            _service = service;
        }

        [HttpPost("assign")]
        public async Task<IActionResult> AssignPermission(AssignPermissionRequest request)
        {
            await _service.AssignPermissionAsync(request);
            return Ok("Permission Assigned");
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetPermissions(Guid userId)
        {
            return Ok(await _service.GetPermissionsAsync(userId));
        }
    }
}
