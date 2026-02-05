using AutoFixture;
using DTOs._3UserService;
using Gateway.Contracts.Users;
using Gateway.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace LMS.Tests.Gateway
{
    public class GatewayUsersControllerTests
    {
        private readonly Fixture _fixture;
        private readonly Mock<IHttpClientFactory> _mockFactory;
        private readonly Mock<HttpMessageHandler> _mockHandler;
        private readonly GatewayUsersController _controller;

        public GatewayUsersControllerTests()
        {
            _fixture = new Fixture();
            _mockHandler = new Mock<HttpMessageHandler>();
            _mockFactory = new Mock<IHttpClientFactory>();

            var client = new HttpClient(_mockHandler.Object)
            {
                BaseAddress = new Uri("http://user-service/")
            };

            _mockFactory.Setup(_ => _.CreateClient("UserService")).Returns(client);
            _controller = new GatewayUsersController(_mockFactory.Object);

            var user = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity());
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        private void SetupMockResponse(HttpStatusCode code, object content)
        {
            var response = new HttpResponseMessage(code)
            {
                Content = JsonContent.Create(content)
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);
        }

        // ------------------- USERS -------------------

        [Fact]
        public async Task GetAllUsers_HitsOkBranch_WhenTypeMatches()
        {
            // Arrange
            var users = _fixture.Create<List<UserDto>>();
            SetupMockResponse(HttpStatusCode.OK, users);

            // Act
            var result = await _controller.GetAllUsers();

            // Assert
            var objectResult = Assert.IsAssignableFrom<ObjectResult>(result.Result);
            Assert.Equal(200, objectResult.StatusCode);

            var actualJson = JsonSerializer.Serialize(objectResult.Value);
            Assert.Contains(users[0].Email, actualJson);
            Assert.Contains(users[0].Name, actualJson);
        }

        [Fact]
        public async Task GetAllUsers_ReturnsResultDirectly_WhenTypeDoesNotMatch()
        {
            // Hits the "return result as ActionResult" by returning something other than List<UserDto>
            SetupMockResponse(HttpStatusCode.NotFound, "Not Found");

            var result = await _controller.GetAllUsers();

            var objectResult = Assert.IsAssignableFrom<ObjectResult>(result.Result);
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetUser_HitsOkObjectBranch()
        {
            // Arrange
            var user = _fixture.Create<UserDto>();
            SetupMockResponse(HttpStatusCode.OK, user);

            // Act
            var result = await _controller.GetUser(_fixture.Create<UserIdRequest>());

            // Assert
            var objectResult = Assert.IsAssignableFrom<ObjectResult>(result.Result);
            Assert.Equal(200, objectResult.StatusCode);

            var actualJson = JsonSerializer.Serialize(objectResult.Value);
            Assert.Contains(user.Email, actualJson);
            Assert.Contains(user.Id.ToString(), actualJson);
        }

        [Fact]
        public async Task CompleteProfile_ReturnsResult()
        {
            // Coverage for the one-liner arrow function
            SetupMockResponse(HttpStatusCode.OK, new { success = true });

            var result = await _controller.CompleteProfile(_fixture.Create<CompleteProfileRequest>());

            var objectResult = Assert.IsAssignableFrom<ObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
        }

        // ------------------- ROLES -------------------

        [Fact]
        public async Task GetAllRoles_ReturnsOk()
        {
            var roles = _fixture.Create<List<Roleresponse>>();
            SetupMockResponse(HttpStatusCode.OK, roles);

            var result = await _controller.GetAllRoles();

            var objectResult = Assert.IsAssignableFrom<ObjectResult>(result.Result);
            Assert.Equal(200, objectResult.StatusCode);
        }

        [Fact]
        public async Task UpdateUserRole_ReturnsResult()
        {
            SetupMockResponse(HttpStatusCode.OK, new { success = true });

            var result = await _controller.UpdateUserRole(_fixture.Create<UpdateUserRoleRequest>());

            var objectResult = Assert.IsAssignableFrom<ObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
        }

        // ------------------- EXCEPTIONS (The 100% Coverage Secret) -------------------

        [Theory]
        [InlineData("GetAllUsers")]
        [InlineData("GetUser")]
        [InlineData("GetAllRoles")]
        public async Task ExceptionPath_Returns500(string method)
        {
            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exception("Fail"));

            ActionResult finalResult;
            if (method == "GetAllUsers") finalResult = (await _controller.GetAllUsers()).Result;
            else if (method == "GetUser") finalResult = (await _controller.GetUser(_fixture.Create<UserIdRequest>())).Result;
            else finalResult = (await _controller.GetAllRoles()).Result;

            var objectResult = Assert.IsType<ObjectResult>(finalResult);
            Assert.Equal(500, objectResult.StatusCode);
        }
    }
}