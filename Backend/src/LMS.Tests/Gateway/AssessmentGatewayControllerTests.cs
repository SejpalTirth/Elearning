using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Gateway.Controllers;
using Gateway.Contracts.Assessment;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.Protected;
using Xunit;

namespace LMS.Tests.Gateway
{
    public class AssessmentGatewayControllerTests
    {
        private Mock<IHttpClientFactory> _factoryMock = null!;
        private Mock<HttpMessageHandler> _handlerMock = null!;

        // ---------------- Helper to mock HttpClient ----------------
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

        private AssessmentGatewayController CreateController(HttpClient client, string? authHeader = null)
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

        // ------------------- GetQuizForModule -------------------

        [Fact]
        public async Task GetQuizForModule_ShouldReturn401_WhenMissingToken()
        {
            var client = CreateClientThatReturns(
                new HttpResponseMessage(HttpStatusCode.Unauthorized)
                {
                    Content = new StringContent("{\"message\": \"Missing access token.\"}", Encoding.UTF8, "application/json")
                });

            var controller = CreateController(client);

            var result = await controller.GetQuizForModule(new GetQuizForModuleRequest { ModuleId = 1 });

            var obj = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal(401, obj.StatusCode);
            Assert.Equal("{\"message\":\"Missing access token.\"}", JsonSerializer.Serialize(obj.Value));
        }



        [Fact]
        public async Task GetQuizForModule_ShouldReturn200_WhenServiceReturnsOk()
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

            var controller = CreateController(client, "Bearer token-123");

            var result = await controller.GetQuizForModule(
                new GetQuizForModuleRequest { ModuleId = 12 });

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, obj.StatusCode);
            Assert.Equal(json, obj.Value);

