using AutoFixture;
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

        private readonly Fixture _fixture;

        public AssessmentGatewayControllerTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        // ------------------------------------------------------------------
        // Reusable helper for HttpClient that returns a predefined response
        // ------------------------------------------------------------------
        private HttpClient CreateClientThatReturns(
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
                BaseAddress = new Uri("https://fake-assessment/")
            };
        }

        private AssessmentGatewayController CreateControllerWithClient(HttpClient client, string? authHeader = null)
        {
            _factoryMock = new Mock<IHttpClientFactory>();
            _factoryMock.Setup(f => f.CreateClient("AssessmentService")).Returns(client);

            var controller = new AssessmentGatewayController(_factoryMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            if (!string.IsNullOrWhiteSpace(authHeader))
                controller.Request.Headers["Authorization"] = authHeader;

            return controller;
        }

        // ---------------------------------------------------------------
        // GetQuizForModule — Missing Token
        // ---------------------------------------------------------------
        [Fact]
        public async Task GetQuizForModule_ShouldReturnUnauthorized_WhenMissingToken()
        {
            var client = CreateClientThatReturns(
                new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(new { }) });

            var controller = CreateControllerWithClient(client, authHeader: null);

            var result = await controller.GetQuizForModule(5);

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Missing access token.", unauthorized.Value);
        }

        // ---------------------------------------------------------------
        // GetQuizForModule — Success + token forwarded
        // ---------------------------------------------------------------
        [Fact]
        public async Task GetQuizForModule_ShouldReturnContent_WhenServiceReturnsSuccess_AndForwardAuth()
        {
            var payload = new { quiz = "ok" };
            var json = JsonSerializer.Serialize(payload);

            HttpRequestMessage? captured = null;

            var client = CreateClientThatReturns(
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                },
                req => captured = req);

            var controller = CreateControllerWithClient(client, "Bearer token-abc");

            var result = await controller.GetQuizForModule(12);

            var contentResult = Assert.IsType<ContentResult>(result);
            Assert.Equal("application/json", contentResult.ContentType);
            Assert.Equal(json, contentResult.Content);

            Assert.NotNull(captured);
            Assert.NotNull(captured!.Headers.Authorization);
            Assert.Equal("Bearer", captured.Headers.Authorization!.Scheme);
            Assert.Equal("token-abc", captured.Headers.Authorization!.Parameter);
        }

        // ---------------------------------------------------------------
        // SubmitQuiz — Plain string response
        // ---------------------------------------------------------------
        [Fact]
        public async Task SubmitQuiz_ShouldReturnStatusWithMessage_WhenServiceReturnsPlainString()
        {
            var plain = "some error happened";

            var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(plain, Encoding.UTF8, "text/plain")
            };

            HttpRequestMessage? captured = null;

            var client = CreateClientThatReturns(response, req => captured = req);
            var controller = CreateControllerWithClient(client, "Bearer abc");

            var dto = new { answers = new[] { 1, 2 } };

            var result = await controller.Submit(dto);

            var status = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.BadRequest, status.StatusCode);

            var dict = status.Value!.GetType().GetProperties()
                .ToDictionary(p => p.Name, p => p.GetValue(status.Value));

            Assert.Equal(plain, dict["message"]);
        }

        // ---------------------------------------------------------------
        // SubmitQuiz — JSON response
        // ---------------------------------------------------------------
        [Fact]
        public async Task SubmitQuiz_ShouldReturnJsonContent_WhenServiceReturnsJson()
        {
            var json = JsonSerializer.Serialize(new { ok = true });

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var client = CreateClientThatReturns(response);
            var controller = CreateControllerWithClient(client, "Bearer abc");

            var result = await controller.Submit(new { foo = "bar" });

            var content = Assert.IsType<ContentResult>(result);
            Assert.Equal("application/json", content.ContentType);
            Assert.Equal(json, content.Content);
        }

        // ---------------------------------------------------------------
        // GetSubmissionResult — Raw content passthrough
        // ---------------------------------------------------------------
        [Fact]
        public async Task GetSubmissionResult_ShouldReturnContent()
        {
            var id = Guid.NewGuid();
            var json = JsonSerializer.Serialize(new { score = 80 });

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var client = CreateClientThatReturns(response);
            var controller = CreateControllerWithClient(client);

            var result = await controller.Result(id);

            var content = Assert.IsType<ContentResult>(result);
            Assert.Equal("application/json", content.ContentType);
            Assert.Equal(json, content.Content);
        }

        // ---------------------------------------------------------------
        // CreateQuiz — forwards & returns content
        // ---------------------------------------------------------------
        [Fact]
        public async Task CreateQuiz_ShouldForwardAndReturnContent()
        {
            var json = JsonSerializer.Serialize(new { created = true });

            var response = new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var client = CreateClientThatReturns(response);
            var controller = CreateControllerWithClient(client, "Bearer zzz");

            var result = await controller.CreateQuiz(new { title = "q" });

            var content = Assert.IsType<ContentResult>(result);
            Assert.Equal("application/json", content.ContentType);
            Assert.Equal(json, content.Content);
        }

        // ---------------------------------------------------------------
        // AddQuestion — forwards with correct route
        // ---------------------------------------------------------------
        [Fact]
        public async Task AddQuestion_ShouldForwardToQuizSpecificRoute()
        {
            HttpRequestMessage? captured = null;

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(new { ok = true }),
                    Encoding.UTF8,
                    "application/json")
            };

            var client = CreateClientThatReturns(response, req => captured = req);
            var controller = CreateControllerWithClient(client);

            var result = await controller.AddQuestion(42, new { q = "x" });

            Assert.IsType<ContentResult>(result);

            Assert.NotNull(captured);
            Assert.Contains("/api/assessment/quiz/42/questions",
                captured!.RequestUri!.ToString());
        }
    }
}
