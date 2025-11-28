using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Gateway.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.Protected;

namespace LMS.Tests.Gateway
{
    public class AssessmentGatewayControllerTests
    {
        private Mock<IHttpClientFactory> _factoryMock = null!;
        private Mock<HttpMessageHandler> _handlerMock = null!;

        private HttpClient CreateClientThatReturns(HttpResponseMessage response, Action<HttpRequestMessage>? capture = null)
        {
            _handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            _handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                   "SendAsync",
                   ItExpr.IsAny<HttpRequestMessage>(),
                   ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync((HttpRequestMessage req, CancellationToken _) =>
               {
                   capture?.Invoke(req);
                   return response;
               });

            var client = new HttpClient(_handlerMock.Object)
            {
                BaseAddress = new Uri("https://fake-assessment/")
            };

            return client;
        }

        private AssessmentGatewayController CreateControllerWithClient(HttpClient client, string? authHeader = null)
        {
            _factoryMock = new Mock<IHttpClientFactory>();
            _factoryMock.Setup(f => f.CreateClient("AssessmentService")).Returns(client);

            var controller = new AssessmentGatewayController(_factoryMock.Object);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            if (!string.IsNullOrEmpty(authHeader))
            {
                controller.Request.Headers["Authorization"] = authHeader;
            }

            return controller;
        }

        // -----------------------
        // GetQuizForModule - Missing token -> Unauthorized
        // -----------------------
        [Fact]
        public async Task GetQuizForModule_ShouldReturnUnauthorized_WhenMissingToken()
        {
            // Arrange: create client but not used because controller returns early
            var client = CreateClientThatReturns(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new { })
            });

            var controller = CreateControllerWithClient(client, authHeader: null);

            // Act
            var result = await controller.GetQuizForModule(5);

            // Assert
            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Missing access token.", unauthorized.Value);
        }

        // -----------------------
        // GetQuizForModule - Success, token forwarded
        // -----------------------
        [Fact]
        public async Task GetQuizForModule_ShouldReturnContent_WhenServiceReturnsSuccess_AndForwardAuth()
        {
            // Arrange
            var payload = new { quiz = "ok" };
            var contentJson = JsonSerializer.Serialize(payload);

            HttpRequestMessage? captured = null;
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(contentJson, Encoding.UTF8, "application/json")
            };

            var client = CreateClientThatReturns(response, req => captured = req);

            var controller = CreateControllerWithClient(client, authHeader: "Bearer token-abc");

            // Act
            var result = await controller.GetQuizForModule(12);

            // Assert - returned JSON as ContentResult
            var contentResult = Assert.IsType<ContentResult>(result);
            Assert.Equal("application/json", contentResult.ContentType);
            Assert.Equal(contentJson, contentResult.Content);

            // Assert - outgoing request had Authorization header set on HttpClient (controller uses DefaultRequestHeaders)
            // because controller sets client.DefaultRequestHeaders.Authorization; ensure handler saw a request
            Assert.NotNull(captured);
            // the request will include Authorization header when sent by HttpClient
            Assert.True(captured!.Headers.Authorization != null);
            Assert.Equal("Bearer", captured.Headers.Authorization!.Scheme);
            Assert.Equal("token-abc", captured.Headers.Authorization!.Parameter);
        }

        // -----------------------
        // Forwarding helper: non-JSON string body -> status code with message
        // -----------------------
        [Fact]
        public async Task SubmitQuiz_ShouldReturnStatusWithMessage_WhenServiceReturnsPlainString()
        {
            // Arrange
            var plain = "some error happened";
            HttpRequestMessage? captured = null;
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(plain, Encoding.UTF8, "text/plain")
            };

            var client = CreateClientThatReturns(response, req => captured = req);
            var controller = CreateControllerWithClient(client, authHeader: "Bearer abc");

            // Act
            var dto = new { answers = new[] { 1, 2 } };
            var result = await controller.SubmitQuiz(dto);

            // Assert: result should be a StatusCodeObjectResult containing message
            var status = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.BadRequest, status.StatusCode);

            // The controller wraps the plain text in anonymous object { message = content }
            var msgObj = status.Value!;
            var dict = msgObj.GetType().GetProperties()
                        .ToDictionary(p => p.Name, p => p.GetValue(msgObj));
            Assert.Equal(plain, dict["message"]);
        }

        // -----------------------
        // Forwarding helper: JSON response -> ContentResult
        // -----------------------
        [Fact]
        public async Task SubmitQuiz_ShouldReturnJsonContent_WhenServiceReturnsJson()
        {
            // Arrange
            var json = JsonSerializer.Serialize(new { ok = true });
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var client = CreateClientThatReturns(response);
            var controller = CreateControllerWithClient(client, authHeader: "Bearer abc");

            // Act
            var result = await controller.SubmitQuiz(new { foo = "bar" });

            // Assert
            var content = Assert.IsType<ContentResult>(result);
            Assert.Equal("application/json", content.ContentType);
            Assert.Equal(json, content.Content);
        }

        // -----------------------
        // GetSubmissionResult -> returns raw content as JSON
        // -----------------------
        [Fact]
        public async Task GetSubmissionResult_ShouldReturnContent()
        {
            // Arrange
            var id = Guid.NewGuid();
            var json = JsonSerializer.Serialize(new { score = 80 });
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var client = CreateClientThatReturns(response);
            var controller = CreateControllerWithClient(client);

            // Act
            var result = await controller.GetSubmissionResult(id);

            // Assert
            var content = Assert.IsType<ContentResult>(result);
            Assert.Equal("application/json", content.ContentType);
            Assert.Equal(json, content.Content);
        }

        // -----------------------
        // CreateQuiz -> forwards and returns JSON content
        // -----------------------
        [Fact]
        public async Task CreateQuiz_ShouldForwardAndReturnContent()
        {
            // Arrange
            var json = JsonSerializer.Serialize(new { created = true });
            var response = new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var client = CreateClientThatReturns(response);
            var controller = CreateControllerWithClient(client, authHeader: "Bearer zzz");

            // Act
            var result = await controller.CreateQuiz(new { title = "q" });

            // Assert
            var content = Assert.IsType<ContentResult>(result);
            Assert.Equal("application/json", content.ContentType);
            Assert.Equal(json, content.Content);
        }

        // -----------------------
        // AddQuestion -> forwards to specific quiz id
        // -----------------------
        [Fact]
        public async Task AddQuestion_ShouldForwardToQuizSpecificRoute()
        {
            // Arrange
            HttpRequestMessage? captured = null;
            // return any JSON so controller returns Content
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new { ok = true }), Encoding.UTF8, "application/json")
            };

            var client = CreateClientThatReturns(response, req => captured = req);
            var controller = CreateControllerWithClient(client);

            // Act
            var result = await controller.AddQuestion(42, new { q = "x" });

            // Assert
            Assert.IsType<ContentResult>(result);

            // Validate the outgoing request URI path contains expected endpoint
            Assert.NotNull(captured);
            Assert.Contains("/api/assessment/quiz/42/questions", captured!.RequestUri!.ToString());
        }
    }
}
