using MBET.Core.Entities;
using MBET.Core.Entities.Identity;
using MBET.Core.Interfaces;
using MBET.Infrastructure.Persistence;
using MBET.Infrastructure.Repositories;
using MBET.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MBET.Tests.Services
{
    public class OrderManagementTests
    {
        private readonly DbContextOptions<MBETDbContext> _options;
        private readonly Mock<ICurrentUserService> _mockUserService;
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<ISettingsService> _mockSettings;
        private readonly Mock<IProductRepository> _mockProductRepo;

        public OrderManagementTests()
        {
            _options = new DbContextOptionsBuilder<MBETDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _mockUserService = new Mock<ICurrentUserService>();
            _mockSettings = new Mock<ISettingsService>();
            _mockProductRepo = new Mock<IProductRepository>();
            var store = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);
        }

        [Fact]
        public async Task UpdateOrderStatusAsync_ShouldUpdateStatus_WhenValid()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            using (var context = new MBETDbContext(_options, _mockUserService.Object))
            {
                context.Orders.Add(new Order { Id = orderId, Status = OrderStatus.Pending });
                await context.SaveChangesAsync();
            }

            var mockFactory = new Mock<IDbContextFactory<MBETDbContext>>();
            mockFactory.Setup(f => f.CreateDbContextAsync(default))
                       .ReturnsAsync(() => new MBETDbContext(_options, _mockUserService.Object));

            var orderRepo = new OrderRepository(mockFactory.Object);
            var service = new OrderService(_mockUserManager.Object, orderRepo, _mockSettings.Object, _mockProductRepo.Object);

            // Act
            await service.UpdateOrderStatusAsync(orderId, OrderStatus.Processing);

            // Assert
            using (var context = new MBETDbContext(_options, _mockUserService.Object))
            {
                var updatedOrder = await context.Orders.FindAsync(orderId);
                Assert.NotNull(updatedOrder);
                Assert.Equal(OrderStatus.Processing, updatedOrder.Status);
            }
        }

        [Fact]
        public async Task UpdateOrderStatusAsync_ShouldThrow_WhenRevertingDeliveredToPending()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            using (var context = new MBETDbContext(_options, _mockUserService.Object))
            {
                context.Orders.Add(new Order { Id = orderId, Status = OrderStatus.Delivered });
                await context.SaveChangesAsync();
            }

            var mockFactory = new Mock<IDbContextFactory<MBETDbContext>>();
            mockFactory.Setup(f => f.CreateDbContextAsync(default))
                       .ReturnsAsync(() => new MBETDbContext(_options, _mockUserService.Object));

            var orderRepo = new OrderRepository(mockFactory.Object);
            var service = new OrderService(_mockUserManager.Object, orderRepo, _mockSettings.Object, _mockProductRepo.Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.UpdateOrderStatusAsync(orderId, OrderStatus.Pending));
        }
    }
}