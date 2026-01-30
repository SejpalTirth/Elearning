using AutoFixture;
using Gateway.Contracts.Course;
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
        private readonly Mock<IHttpClientFactory> _factoryMock;
        private readonly Mock<HttpMessageHandler> _handlerMock;
        private readonly HttpClient _client;

        public GatewayCourseControllerTests()
        {
            _handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            _client = new HttpClient(_handlerMock.Object)
            {
                BaseAddress = new Uri("http://fake-course/")
            };

            _factoryMock = new Mock<IHttpClientFactory>();
            _factoryMock.Setup(f => f.CreateClient("CourseService"))
                .Returns(_client);
        }

        private GatewayCourseController CreateController()
        {
            return new GatewayCourseController(_factoryMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        private void SetupJsonResponse(string json)
        {
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                });
        }

        // ---------------- COURSES ----------------

        [Fact]
        public async Task GetAll_ShouldForwardPost_ToCoursesAll()
        {
            SetupJsonResponse("[{\"id\":1}]");

            var controller = CreateController();
            var result = await controller.GetAll();

            var content = Assert.IsType<ContentResult>(result);
            Assert.Equal("[{\"id\":1}]", content.Content);

            VerifyPost("api/courses/all");
        }

        [Fact]
        public async Task GetById_ShouldForwardPost_ToById()
        {
            SetupJsonResponse("{\"id\":5}");

            var controller = CreateController();
            var result = await controller.GetById(new CourseIdRequest { CourseId = 5 });


            AssertContent(result, "{\"id\":5}");
            VerifyPost("api/courses/by-id");
        }

        [Fact]
        public async Task Create_ShouldForwardPost_ToCreate()
        {
            SetupJsonResponse("{\"created\":true}");

            var controller = CreateController();
            var result = await controller.Create(new Course
            {
                Title = "New Course"
            });


            AssertContent(result, "{\"created\":true}");
            VerifyPost("api/courses/create");
        }

        [Fact]
        public async Task Update_ShouldForwardPost_ToUpdate()
        {
            SetupJsonResponse("{\"updated\":true}");

            var controller = CreateController();

            var fixture = new Fixture();

            var request = fixture.Create<UpdateCourseRequest>();

            var result = await controller.Update(request);

            var content = Assert.IsType<ContentResult>(result);
            Assert.Equal("{\"updated\":true}", content.Content);

            VerifyPost("api/courses/update");
        }

        [Fact]
        public async Task Delete_ShouldForwardPost_ToDelete()
        {
            SetupJsonResponse("{\"deleted\":true}");

            var controller = CreateController();
            var result = await controller.Delete(new CourseIdRequest { CourseId = 1 });

            AssertContent(result, "{\"deleted\":true}");
            VerifyPost("api/courses/delete");
        }

        // ---------------- INSTRUCTOR ----------------

        [Fact]
        public async Task GetByInstructor_ShouldForwardPost()
        {
            SetupJsonResponse("[{\"id\":2}]");

            var controller = CreateController();
            var result = await controller.GetByInstructor();

            AssertContent(result, "[{\"id\":2}]");
            VerifyPost("api/courses/instructor");
        }

        [Fact]
        public async Task GetUnfinishedCourses_ShouldForwardPost()
        {
            SetupJsonResponse("[{\"id\":3}]");

            var controller = CreateController();
            var result = await controller.GetUnfinishedCourses();

            AssertContent(result, "[{\"id\":3}]");
            VerifyPost("api/courses/unfinished");
        }

        [Fact]
        public async Task Continue_ShouldForwardPost()
        {
            SetupJsonResponse("{\"ok\":true}");

            var controller = CreateController();
            var result = await controller.Continue(new ContinueCourseRequest { CourseId = 10 });

            AssertContent(result, "{\"ok\":true}");
            VerifyPost("api/courses/continue");
        }

        // ---------------- ENROLLMENT ----------------

        [Fact]
        public async Task Enroll_ShouldForwardPost()
        {
            SetupJsonResponse("{\"enrolled\":true}");

            var controller = CreateController();
            var result = await controller.Enroll(new EnrollRequest
            {
                CourseId = 1,
            });


            AssertContent(result, "{\"enrolled\":true}");
            VerifyPost("api/courses/enroll");
        }

        [Fact]
        public async Task GetEnrolled_ShouldForwardPost()
        {
            SetupJsonResponse("[{\"id\":1}]");

            var controller = CreateController();
            var result = await controller.GetEnrolled();

            AssertContent(result, "[{\"id\":1}]");
            VerifyPost("api/courses/enrolled");
        }

        // ---------------- MODULES ----------------

        [Fact]
        public async Task GetModules_ShouldForwardPost()
        {
            SetupJsonResponse("[{\"id\":10}]");

            var controller = CreateController();
            var result = await controller.GetModules(new CourseIdRequest { CourseId = 10 });

            AssertContent(result, "[{\"id\":10}]");
            VerifyPost("api/modules/by-course");
        }

        [Fact]
        public async Task GetModule_ShouldForwardPost()
        {
            SetupJsonResponse("{\"id\":9}");

            var controller = CreateController();
            var result = await controller.GetModule(new ModuleIdRequest { ModuleId = 9 });

            AssertContent(result, "{\"id\":9}");
            VerifyPost("api/modules/content");
        }

        // ---------------- CATEGORIES ----------------

        [Fact]
        public async Task GetCategories_ShouldForwardPost()
        {
            SetupJsonResponse("[{\"id\":1,\"name\":\"Backend\"}]");

            var controller = CreateController();
            var result = await controller.GetCategories();

            AssertContent(result, "[{\"id\":1,\"name\":\"Backend\"}]");
            VerifyPost("api/categories");
        }

        // ---------------- PUBLISH / RESTORE ----------------

        [Fact]
        public async Task PublishCourse_ShouldForwardPost()
        {
            SetupJsonResponse("{\"published\":true}");

            var controller = CreateController();
            var result = await controller.PublishCourse(new CourseIdRequest { CourseId = 88 });

            AssertContent(result, "{\"published\":true}");
            VerifyPost("api/courses/publish");
        }

        [Fact]
        public async Task Restore_ShouldForwardPost()
        {
            SetupJsonResponse("{\"restored\":true}");

            var controller = CreateController();
            var result = await controller.Restore(new CourseIdRequest { CourseId = 1 });

            AssertContent(result, "{\"restored\":true}");
            VerifyPost("api/courses/restore");
        }

        // ---------------- Helpers ----------------

        private void AssertContent(IActionResult result, string expected)
        {
            var content = Assert.IsType<ContentResult>(result);
            Assert.Equal(expected, content.Content);
        }

        private void VerifyPost(string endsWith)
        {
            _handlerMock.Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri!.ToString().EndsWith(endsWith)),
                    ItExpr.IsAny<CancellationToken>());
        }
    }
}
