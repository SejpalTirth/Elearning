using GatewayService.BLL.Interface;
using GatewayService.DAL.Models;
using GatewayService.DAL.Repo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace Gateway.Controllers
{
    [ApiController]
    [Route("api/debug")]
    public class DebugAuthController : ControllerBase
    {
        private readonly IAuthService _auth;
        private readonly IUserRepository _users;

        public DebugAuthController(IAuthService auth, IUserRepository users)
        {
            _auth = auth;
            _users = users;
        }

        [HttpGet("token")]
        [AllowAnonymous]
        public IActionResult GetToken()
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "debug@test.com",
                Name = "Debug User",
                Role = "Admin",
                Ssoprovider = "Debug"
            };

            var token = typeof(AuthService)
                .GetMethod("CreateEncryptedJwt", BindingFlags.NonPublic | BindingFlags.Instance)!
                .Invoke(_auth, new object[] { user });

            return Ok(new { accessToken = token });
        }

    }

}
