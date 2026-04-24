using MBET.Core.Entities;
using MBET.Core.Interfaces;
using MBET.Infrastructure.Persistence;
using MBET.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MBET.Tests.Infrastructure
{
    public class OrderRepositoryTests
    {
        private readonly DbContextOptions<MBETDbContext> _options;
        private readonly Mock<ICurrentUserService> _mockUserService;

        public OrderRepositoryTests()
        {
            _options = new DbContextOptionsBuilder<MBETDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _mockUserService = new Mock<ICurrentUserService>();
        }

        [Fact]
        public async Task GetOrdersByUserIdAsync_ShouldReturnUserOrdersOnly()
        {
            // Arrange
            var user1 = Guid.NewGuid();
            var user2 = Guid.NewGuid();

            using (var context = new MBETDbContext(_options, _mockUserService.Object))
            {
                context.Orders.Add(new Order { UserId = user1, GrandTotal = 100 });
                context.Orders.Add(new Order { UserId = user1, GrandTotal = 200 });
                context.Orders.Add(new Order { UserId = user2, GrandTotal = 500 });
                await context.SaveChangesAsync();
            }

            var mockFactory = new Mock<IDbContextFactory<MBETDbContext>>();
            mockFactory.Setup(f => f.CreateDbContextAsync(default))
                       .ReturnsAsync(() => new MBETDbContext(_options, _mockUserService.Object));

            var repo = new OrderRepository(mockFactory.Object);

            // Act
            var orders = await repo.GetOrdersByUserIdAsync(user1);

            // Assert
            Assert.Equal(2, orders.Count());
            Assert.All(orders, o => Assert.Equal(user1, o.UserId));
        }
    }
}