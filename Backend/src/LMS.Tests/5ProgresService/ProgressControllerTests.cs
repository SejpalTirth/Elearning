using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using Moq;
using DTOs._5ProgresService;
using ProgresService.BLL.Interface;
using ProgresService.BLL.Models;
using ProgresService.BLL.UserContext;
using ProgressService.Web.Controllers;
using System.Net;
using System.Text;
using System.Text.Json;

namespace LMS.Tests.ProgressService
{
    public class ProgressControllerTests : BaseTest
    {
        private readonly Mock<IProgressService> _serviceMock;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<IUserContextAccessor> _userContextAccessorMock;
        private readonly IFixture _fixture;

        public ProgressControllerTests()
        {
            _serviceMock = new Mock<IProgressService>();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _userContextAccessorMock = new Mock<IUserContextAccessor>();

            _fixture = new Fixture();

            _fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));

            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        // ---------------------------------------------------------
        // Fake Http Handler
        // ---------------------------------------------------------
        private class FakeHttpHandler : HttpMessageHandler
        {
            public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
            public object? Response { get; set; }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                var message = new HttpResponseMessage(StatusCode);

                if (Response != null)
                {
                    var json = JsonSerializer.Serialize(Response);
                    message.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                return Task.FromResult(message);
            }
        }

        private ProgressController CreateController(
            HttpClient? httpClient = null,
            Guid? userId = null)
        {
            if (httpClient != null)
            {
                _httpClientFactoryMock
                    .Setup(f => f.CreateClient("CourseService"))
                    .Returns(httpClient);
            }

            if (userId.HasValue)
            {
                _userContextAccessorMock
                    .Setup(u => u.Current)
                    .Returns(new UserContextDto { UserId = userId.Value });
            }
            else
            {
                _userContextAccessorMock
                    .Setup(u => u.Current)
                    .Returns((UserContextDto?)null);
            }

            return new ProgressController(
                _serviceMock.Object,
                _httpClientFactoryMock.Object,
                _userContextAccessorMock.Object);
        }

        // ---------------------------------------------------------
        // TEST 1: GetUserProgress → OK
        // ---------------------------------------------------------
        [Fact]
        public async Task GetUserProgress_ShouldReturnOk_WhenUserContextIsValid()
        {
            var userId = Guid.NewGuid();

            var progress = new List<ProgresDto>
            {
                new ProgresDto
                {
                    CourseId = 1,
                    ModuleId = 10,
                    IsCompleted = true
                }
            };

            _serviceMock
                .Setup(s => s.GetUserProgressAsync(userId))
                .ReturnsAsync(progress);

            var controller = CreateController(userId: userId);

            var result = await controller.GetUserProgress();

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(progress, ok.Value);
        }

        // ---------------------------------------------------------
        // TEST 2: GetUserProgress → Unauthorized
        // ---------------------------------------------------------
        [Fact]
        public async Task GetUserProgress_ShouldReturnUnauthorized_WhenUserContextMissing()
        {
            var controller = CreateController();

            var result = await controller.GetUserProgress();

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        // ---------------------------------------------------------
        // TEST 3: CompleteModule → CourseService failure
        // ---------------------------------------------------------
        [Fact]
        public async Task CompleteModule_ShouldReturnBadRequest_WhenCourseServiceFails()
        {
            var handler = new FakeHttpHandler
            {
                StatusCode = HttpStatusCode.BadRequest
            };

            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://fake/")
            };

            var controller = CreateController(client, Guid.NewGuid());

            var request = new ModuleCompleteRequest { ModuleId = 10 };

            var result = await controller.CompleteModule(request);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        // ---------------------------------------------------------
        // TEST 4: CompleteModule → CourseService returns null
        // ---------------------------------------------------------
        [Fact]
        public async Task CompleteModule_ShouldReturnNotFound_WhenCourseServiceReturnsNull()
        {
            var handler = new FakeHttpHandler
            {
                Response = null
            };

            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://fake/")
            };

            var controller = CreateController(client, Guid.NewGuid());

            var request = new ModuleCompleteRequest { ModuleId = 99 };

            var result = await controller.CompleteModule(request);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        // ---------------------------------------------------------
        // TEST 5: CompleteModule → Success
        // ---------------------------------------------------------
        [Fact]
        public async Task CompleteModule_ShouldCallService_AndReturnOk()
        {
            var userId = Guid.NewGuid();

            var handler = new FakeHttpHandler
            {
                Response = new CourseIdResponseDTO
                {
                    CourseId = 5
                }
            };

            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://fake/")
            };

            var controller = CreateController(client, userId);

            var request = new ModuleCompleteRequest { ModuleId = 10 };

            var result = await controller.CompleteModule(request);

            Assert.IsType<OkObjectResult>(result);

            _serviceMock.Verify(s =>
                s.MarkModuleCompletedAsync(userId, 5, 10),
                Times.Once);
        }

        // ---------------------------------------------------------
        // TEST 6: CompleteModule → Unauthorized
        // ---------------------------------------------------------
        [Fact]
        public async Task CompleteModule_ShouldReturnUnauthorized_WhenUserContextMissing()
        {
            var controller = CreateController();

            var request = new ModuleCompleteRequest { ModuleId = 10 };

            var result = await controller.CompleteModule(request);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }
    }
}
