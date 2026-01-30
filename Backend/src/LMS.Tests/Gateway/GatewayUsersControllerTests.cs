using AutoFixture;
using Gateway.Controllers;
using Gateway.Contracts.Users;
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

            _handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            _client = new HttpClient(_handlerMock.Object)
            {
                BaseAddress = new Uri("http://fake-user-service/")
            };

            _factoryMock = new Mock<IHttpClientFactory>();
            _factoryMock.Setup(f => f.CreateClient("UserService"))
                        .Returns(_client);
        }

        private GatewayUsersController CreateController()
        {
            return new GatewayUsersController(_factoryMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        private void SetupJsonResponse(string json, HttpStatusCode code = HttpStatusCode.OK)
        {
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = code,
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                });
        }

        // ------------------- COMPLETE PROFILE -------------------
        [Fact]
        public async Task CompleteProfile_ShouldPostCorrectUrl()
        {
            SetupJsonResponse("{\"completed\":true}");

            var controller = CreateController();
            var dto = new CompleteProfileRequest
            {
                UserId = Guid.NewGuid(),
                Name = "John Doe",
                Role = "Admin"
            };

            var result = await controller.CompleteProfile(dto);
            var content = Assert.IsType<ContentResult>(result);

            Assert.Equal("{\"completed\":true}", content.Content);

            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.ToString().EndsWith("/api/users/complete-profile")),
                ItExpr.IsAny<CancellationToken>());
        }

        // ------------------- GET USER -------------------
        [Fact]
        public async Task GetUser_ShouldPostCorrectUrl()
        {
            SetupJsonResponse("{\"userId\":\"12345\"}");

            var controller = CreateController();
            var dto = new UserIdRequest
            {
                UserId = Guid.NewGuid()
            };

            var result = await controller.GetUser(dto);
            var content = Assert.IsType<ContentResult>(result);

            Assert.Equal("{\"userId\":\"12345\"}", content.Content);

            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.ToString().EndsWith("/api/users/by-id")),
                ItExpr.IsAny<CancellationToken>());
        }

        // ------------------- UPDATE USER ROLE -------------------
        [Fact]
        public async Task UpdateUserRole_ShouldPostCorrectUrl()
        {
            SetupJsonResponse("{\"updated\":true}");

            var controller = CreateController();
            var dto = new UpdateUserRoleRequest
            {
                UserId = Guid.NewGuid(),
                RoleId = 2 // Example role ID
            };

            var result = await controller.UpdateUserRole(dto);
            var content = Assert.IsType<ContentResult>(result);

            Assert.Equal("{\"updated\":true}", content.Content);

            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.ToString().EndsWith("/api/roles/update")),
                ItExpr.IsAny<CancellationToken>());
        }

        // ------------------- DELETE USER -------------------
        [Fact]
        public async Task DeleteUser_ShouldPostCorrectUrl()
        {
            SetupJsonResponse("{\"deleted\":true}");

            var controller = CreateController();
            var dto = new UserIdRequest
            {
                UserId = Guid.NewGuid()
            };

            var result = await controller.DeleteUser(dto);
            var content = Assert.IsType<ContentResult>(result);

            Assert.Equal("{\"deleted\":true}", content.Content);

            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.ToString().EndsWith("/api/users/delete")),
                ItExpr.IsAny<CancellationToken>());
        }

        // ------------------- GET ALL USERS -------------------
        [Fact]
        public async Task GetAllUsers_ShouldPostCorrectUrl()
        {
            SetupJsonResponse("[{\"userId\":\"12345\", \"name\":\"John\"}]");

            var controller = CreateController();

            var result = await controller.GetAllUsers();
            var content = Assert.IsType<ContentResult>(result);

            Assert.Equal("[{\"userId\":\"12345\", \"name\":\"John\"}]", content.Content);

            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.ToString().EndsWith("/api/users/all")),
                ItExpr.IsAny<CancellationToken>());
        }
    }
}
