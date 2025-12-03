using GatewayService.BLL.DTOs;
using GatewayService.BLL.Interface;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace LMS.Tests.Gateway
{
    public class GatewayAuthControllerTests
    {
        private readonly Mock<IAuthService> _authMock = new();
        private readonly Mock<ILogger<GatewayAuthController>> _logMock = new();

        private GatewayAuthController CreateController(
    ClaimsPrincipal? externalUser = null,
    bool externalSucceeded = true)
        {
            var controller = new GatewayAuthController(_authMock.Object, _logMock.Object);

            // HttpContext
            var ctx = new DefaultHttpContext();
            var services = new ServiceCollection();

            // 1) Fake AuthenticationService
            var fakeAuthService = new Mock<IAuthenticationService>();
            fakeAuthService.Setup(x => x.AuthenticateAsync(
                    It.IsAny<HttpContext>(), "External"))
                .ReturnsAsync(() =>
                    externalSucceeded
                        ? AuthenticateResult.Success(
                                new AuthenticationTicket(
                                    externalUser ?? new ClaimsPrincipal(),
                                    new AuthenticationProperties
                                    {
                                        Items = { [".AuthScheme"] = "Google" }
                                    },
                                    "External"))
                        : AuthenticateResult.Fail("failed"));

            fakeAuthService.Setup(x => x.SignInAsync(
                    It.IsAny<HttpContext>(),
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    It.IsAny<ClaimsPrincipal>(),
                    It.IsAny<AuthenticationProperties>()))
                .Returns(Task.CompletedTask);

            fakeAuthService.Setup(x => x.SignOutAsync(
                    It.IsAny<HttpContext>(),
                    It.IsAny<string>(),
                    It.IsAny<AuthenticationProperties>()))
                .Returns(Task.CompletedTask);

            services.AddSingleton<IAuthenticationService>(fakeAuthService.Object);

            // 2) Fake UrlHelper + factory
            var urlHelper = new Mock<IUrlHelper>();
            urlHelper.Setup(x => x.IsLocalUrl(It.IsAny<string>())).Returns(true);
            urlHelper
                .Setup(x => x.Action(It.IsAny<UrlActionContext>()))
                .Returns("http://localhost/callback");

            var urlFactory = new Mock<IUrlHelperFactory>();
            urlFactory.Setup(f => f.GetUrlHelper(It.IsAny<ActionContext>()))
                      .Returns(urlHelper.Object);

            services.AddSingleton<IUrlHelperFactory>(urlFactory.Object);

            // Build provider
            ctx.RequestServices = services.BuildServiceProvider();

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = ctx
            };

            // Assign Url helper explicitly
            controller.Url = urlHelper.Object;

            return controller;
        }


        // ---------------------------------------------------------
        // GOOGLE LOGIN
        // ---------------------------------------------------------
        [Fact]
        public void GoogleLogin_ShouldReturnChallenge()
        {
            var controller = CreateController();

            var result = controller.Google("/dashboard");
            var challenge = Assert.IsType<ChallengeResult>(result);

            Assert.Equal(GoogleDefaults.AuthenticationScheme, challenge.AuthenticationSchemes.Single());
            Assert.Contains("prompt", challenge.Properties.Items.Keys);
        }

        // ---------------------------------------------------------
        // MICROSOFT LOGIN
        // ---------------------------------------------------------
        [Fact]
        public void MicrosoftLogin_ShouldReturnChallenge()
        {
            var controller = CreateController();

            var result = controller.Microsoft("/test");
            var challenge = Assert.IsType<ChallengeResult>(result);

            Assert.Equal("Microsoft", challenge.AuthenticationSchemes.Single());
        }

        // ---------------------------------------------------------
        // EXTERNAL FAILURE
        // ---------------------------------------------------------
        [Fact]
        public async Task ExternalResponse_ShouldReturnBadRequest_WhenExternalFails()
        {
            var controller = CreateController(externalSucceeded: false);

            var result = await controller.ExternalResponse("/");

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("External authentication failed", bad.Value);
        }

        // ---------------------------------------------------------
        // MISSING CLAIMS
        // ---------------------------------------------------------
        [Fact]
        public async Task ExternalResponse_ShouldReturnBadRequest_WhenMissingClaims()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity()); // no claims

            var controller = CreateController(user);

            var result = await controller.ExternalResponse("/");

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid external data", bad.Value);
        }

        // ---------------------------------------------------------
        // NEW USER -> redirect with isNewUser
        // ---------------------------------------------------------
        [Fact]
        public async Task ExternalResponse_ShouldRedirect_WhenNewUser()
        {
            var claims = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "sub123"),
                new Claim(ClaimTypes.Email, "test@mail.com"),
                new Claim(ClaimTypes.Name, "Kira")
            });

            var user = new ClaimsPrincipal(claims);

            _authMock.Setup(a => a.SignInExternalAsync("Google", "sub123", "test@mail.com", "Kira"))
                     .ReturnsAsync(new ExternalSignInResultDto
                     {
                         IsNewUser = true,
                         UserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")
                     });

            var controller = CreateController(user);

            var result = await controller.ExternalResponse("/");

            var redirect = Assert.IsType<RedirectResult>(result);
            Assert.Contains("isNewUser=true", redirect.Url);
            Assert.Contains("userId=aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa", redirect.Url);
        }

        // ---------------------------------------------------------
        // EXISTING USER + TOKENS -> redirect with tokens
        // ---------------------------------------------------------
        [Fact]
        public async Task ExternalResponse_ShouldRedirectWithToken_WhenExistingUser()
        {
            var claims = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "sub1"),
                new Claim(ClaimTypes.Email, "e@mail.com"),
                new Claim(ClaimTypes.Name, "Kira")
            });

            var user = new ClaimsPrincipal(claims);

            _authMock.Setup(a => a.SignInExternalAsync("Google", "sub1", "e@mail.com", "Kira"))
                     .ReturnsAsync(new ExternalSignInResultDto
                     {
                         IsNewUser = false,
                         UserId = Guid.NewGuid(),
                         Tokens = new TokenResponseDto
                         {
                             AccessToken = "access123",
                             RefreshToken = "refresh123"
                         }
                     });

            var controller = CreateController(user);

            var result = await controller.ExternalResponse("/");

            var redirect = Assert.IsType<RedirectResult>(result);
            Assert.Contains("token=access123", redirect.Url);
            Assert.Contains("refresh=refresh123", redirect.Url);
        }

        // ---------------------------------------------------------
        // EXISTING USER without tokens -> simple redirect
        // ---------------------------------------------------------
        [Fact]
        public async Task ExternalResponse_ShouldRedirectWithoutToken_WhenNoTokens()
        {
            var claims = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "subX"),
                new Claim(ClaimTypes.Email, "xx@mail.com"),
                new Claim(ClaimTypes.Name, "Kira")
            });

            var user = new ClaimsPrincipal(claims);

            _authMock.Setup(a => a.SignInExternalAsync("Google", "subX", "xx@mail.com", "Kira"))
                     .ReturnsAsync(new ExternalSignInResultDto
                     {
                         IsNewUser = false,
                         UserId = Guid.NewGuid(),
                         Tokens = null
                     });

            var controller = CreateController(user);

            var result = await controller.ExternalResponse("/");

            var redirect = Assert.IsType<RedirectResult>(result);
            Assert.DoesNotContain("token=", redirect.Url);
        }

        // ---------------------------------------------------------
        // LOGOUT
        // ---------------------------------------------------------
        [Fact]
        public async Task Logout_ShouldRevokeRefresh_AndSignOut()
        {
            var controller = CreateController();

            var result = await controller.Logout("abc123");

            _authMock.Verify(a => a.RevokeRefreshTokenAsync("abc123"), Times.Once);

            var ok = Assert.IsType<OkObjectResult>(result);
            var dict = ok.Value!.GetType()
                .GetProperties()
                .ToDictionary(p => p.Name, p => p.GetValue(ok.Value));

            Assert.Equal("Logged out", dict["message"]);
        }
    }
}
