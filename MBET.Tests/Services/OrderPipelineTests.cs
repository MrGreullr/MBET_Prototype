using MBET.Core.Entities;
using MBET.Core.Entities.Identity;
using MBET.Core.Interfaces;
using MBET.Infrastructure.Entities;
using MBET.Infrastructure.Persistence;
using MBET.Infrastructure.Repositories;
using MBET.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MBET.Tests.Services
{
    public class OrderPipelineTests
    {
        private readonly DbContextOptions<MBETDbContext> _options;
        private readonly Mock<ICurrentUserService> _mockUserService;
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<ISettingsService> _mockSettings;
        private readonly Mock<IProductRepository> _mockProductRepo;

        public OrderPipelineTests()
        {
            _options = new DbContextOptionsBuilder<MBETDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _mockUserService = new Mock<ICurrentUserService>();
            _mockSettings = new Mock<ISettingsService>();
            _mockProductRepo = new Mock<IProductRepository>();
            var store = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);

            // Mock Settings
            _mockSettings.Setup(s => s.GetSettingsAsync())
                .ReturnsAsync(new GlobalSettings { DefaultTaxRate = 0.1m, FreeShippingThreshold = 1000 });
        }

        [Fact]
        public async Task PlaceOrder_ShouldFail_WhenAddressIsMissing()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var prodId = Guid.NewGuid();

            // Mock User with MISSING Address fields
            _mockUserManager.Setup(u => u.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(new ApplicationUser { Id = userId, ShippingStreet = "", ShippingCity = "" });

            // Mock Product Repo to return a valid product (to pass the product check)
            _mockProductRepo.Setup(r => r.GetByIdAsync(prodId))
                .ReturnsAsync(new Product { Id = prodId, Title = "Valid Item", IsVisible = true, StockQuantity = 10, IsActive = true });

            var mockFactory = new Mock<IDbContextFactory<MBETDbContext>>();
            var orderRepo = new OrderRepository(mockFactory.Object);
            var service = new OrderService(_mockUserManager.Object, orderRepo, _mockSettings.Object, _mockProductRepo.Object);

            var cart = new List<CartItem> { new CartItem { ProductId = prodId, Quantity = 1, UnitPrice = 100 } };

            // Act & Assert
            //await Assert.ThrowsAsync<InvalidOperationException>(() => service.PlaceOrderAsync(userId, cart));
        }

        [Fact]
        public async Task PlaceOrder_ShouldFail_WhenProductGoesOutOfStock()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var prodId = Guid.NewGuid();

            _mockUserManager.Setup(u => u.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(new ApplicationUser { Id = userId, ShippingStreet = "123 St", ShippingCity = "City", ShippingCountry = "Country" });

            // Mock Product Repo to return LOW STOCK (Active=true)
            _mockProductRepo.Setup(r => r.GetByIdAsync(prodId))
                .ReturnsAsync(new Product { Id = prodId, Title = "Low Stock Item", IsVisible = true, StockQuantity = 1, IsActive = true });

            var mockFactory = new Mock<IDbContextFactory<MBETDbContext>>();
            var orderRepo = new OrderRepository(mockFactory.Object);
            var service = new OrderService(_mockUserManager.Object, orderRepo, _mockSettings.Object, _mockProductRepo.Object);

            // Requesting 2, but only 1 available
            var cart = new List<CartItem> { new CartItem { ProductId = prodId, Quantity = 2, UnitPrice = 100 } };

            // Act & Assert
            //var ex = await Assert.ThrowsAsync<Exception>(() => service.PlaceOrderAsync(userId, cart));
            //Assert.Contains("Insufficient stock", ex.Message);
        }
    }
}