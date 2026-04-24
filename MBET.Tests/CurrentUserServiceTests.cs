using MBET.web.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;
using Xunit;

namespace MBET.Tests.Services
{
    public class CurrentUserServiceTests
    {
        [Fact]
        public void GetUserId_ShouldReturnGuid_WhenUserIsAuthenticated()
        {
            // 1. Arrange (Setup the scenario)
            var userId = Guid.NewGuid();

            // Mocking IHttpContextAccessor because we don't have a real web server running in a test
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext();

            // Simulate a logged-in user with a specific ID
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            context.User = principal;
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

            // Create the service with the mock
            var service = new CurrentUserService(mockHttpContextAccessor.Object);

            // 2. Act (Execute the method we are testing)
            var result = service.UserId;

            // 3. Assert (Verify the result is what we expected)
            Assert.Equal(userId, result);
        }

        [Fact]
        public void GetUserId_ShouldReturnNull_WhenUserIsNotAuthenticated()
        {
            // Arrange
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

            // Simulate context where user is null or empty
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(new DefaultHttpContext());

            var service = new CurrentUserService(mockHttpContextAccessor.Object);

            // Act
            var result = service.UserId;

            // Assert
            Assert.Null(result);
        }
    }
}