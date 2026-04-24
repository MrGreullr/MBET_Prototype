using MBET.Core.Entities;
using MBET.Core.Interfaces;
using MBET.Infrastructure.Persistence;
using MBET.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MBET.Tests.Infrastructure
{
    public class ConcurrencyTests
    {
        private readonly DbContextOptions<MBETDbContext> _options;
        private readonly Mock<ICurrentUserService> _mockUserService;

        public ConcurrencyTests()
        {
            _options = new DbContextOptionsBuilder<MBETDbContext>()
                .UseInMemoryDatabase(databaseName: "StressTestDB")
                .Options;
            _mockUserService = new Mock<ICurrentUserService>();
        }

        [Fact]
        public async Task ParallelWrites_ShouldNotCrashContext()
        {
            // Arrange
            var mockFactory = new Mock<IDbContextFactory<MBETDbContext>>();
            // Critical: Return NEW context per call
            mockFactory.Setup(f => f.CreateDbContextAsync(default))
                       .ReturnsAsync(() => new MBETDbContext(_options, _mockUserService.Object));

            var repo = new ProductRepository(mockFactory.Object);
            var tasks = new List<Task>();
            int threadCount = 50;

            // Act: 50 threads writing simultaneously
            for (int i = 0; i < threadCount; i++)
            {
                int id = i;
                tasks.Add(Task.Run(async () => {
                    await repo.AddProductAsync(new Product
                    {
                        Title = $"Stress {id}",
                        Price = 10,
                        IsActive = true
                    });
                }));
            }

            await Task.WhenAll(tasks);

            // Assert
            using var context = new MBETDbContext(_options, _mockUserService.Object);
            var count = await context.Products.CountAsync();
            Assert.Equal(threadCount, count);
        }
    }
}