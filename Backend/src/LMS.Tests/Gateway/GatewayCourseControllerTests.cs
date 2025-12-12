using AutoFixture;
using Gateway.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;
using Xunit;

namespace LMS.Tests.Gateway
{
    public class GatewayCourseControllerTests
    {
        private readonly Fixture _fixture;

        private readonly Mock<IHttpClientFactory> _factoryMock;
        private readonly Mock<HttpMessageHandler> _handlerMock;
        private readonly HttpClient _client;

        public GatewayCourseControllerTests()
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
                BaseAddress = new Uri("http://fake-course-service/")
            };

            _factoryMock = new Mock<IHttpClientFactory>();
            _factoryMock.Setup(f => f.CreateClient("CourseService"))
                        .Returns(_client);
        }

        private GatewayCourseController CreateController(HttpContext? ctx = null)
        {
            return new GatewayCourseController(_factoryMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = ctx ?? new DefaultHttpContext()
                }
            };
        }

        // -------------------------------------------------------------------
        // Utilities for Response Setup
        // -------------------------------------------------------------------
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

        // -------------------------------------------------------------------
        // GET ALL
        // -------------------------------------------------------------------
        [Fact]
        public async Task GetAll_ShouldForwardTo_CourseService()
        {
            SetupJsonResponse("[{\"id\":1}]");

            var controller = CreateController();

            var result = await controller.GetAll();
            var content = Assert.IsType<ContentResult>(result);

            Assert.Equal("[{\"id\":1}]", content.Content);
            Assert.Equal("application/json", content.ContentType);
        }

        // -------------------------------------------------------------------
        // GET by ID
        // -------------------------------------------------------------------
        [Fact]
        public async Task GetById_ShouldForwardCorrectUrl()
        {
            SetupJsonResponse("{\"id\":5}");

            var controller = CreateController();

            var result = await controller.GetById(5);
            var content = Assert.IsType<ContentResult>(result);

            Assert.Equal("{\"id\":5}", content.Content);

            _handlerMock.Protected()
                .Verify("SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString().EndsWith("api/courses/5")),
                    ItExpr.IsAny<CancellationToken>());
        }

        // -------------------------------------------------------------------
        // CREATE
        // -------------------------------------------------------------------
        [Fact]
        public async Task Create_ShouldSendPost_WithJsonBody()
        {
            SetupJsonResponse("{\"ok\":true}");

            var controller = CreateController();
            var dto = new { title = "New Course" };

            var result = await controller.Create(dto);
            var content = Assert.IsType<ContentResult>(result);

            Assert.Equal("{\"ok\":true}", content.Content);

            _handlerMock.Protected()
                .Verify("SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri!.ToString().EndsWith("api/courses") &&
                        req.Content != null),
                    ItExpr.IsAny<CancellationToken>());
        }

        // -------------------------------------------------------------------
        // UPDATE
        // -------------------------------------------------------------------
        [Fact]
        public async Task Update_ShouldSendPut_ToCorrectUrl()
        {
            SetupJsonResponse("{\"updated\":true}");

            var controller = CreateController();
            var dto = new { title = "updated" };

            var result = await controller.Update(10, dto);
            var content = Assert.IsType<ContentResult>(result);

            Assert.Equal("{\"updated\":true}", content.Content);

            _handlerMock.Protected()
                .Verify("SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Put &&
                        req.RequestUri!.ToString().EndsWith("api/courses/10")),
                    ItExpr.IsAny<CancellationToken>());
        }

        // -------------------------------------------------------------------
        // DELETE
        // -------------------------------------------------------------------
        [Fact]
        public async Task Delete_ShouldCallCorrectUrl()
        {
            SetupJsonResponse("{\"deleted\":true}");

            var controller = CreateController();

            var result = await controller.Delete(7);
            var content = Assert.IsType<ContentResult>(result);

            Assert.Equal("{\"deleted\":true}", content.Content);

            _handlerMock.Protected()
                .Verify("SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Delete &&
                        req.RequestUri!.ToString().EndsWith("api/courses/7")),
                    ItExpr.IsAny<CancellationToken>());
        }

        // -------------------------------------------------------------------
        // NON-JSON RESPONSE WRAPPING
        // -------------------------------------------------------------------
        [Fact]
        public async Task Forward_ShouldWrapNonJson_InMessageField()
        {
            SetupTextResponse("Course not found", HttpStatusCode.NotFound);

            var controller = CreateController();

            var result = await controller.GetById(999);
            var bad = Assert.IsType<ObjectResult>(result);

            var value = bad.Value!;
            var messageProp = value.GetType().GetProperty("message")!;
            var message = messageProp.GetValue(value) as string;

            Assert.Equal("Course not found", message);
            Assert.Equal(404, bad.StatusCode);
        }

        // -------------------------------------------------------------------
        // AUTH HEADER FORWARDING
        // -------------------------------------------------------------------
        [Fact]
        public async Task Forward_ShouldIncludeAuthorizationHeader()
        {
            SetupJsonResponse("{\"ok\":true}");

            var ctx = new DefaultHttpContext();
            ctx.Request.Headers.Authorization = "Bearer test-token";

            var controller = CreateController(ctx);

            await controller.GetAll();

            _handlerMock.Protected()
                .Verify("SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Headers.Authorization!.ToString() == "Bearer test-token"),
                    ItExpr.IsAny<CancellationToken>());
        }

        // -------------------------------------------------------------------
        // GET BY INSTRUCTOR
        // -------------------------------------------------------------------
        [Fact]
        public async Task GetByInstructor_ShouldForwardCorrectUrl()
        {
            SetupJsonResponse("[{\"id\":1}]");

            var controller = CreateController();
            var instructorId = Guid.NewGuid();

            var result = await controller.GetByInstructor(instructorId);
            var content = Assert.IsType<ContentResult>(result);

            Assert.Equal("application/json", content.ContentType);

            _handlerMock.Protected()
                .Verify("SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString().EndsWith($"api/courses/instructor/{instructorId}")),
                    ItExpr.IsAny<CancellationToken>());
        }

        // -------------------------------------------------------------------
        // GET UNFINISHED COURSE
        // -------------------------------------------------------------------
        [Fact]
        public async Task GetUnfinishedCourse_ShouldForwardCorrectUrl()
        {
            SetupJsonResponse("{\"id\":3}");

            var controller = CreateController();
            var instructorId = Guid.NewGuid();

            var result = await controller.GetUnfinishedCourse(instructorId);
            var content = Assert.IsType<ContentResult>(result);

            Assert.Equal("{\"id\":3}", content.Content);

            _handlerMock.Protected()
                .Verify("SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString().EndsWith($"api/courses/unfinished/{instructorId}")),
                    ItExpr.IsAny<CancellationToken>());
        }

        // -------------------------------------------------------------------
        // CONTINUE COURSE
        // -------------------------------------------------------------------
        [Fact]
        public async Task Continue_ShouldForwardPost_ToCorrectUrl()
        {
            SetupJsonResponse("{\"ok\":true}");

            var controller = CreateController();

            var result = await controller.Continue(15);
            var content = Assert.IsType<ContentResult>(result);

            Assert.Equal("{\"ok\":true}", content.Content);

            _handlerMock.Protected()
                .Verify("SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri!.ToString().EndsWith("api/courses/continue/15")),
                    ItExpr.IsAny<CancellationToken>());
        }

        // -------------------------------------------------------------------
        // ENROLL
        // -------------------------------------------------------------------
        [Fact]
        public async Task Enroll_ShouldPostToEnrollEndpoint()
        {
            SetupJsonResponse("{\"enrolled\":true}");

            var controller = CreateController();
            var dto = new { courseId = 1, userId = "U1" };

            var result = await controller.Enroll(dto);
            var content = Assert.IsType<ContentResult>(result);

            Assert.Equal("{\"enrolled\":true}", content.Content);

            _handlerMock.Protected()
                .Verify("SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri!.ToString().EndsWith("api/courses/enroll") &&
                        req.Content != null),
                    ItExpr.IsAny<CancellationToken>());
        }

        // -------------------------------------------------------------------
        // GET ENROLLED COURSES
        // -------------------------------------------------------------------
        [Fact]
        public async Task GetEnrolled_ShouldCallEnrolledEndpoint()
        {
            SetupJsonResponse("[{\"id\":1}]");

            var controller = CreateController();
            var userId = "user-123";

            var result = await controller.GetEnrolled(userId);
            var content = Assert.IsType<ContentResult>(result);

            Assert.Equal("[{\"id\":1}]", content.Content);

            _handlerMock.Protected()
                .Verify("SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString().EndsWith($"api/courses/enrolled/{userId}")),
                    ItExpr.IsAny<CancellationToken>());
        }

        // -------------------------------------------------------------------
        // GET MODULES FOR COURSE
        // -------------------------------------------------------------------
        [Fact]
        public async Task GetModules_ShouldCallCorrectUrl()
        {
            SetupJsonResponse("[{\"id\":10}]");

            var controller = CreateController();

            var result = await controller.GetModules(20);
            var content = Assert.IsType<ContentResult>(result);

            Assert.Equal("[{\"id\":10}]", content.Content);

            _handlerMock.Protected()
                .Verify("SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString().EndsWith("api/courses/20/modules")),
                    ItExpr.IsAny<CancellationToken>());
        }

        // -------------------------------------------------------------------
        // GET SINGLE MODULE
        // -------------------------------------------------------------------
        [Fact]
        public async Task GetModule_ShouldForwardToModulesController()
        {
            SetupJsonResponse("{\"id\":9}");

            var controller = CreateController();

            var result = await controller.GetModule(9);
            var content = Assert.IsType<ContentResult>(result);

            Assert.Equal("{\"id\":9}", content.Content);

            _handlerMock.Protected()
                .Verify("SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString().EndsWith("api/modules/9")),
                    ItExpr.IsAny<CancellationToken>());
        }

        // -------------------------------------------------------------------
        // GET CATEGORIES
        // -------------------------------------------------------------------
        [Fact]
        public async Task GetCategories_ShouldCallCategoriesEndpoint()
        {
            SetupJsonResponse("[{\"id\":1,\"name\":\"Backend\"}]");

            var controller = CreateController();

            var result = await controller.GetCategories();
            var content = Assert.IsType<ContentResult>(result);

            Assert.Equal("[{\"id\":1,\"name\":\"Backend\"}]", content.Content);

            _handlerMock.Protected()
                .Verify("SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString().EndsWith("api/categories")),
                    ItExpr.IsAny<CancellationToken>());
        }

        // -------------------------------------------------------------------
        // PUBLISH COURSE
        // -------------------------------------------------------------------
        [Fact]
        public async Task PublishCourse_ShouldCallPublishEndpoint()
        {
            SetupJsonResponse("{\"published\":true}");

            var controller = CreateController();

            var result = await controller.PublishCourse(88);
            var content = Assert.IsType<ContentResult>(result);

            Assert.Equal("{\"published\":true}", content.Content);

            _handlerMock.Protected()
                .Verify("SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri!.ToString().EndsWith("api/courses/88/publish")),
                    ItExpr.IsAny<CancellationToken>());
        }
    }
}
