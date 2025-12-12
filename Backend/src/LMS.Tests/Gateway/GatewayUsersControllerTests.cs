using AutoFixture;
using Gateway.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;

namespace LMS.Tests.Gateway
{
    public class GatewayUsersControllerTests
    {
        private readonly Fixture _fixture;

        private readonly Mock<IHttpClientFactory> _factoryMock;
        private readonly Mock<HttpMessageHandler> _handlerMock;
        private readonly HttpClient _client;

        public GatewayUsersControllerTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));

            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            // Mock HTTP
            _handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            _client = new HttpClient(_handlerMock.Object)
            {
                BaseAddress = new Uri("http://fake-user-service/")
            };

            _factoryMock = new Mock<IHttpClientFactory>();
            _factoryMock.Setup(f => f.CreateClient("UserService"))
                        .Returns(_client);
        }

        private GatewayUsersController CreateController(HttpContext? context = null)
        {
            return new GatewayUsersController(_factoryMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = context ?? new DefaultHttpContext()
                }
            };
        }

        // --------------------------------------------
        // Helpers for setting mock responses
        // --------------------------------------------
        private void SetupJsonResponse(string json, HttpStatusCode status = HttpStatusCode.OK)
        {
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = status,
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                });
        }

        private void SetupTextResponse(string text, HttpStatusCode status = HttpStatusCode.BadRequest)
        {
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = status,
                    Content = new StringContent(text)
                });
        }

        // ======================================================
        // USERS ROUTES
        // ======================================================

        [Fact]
        public async Task GetAllUsers_ShouldForwardRequest()
        {
            SetupJsonResponse("[{\"id\":1}]");

            var controller = CreateController();

            var result = await controller.GetAllUsers();
            var content = Assert.IsType<ContentResult>(result);

            Assert.Equal("[{\"id\":1}]", content.Content);

            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().EndsWith("api/users")),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task GetUser_ShouldHitCorrectEndpoint()
        {
            SetupJsonResponse("{\"id\":555}");

            var controller = CreateController();
            var id = Guid.NewGuid();

            var result = await controller.GetUser(id);
            var content = Assert.IsType<ContentResult>(result);

            Assert.Equal("{\"id\":555}", content.Content);

            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().EndsWith($"api/users/{id}")),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task DeleteUser_ShouldCallDeleteOnCorrectUrl()
        {
            SetupJsonResponse("{\"deleted\":true}");

            var controller = CreateController();
            var id = Guid.NewGuid();

            var result = await controller.DeleteUser(id);
            var content = Assert.IsType<ContentResult>(result);

            Assert.Equal("{\"deleted\":true}", content.Content);

            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Delete &&
                    req.RequestUri!.ToString().EndsWith($"api/users/{id}")),
                ItExpr.IsAny<CancellationToken>());
        }

        // ======================================================
        // ROLES ROUTES
        // ======================================================

        [Fact]
        public async Task GetAllRoles_ShouldForwardToRoleService()
        {
            SetupJsonResponse("[{\"id\":1,\"name\":\"Admin\"}]");

            var controller = CreateController();

            var result = await controller.GetAllRoles();
            var content = Assert.IsType<ContentResult>(result);

            Assert.Equal("[{\"id\":1,\"name\":\"Admin\"}]", content.Content);
        }

        [Fact]
        public async Task GetUserRole_ShouldCallCorrectUrl()
        {
            SetupJsonResponse("[\"Admin\"]");

            var controller = CreateController();
            var id = Guid.NewGuid();

            var result = await controller.GetUserRole(id);
            var content = Assert.IsType<ContentResult>(result);

            Assert.Equal("[\"Admin\"]", content.Content);

            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().EndsWith($"api/roles/{id}")),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task UpdateUserRole_ShouldSendPutRequest()
        {
            SetupJsonResponse("{\"updated\":true}");

            var controller = CreateController();
            var dto = new { roleId = 2 };

            var result = await controller.UpdateUserRole(dto);
            var content = Assert.IsType<ContentResult>(result);

            Assert.Equal("{\"updated\":true}", content.Content);

            _handlerMock.Protected()
                .Verify("SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Put &&
                        req.RequestUri!.ToString().EndsWith("api/roles/update") &&
                        req.Content != null),
                    ItExpr.IsAny<CancellationToken>());
        }

        // ======================================================
        // NON-JSON RESPONSES
        // ======================================================

        [Fact]
        public async Task Forward_ShouldWrapNonJson_AsMessage()
        {
            SetupTextResponse("User not found", HttpStatusCode.NotFound);

            var controller = CreateController();

            var result = await controller.GetUser(Guid.NewGuid());
            var obj = Assert.IsType<ObjectResult>(result);

            Assert.Equal(404, obj.StatusCode);

            var msg = obj.Value!.GetType().GetProperty("message")!.GetValue(obj.Value);
            Assert.Equal("User not found", msg);
        }

        // ======================================================
        // AUTH HEADER COPYING
        // ======================================================

        [Fact]
        public async Task Forward_ShouldCopyAuthorizationHeader()
        {
            SetupJsonResponse("{\"ok\":true}");

            var ctx = new DefaultHttpContext();
            ctx.Request.Headers.Authorization = "Bearer abc123";

            var controller = CreateController(ctx);

            await controller.GetAllUsers();

            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Headers.Authorization!.ToString() == "Bearer abc123"),
                ItExpr.IsAny<CancellationToken>());
        }
    }
}
