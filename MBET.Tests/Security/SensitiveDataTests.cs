using MBET.web.Controllers; // For LoginViewModel
using Xunit;

namespace MBET.Tests.Security
{
    public class SensitiveDataTests
    {
        [Fact]
        public void LoginViewModel_ToString_ShouldNotRevealPassword()
        {
            // Arrange
            var model = new LoginViewModel { Email = "admin@mbet.io", Password = "SuperSecretPassword123!" };

            // Act
            var stringRepresentation = model.ToString();

            // Assert
            // By default, C# objects return their Type Name on ToString(), unless overridden.
            // If someone overrode it to dump all properties, this test will catch it.
            Assert.DoesNotContain("SuperSecretPassword123!", stringRepresentation);
        }
    }
}