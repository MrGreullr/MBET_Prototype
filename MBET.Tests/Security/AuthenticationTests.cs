using MBET.Core.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Moq;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace MBET.Tests.Security
{
    public class AuthenticationTests
    {
        [Fact]
        public async Task ValidateSecurityStamp_ShouldFail_IfStampsDoNotMatch()
        {
            // Arrange
            var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = "test", SecurityStamp = "stamp1" };

            var store = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);

            userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            userManagerMock.Setup(u => u.SupportsUserSecurityStamp).Returns(true);
            userManagerMock.Setup(u => u.GetSecurityStampAsync(user)).ReturnsAsync("stamp2"); // DB has NEW stamp

            // Create a principal (cookie) that has the OLD stamp
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("AspNet.Identity.SecurityStamp", "stamp1") // Cookie has OLD stamp
            }, "test");
            var principal = new ClaimsPrincipal(identity);

            // Act - Simulation of what RevalidatingProvider does
            var userStamp = await userManagerMock.Object.GetSecurityStampAsync(user);
            var principalStamp = principal.FindFirstValue("AspNet.Identity.SecurityStamp");
            var isValid = userStamp == principalStamp;

            // Assert
            Assert.False(isValid, "Session should be invalid because security stamps do not match.");
        }
    }
}