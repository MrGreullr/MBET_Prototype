using MBET.Core.Entities.Identity;
using MBET.web.Controllers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MBET.Tests.Controllers
{
    public class AccountControllerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<SignInManager<ApplicationUser>> _mockSignInManager;
        private readonly AccountController _controller;

        public AccountControllerTests()
        {
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(userStore.Object, null, null, null, null, null, null, null, null);

            var contextAccessor = new Mock<IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();

            _mockSignInManager = new Mock<SignInManager<ApplicationUser>>(
                _mockUserManager.Object,
                contextAccessor.Object,
                claimsFactory.Object,
                null, null, null, null);

            _controller = new AccountController(_mockUserManager.Object, _mockSignInManager.Object);
        }

        [Fact]
        public async Task Login_ShouldRedirectToError_WhenUserNotFound()
        {
            // Arrange
            _mockUserManager.Setup(u => u.FindByEmailAsync("unknown@test.com"))
                            .ReturnsAsync((ApplicationUser?)null);

            var model = new LoginViewModel { Email = "unknown@test.com", Password = "password" };

            // Act
            var result = await _controller.Login(model);

            // Assert
            var redirect = Assert.IsType<RedirectResult>(result);
            Assert.Contains("error=Invalid credentials", redirect.Url);
        }

        [Fact]
        public async Task Login_ShouldRedirectToError_WhenUserIsInactive()
        {
            // Arrange
            var user = new ApplicationUser { Email = "inactive@test.com", IsActive = false };
            _mockUserManager.Setup(u => u.FindByEmailAsync(user.Email)).ReturnsAsync(user);

            var model = new LoginViewModel { Email = user.Email, Password = "password" };

            // Act
            var result = await _controller.Login(model);

            // Assert
            var redirect = Assert.IsType<RedirectResult>(result);
            Assert.Contains("pending approval", redirect.Url);
        }

        [Fact]
        public async Task Login_ShouldRedirectToDashboard_WhenSuccess()
        {
            // Arrange
            var user = new ApplicationUser { Email = "valid@test.com", IsActive = true };
            _mockUserManager.Setup(u => u.FindByEmailAsync(user.Email)).ReturnsAsync(user);

            _mockSignInManager.Setup(s => s.PasswordSignInAsync(user, "password", false, true))
                              .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            var model = new LoginViewModel { Email = user.Email, Password = "password" };

            // Act
            var result = await _controller.Login(model);

            // Assert
            var redirect = Assert.IsType<RedirectResult>(result);
            Assert.Equal("/", redirect.Url);
        }

        [Fact]
        public async Task Register_ShouldAssignCustomerRole_WhenSuccess()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                Email = "new@test.com",
                Password = "Password123!",
                // FIX: Ensure ConfirmPassword matches Password so ModelState is valid (conceptually) 
                // and any controller logic checking this passes.
                ConfirmPassword = "Password123!",
                FirstName = "Test",
                LastName = "User"
            };

            // Setup CreateAsync to succeed
            _mockUserManager.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), model.Password))
                            .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.Register(model);

            // Assert
            // Verify AddToRoleAsync was called with "Customer"
            _mockUserManager.Verify(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Customer"), Times.Once);

            var redirect = Assert.IsType<RedirectResult>(result);
            Assert.Contains("successfully", redirect.Url);
        }
    }
}