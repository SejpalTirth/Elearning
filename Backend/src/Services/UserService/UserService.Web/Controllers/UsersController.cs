using Microsoft.AspNetCore.Mvc;
using UserService.BLL.Interface;

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
    }
}
