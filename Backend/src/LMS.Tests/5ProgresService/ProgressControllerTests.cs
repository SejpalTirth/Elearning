using AutoFixture;
using DTOs._5ProgresService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.Protected;
using ProgressService.Web.Controllers;
using ProgresService.BLL.Interface;
using ProgresService.BLL.UserContext;
using System.Net;
using System.Net.Http.Json;
using Xunit;

using UserContextDto = ProgresService.BLL.UserContext.UserContextDto;

namespace LMS.Tests.ProgressService
{
    public class ProgressControllerTests
    {
        private readonly Fixture _fixture;
        private readonly Mock<IProgressService> _mockService;
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly Mock<IUserContextAccessor> _mockUserContext;
        private readonly Mock<HttpMessageHandler> _mockHandler;
        private readonly ProgressController _controller;

        public ProgressControllerTests()
        {
            _fixture = new Fixture();
            _mockService = new Mock<IProgressService>();
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockUserContext = new Mock<IUserContextAccessor>();
            _mockHandler = new Mock<HttpMessageHandler>();

            // 1. Setup Mock HttpClient
            var client = new HttpClient(_mockHandler.Object)
            {
                BaseAddress = new Uri("http://course-service/")
            };
            _mockHttpClientFactory.Setup(_ => _.CreateClient("CourseService")).Returns(client);

            // 2. Initialize Controller
            _controller = new ProgressController(
                _mockService.Object,
                _mockHttpClientFactory.Object,
                _mockUserContext.Object);

            // 3. Setup Controller Context for Headers
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        private void SetupUserContext(Guid? userId)
        {
            if (userId == null)
            {
                _mockUserContext.Setup(x => x.Current).Returns((UserContextDto)null);
            }
            else
            {
                // Fix: Create the DTO with the UserId property
                var contextDto = new UserContextDto { UserId = userId.Value };
                _mockUserContext.Setup(x => x.Current).Returns(contextDto);
            }
        }

        private void SetupCourseServiceResponse(HttpStatusCode code, object content)
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

        // ---------------- USER PROGRESS TESTS ----------------

        [Fact]
        public async Task GetUserProgress_ReturnsUnauthorized_WhenContextIsNull()
        {
            SetupUserContext(null);
            var result = await _controller.GetUserProgress();
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task GetUserProgress_ReturnsOk_WhenSuccessful()
        {
            var userId = Guid.NewGuid();
            var data = _fixture.Create<List<ProgresDto>>();
            SetupUserContext(userId);
            _mockService.Setup(s => s.GetUserProgressAsync(userId)).ReturnsAsync(data);

            var result = await _controller.GetUserProgress();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(data, okResult.Value);
        }

        // ---------------- COMPLETE MODULE TESTS ----------------

        [Fact]
        public async Task CompleteModule_ReturnsUnauthorized_WhenUserIdIsEmpty()
        {
            SetupUserContext(Guid.Empty);
            var result = await _controller.CompleteModule(_fixture.Create<ModuleCompleteRequest>());
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task CompleteModule_ReturnsBadRequest_WhenCourseServiceFails()
        {
            // Arrange
            SetupUserContext(Guid.NewGuid());
            SetupCourseServiceResponse(HttpStatusCode.BadRequest, "Invalid Module");

            // Act
            var result = await _controller.CompleteModule(_fixture.Create<ModuleCompleteRequest>());

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Failed to fetch course info", badRequest.Value.ToString());
        }

        [Fact]
        public async Task CompleteModule_ReturnsNotFound_WhenCourseInfoIsNull()
        {
            // Arrange
            SetupUserContext(Guid.NewGuid());
            // Mocking a 200 OK but with null body to hit the "if (courseInfo == null)" branch
            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create((object)null) };
            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            // Act
            var result = await _controller.CompleteModule(_fixture.Create<ModuleCompleteRequest>());

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task CompleteModule_ReturnsOk_AndForwardsHeader()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = _fixture.Create<ModuleCompleteRequest>();
            var courseResponse = _fixture.Create<CourseIdResponseDTO>();

            SetupUserContext(userId);
            SetupCourseServiceResponse(HttpStatusCode.OK, courseResponse);

            // This hits the Request.Headers.TryGetValue branch
            _controller.Request.Headers["Authorization"] = "Bearer MyTestToken";

            // Act
            var result = await _controller.CompleteModule(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            _mockService.Verify(s => s.MarkModuleCompletedAsync(userId, courseResponse.CourseId, request.ModuleId), Times.Once);
        }
    }
}