            Assert.NotNull(captured);
            Assert.Equal("Bearer", captured!.Headers.Authorization!.Scheme);
            Assert.Equal("token-123", captured.Headers.Authorization.Parameter);
        }

        [Fact]
        public async Task GetQuizForModule_ShouldReturn500_OnException()
        {
            var handler = new Mock<HttpMessageHandler>();
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exception("boom"));

            var client = new HttpClient(handler.Object);
            var controller = CreateController(client, "Bearer token");

            var result = await controller.GetQuizForModule(
                new GetQuizForModuleRequest { ModuleId = 1 });

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
        }

        // ------------------- CreateQuiz -------------------

        [Fact]
        public async Task CreateQuiz_ShouldReturn200_WhenOk()
        {
            var expectedPayload = new { quizId = 99 };
            var json = JsonSerializer.Serialize(expectedPayload);

            var client = CreateClientThatReturns(
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                });

            var controller = CreateController(client);

            var result = await controller.CreateQuiz(new CreateQuiz { Title = "Test Quiz", ModuleId = 1 });

            var obj = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(200, obj.StatusCode);

            // Deserialize the JSON string from obj.Value
            var actualJson = obj.Value as string ?? JsonSerializer.Serialize(obj.Value);
            var actualPayload = JsonSerializer.Deserialize<Dictionary<string, int>>(actualJson);

            Assert.NotNull(actualPayload);
            Assert.Equal(99, actualPayload!["quizId"]);
        }

        [Fact]
        public async Task CreateQuiz_ShouldReturn400_WhenServiceFails()
        {
            var client = CreateClientThatReturns(
                new HttpResponseMessage(HttpStatusCode.BadRequest));

            var controller = CreateController(client);

            var result = await controller.CreateQuiz(
                new CreateQuiz { Title = "Fail", ModuleId = 1 });

            var obj = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(400, obj.StatusCode);
        }

        [Fact]
        public async Task CreateQuiz_ShouldReturn500_OnException()
        {
            var handler = new Mock<HttpMessageHandler>();
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exception("crash"));

            var client = new HttpClient(handler.Object);
            var controller = CreateController(client);

            var result = await controller.CreateQuiz(
                new CreateQuiz { Title = "Crash", ModuleId = 1 });

            var obj = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, obj.StatusCode);
        }

        // ------------------- AddQuestion -------------------

        [Fact]
        public async Task AddQuestion_ShouldReturn200_WhenOk()
        {
            var expectedPayload = new { success = true };
            var json = JsonSerializer.Serialize(expectedPayload);

            var client = CreateClientThatReturns(
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                });

            var controller = CreateController(client);

            var result = await controller.AddQuestion(new AddQuestionRequest
            {
                QuizId = 1,
                Question = new CreateQuestion
                {
                    Question = "Q",
                    Marks = 1,
                    Options = new() { "A", "B" },
                    CorrectAnswerIndex = 0
                }
            });

            var obj = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(200, obj.StatusCode);

            var actualJson = obj.Value as string ?? JsonSerializer.Serialize(obj.Value);
            var actualPayload = JsonSerializer.Deserialize<Dictionary<string, bool>>(actualJson);

            Assert.NotNull(actualPayload);
            Assert.True(actualPayload!["success"]);
        }

        [Fact]
        public async Task AddQuestion_ShouldReturn400_WhenServiceFails()
        {
            var client = CreateClientThatReturns(
                new HttpResponseMessage(HttpStatusCode.BadRequest));

            var controller = CreateController(client);

            var result = await controller.AddQuestion(new AddQuestionRequest
            {
                QuizId = 1,
                Question = new CreateQuestion
                {
                    Question = "Q",
                    Marks = 1,
                    Options = new() { "A", "B" },
                    CorrectAnswerIndex = 0
                }
            });

            var obj = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(400, obj.StatusCode);
        }

        // ------------------- Submit -------------------

        [Fact]
        public async Task Submit_ShouldReturn200_WhenOk()
        {
            var payload = new { score = 90 };
            var json = JsonSerializer.Serialize(payload);

            var client = CreateClientThatReturns(
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                });

            var controller = CreateController(client);

            var result = await controller.Submit(new SubmitQuiz());

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, obj.StatusCode);
            Assert.Equal(json, obj.Value);
        }

        [Fact]
        public async Task Submit_ShouldReturn500_OnException()
        {
            var handler = new Mock<HttpMessageHandler>();
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exception("submit failed"));

            var client = new HttpClient(handler.Object);
            var controller = CreateController(client);

            var result = await controller.Submit(new SubmitQuiz());

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
        }

        // ------------------- Result -------------------

        [Fact]
        public async Task Result_ShouldReturn404_WhenNull()
        {
            var client = CreateClientThatReturns(
                new HttpResponseMessage(HttpStatusCode.NoContent));

            var controller = CreateController(client);

            var result = await controller.Result(
                new ResultRequest { SubmissionId = Guid.NewGuid() });

            var obj = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(204, obj.StatusCode);
        }

        // ------------------- QuizStatus -------------------

        [Fact]
        public async Task QuizStatus_ShouldReturn200_WhenOk()
        {
            var expectedPayload = new { AllQuizzesCreated = true };
            var json = JsonSerializer.Serialize(expectedPayload);

            var client = CreateClientThatReturns(
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                });

            var controller = CreateController(client);

            var result = await controller.QuizStatus(new CourseQuizStatusRequest { CourseId = 1 });

            var obj = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(200, obj.StatusCode);

            var actualJson = obj.Value as string ?? JsonSerializer.Serialize(obj.Value);
            var actualPayload = JsonSerializer.Deserialize<Dictionary<string, bool>>(actualJson);

            Assert.NotNull(actualPayload);
            Assert.True(actualPayload!["AllQuizzesCreated"]);
        }

        // ------------------- GetUnquizzed -------------------

        [Fact]
        public async Task GetUnquizzed_ShouldReturn200_WhenOk()
        {
            var payload = new List<int> { 1, 2, 3 };
            var json = JsonSerializer.Serialize(payload);

            var client = CreateClientThatReturns(
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                });

            var controller = CreateController(client);

            var result = await controller.GetUnquizzed(
                new GetUnquizzedRequest { CourseId = 1 });

            var obj = Assert.IsType<NotFoundResult>(result.Result);
            Assert.Equal(404, obj.StatusCode);
        }

        [Fact]
        public async Task GetUnquizzed_ShouldReturn404_WhenServiceFails()
        {
            var client = CreateClientThatReturns(
                new HttpResponseMessage(HttpStatusCode.BadRequest));

            var controller = CreateController(client);

            var result = await controller.GetUnquizzed(
                new GetUnquizzedRequest { CourseId = 1 });

            var obj = Assert.IsType<NotFoundResult>(result.Result);
            Assert.Equal(404, obj.StatusCode);
        }
    }
}
