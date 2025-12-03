using Gateway.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace LMS.Tests.Gateway
{
    public class GatewayCourseControllerTests
    {
        private readonly Mock<IHttpClientFactory> _factoryMock;
        private readonly Mock<HttpMessageHandler> _handlerMock;
        private readonly HttpClient _client;

        public GatewayCourseControllerTests()
        {
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
            var controller = new GatewayCourseController(_factoryMock.Object);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = ctx ?? new DefaultHttpContext()
            };

            return controller;
        }

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

        // -------------------------------------------------------------
        // GET ALL
        // -------------------------------------------------------------
        [Fact]
        public async Task GetAll_ShouldForwardTo_CourseService()
        {
            SetupJsonResponse("[{\"id\":1}]");

            var controller = CreateController();

            var result = await controller.GetAll();
            var content = Assert.IsType<ContentResult>(result);

            Assert.Equal("[{\"id\":1}]", content.Content);
        }

        // -------------------------------------------------------------
        // GET by ID
        // -------------------------------------------------------------
        [Fact]
        public async Task Get_ShouldForwardCorrectUrl()
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

        // -------------------------------------------------------------
        // CREATE
        // -------------------------------------------------------------
        [Fact]
        public async Task Create_ShouldSendPost_WithJsonBody()
        {
            SetupJsonResponse("{\"ok\":true}");

            var controller = CreateController();
            var dto = new { title = "abc" };

            var result = await controller.Create(dto);
            var content = Assert.IsType<ContentResult>(result);

            Assert.Equal("{\"ok\":true}", content.Content);

            _handlerMock.Protected()
                .Verify("SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.Content != null),
                    ItExpr.IsAny<CancellationToken>());
        }

        // -------------------------------------------------------------
        // UPDATE
        // -------------------------------------------------------------
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

        // -------------------------------------------------------------
        // NON-JSON RESPONSE
        // -------------------------------------------------------------
        [Fact]
        public async Task Forward_ShouldWrapNonJson_InMessageField()
        {
            SetupTextResponse("Course not found", HttpStatusCode.NotFound);

            var controller = CreateController();

            var result = await controller.GetById(999);
            var bad = Assert.IsType<ObjectResult>(result);

            var dict = bad.Value!.GetType()
                .GetProperties()
                .ToDictionary(p => p.Name, p => p.GetValue(bad.Value));

            Assert.Equal("Course not found", dict["message"]);
            Assert.Equal(404, bad.StatusCode);
        }

        // -------------------------------------------------------------
        // AUTH HEADER FORWARDING
        // -------------------------------------------------------------
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

        // -------------------------------------------------------------
        // Delete
        // -------------------------------------------------------------
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
    }
}
