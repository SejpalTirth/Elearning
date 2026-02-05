using Gateway.UserContext;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Text;
using Xunit;

namespace LMS.Tests.Gateway
{
    public class GatewayForwarderTests
    {
        [Fact]
        public void AttachUserContext_ReturnsEarly_WhenUserIsNotAuthenticated()
        {
            // Arrange
            var context = new DefaultHttpContext();
            // User is not authenticated by default in DefaultHttpContext
            var request = new HttpRequestMessage();

            // Act
            GatewayForwarder.AttachUserContext(context, request);

            // Assert
            Assert.Empty(request.Headers);
        }

        [Fact]
        public void AttachUserContext_AddsHeader_WhenUserIsAuthenticated()
        {
            // Arrange
            var request = new HttpRequestMessage();
            var context = new DefaultHttpContext();

            // Create an authenticated user
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "user-123"),
                new Claim(ClaimTypes.Email, "test@example.com"),
                new Claim(ClaimTypes.Role, "Student")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType"); // AuthType makes IsAuthenticated = true
            context.User = new ClaimsPrincipal(identity);

            // Act
            GatewayForwarder.AttachUserContext(context, request);

            // Assert
            Assert.True(request.Headers.Contains("X-User-Context"));

            var headerValue = request.Headers.GetValues("X-User-Context").First();
            Assert.NotEmpty(headerValue);

            // Optional: Verify the Base64 content actually decodes back to JSON
            var decodedBytes = Convert.FromBase64String(headerValue);
            var decodedJson = Encoding.UTF8.GetString(decodedBytes);
            Assert.Contains("user-123", decodedJson);
        }

        [Fact]
        public void AttachUserContext_HandlesNullIdentity_ByReturningEarly()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.User = new ClaimsPrincipal(); // No identity
            var request = new HttpRequestMessage();

            // Act
            GatewayForwarder.AttachUserContext(context, request);

            // Assert
            Assert.Empty(request.Headers);
        }
    }
}