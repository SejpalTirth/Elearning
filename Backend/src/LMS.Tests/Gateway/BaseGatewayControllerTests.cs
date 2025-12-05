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
    public class BaseGatewayControllerTests
    {
        // --------------------------------------------------------------------
        // Local test controller that exposes BaseGatewayController methods
        // --------------------------------------------------------------------
        private class TestGatewayController : BaseGatewayController
        {
            public TestGatewayController(HttpClient client) : base(client) { }

            public Task<IActionResult> TestGet(string url) => ForwardGet(url);
            public Task<IActionResult> TestPost(string url, object body) => ForwardPost(url, body);
            public Task<IActionResult> TestPut(string url, object body) => ForwardPut(url, body);
            public Task<IActionResult> TestDelete(string url) => ForwardDelete(url);
        }

        private readonly Fixture _fixture;
        private Mock<HttpMessageHandler> _handlerMock = null!;

        public BaseGatewayControllerTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        // --------------------------------------------------------------------
        // Helper: Creates an HttpClient that returns a specific response
        // --------------------------------------------------------------------
        private HttpClient CreateHttpClientReturning(
            HttpResponseMessage response,
            Action<HttpRequestMessage>? capture = null)
        {
            _handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync((HttpRequestMessage req, CancellationToken _) =>
                {
                    capture?.Invoke(req);
                    return response;
                });

            return new HttpClient(_handlerMock.Object)
            {
                BaseAddress = new Uri("https://fake/")
            };
        }

        private TestGatewayController CreateController(HttpClient client)
        {
            return new TestGatewayController(client)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        // --------------------------------------------------------------------
        // SUCCESS RESPONSE
        // --------------------------------------------------------------------
        [Fact]
        public async Task ForwardGet_ShouldReturnJsonContent_WhenSuccess()
        {
            var json = "{\"ok\":true}";

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var client = CreateHttpClientReturning(response);
            var controller = CreateController(client);

            var result = await controller.TestGet("api/test");

            var content = Assert.IsType<ContentResult>(result);
            Assert.Equal("application/json", content.ContentType);
            Assert.Equal(json, content.Content);
        }

        // --------------------------------------------------------------------
        // ERROR RESPONSE
        // --------------------------------------------------------------------
        [Fact]
        public async Task ForwardGet_ShouldReturnStatusCode_WhenError()
        {
            var body = "Something went wrong";

            var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(body)
            };

            var client = CreateHttpClientReturning(response);
            var controller = CreateController(client);

            var result = await controller.TestGet("api/error");

            var status = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.BadRequest, status.StatusCode);
            Assert.Equal(body, status.Value);
        }

        // --------------------------------------------------------------------
        // POST forwards body
        // --------------------------------------------------------------------
        [Fact]
        public async Task ForwardPost_ShouldSendJsonBody()
        {
            HttpRequestMessage? captured = null;

            var jsonResponse = "{\"success\":true}";

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
            };

            var client = CreateHttpClientReturning(response, req => captured = req);
            var controller = CreateController(client);

            var dto = _fixture.Create<object>(); // AutoFixture for request body

            var result = await controller.TestPost("api/post", dto);

            var content = Assert.IsType<ContentResult>(result);
            Assert.Equal(jsonResponse, content.Content);

            Assert.NotNull(captured);
            Assert.Equal(HttpMethod.Post, captured!.Method);

            var reqBody = await captured.Content!.ReadAsStringAsync();

            // verify something from dto appears (string representation)
            foreach (var prop in dto.GetType().GetProperties())
            {
                var val = prop.GetValue(dto)?.ToString();
                if (val != null)
                    Assert.Contains(val, reqBody);
            }
        }

        // --------------------------------------------------------------------
        // PUT forwards body
        // --------------------------------------------------------------------
        [Fact]
        public async Task ForwardPut_ShouldSendJsonBody()
        {
            HttpRequestMessage? captured = null;

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"updated\":true}", Encoding.UTF8, "application/json")
            };

            var client = CreateHttpClientReturning(response, req => captured = req);
            var controller = CreateController(client);

            var dto = new { value = 123 };

            var result = await controller.TestPut("api/put", dto);

            var content = Assert.IsType<ContentResult>(result);
            Assert.Contains("updated", content.Content);

            Assert.NotNull(captured);
            Assert.Equal(HttpMethod.Put, captured!.Method);

            var reqBody = await captured.Content!.ReadAsStringAsync();
            Assert.Contains("\"value\":123", reqBody);
        }

        // --------------------------------------------------------------------
        // DELETE
        // --------------------------------------------------------------------
        [Fact]
        public async Task ForwardDelete_ShouldSendDeleteRequest()
        {
            HttpRequestMessage? captured = null;

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"deleted\":true}", Encoding.UTF8, "application/json")
            };

            var client = CreateHttpClientReturning(response, req => captured = req);
            var controller = CreateController(client);

            var result = await controller.TestDelete("api/delete");

            var content = Assert.IsType<ContentResult>(result);
            Assert.Contains("deleted", content.Content);

            Assert.NotNull(captured);
            Assert.Equal(HttpMethod.Delete, captured!.Method);
        }
    }
}
