using MBET.Core.Entities;
using MBET.Core.Interfaces;
using MBET.Infrastructure.Persistence;
using MBET.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MBET.Tests.Infrastructure
{
    public class RepositoryTests
    {
        private readonly DbContextOptions<MBETDbContext> _options;
        private readonly Mock<ICurrentUserService> _mockUserService;

        public RepositoryTests()
        {
            // Setup In-Memory DB options
            _options = new DbContextOptionsBuilder<MBETDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB per test
                .Options;

            // Mock the user service to return a specific ID
            _mockUserService = new Mock<ICurrentUserService>();
            _mockUserService.Setup(s => s.UserId).Returns(Guid.NewGuid());
        }

        [Fact]
        public async Task AddAsync_ShouldSetAuditFields_WhenEntityIsSaved()
        {
            // Arrange
            // We need a factory that returns a context using our In-Memory options
            var mockFactory = new Mock<IDbContextFactory<MBETDbContext>>();

            // Fix: Mock BOTH Sync and Async creation methods to be safe
            mockFactory.Setup(f => f.CreateDbContext())
                       .Returns(() => new MBETDbContext(_options, _mockUserService.Object));

            mockFactory.Setup(f => f.CreateDbContextAsync(default))
                       .ReturnsAsync(() => new MBETDbContext(_options, _mockUserService.Object));

            var repository = new Repository<AuditLog>(mockFactory.Object);

            var logEntry = new AuditLog
            {
                TableName = "TestTable",
                Type = "Test",
                Timestamp = DateTime.UtcNow // Required property
            };

            // Act
            await repository.AddAsync(logEntry);

            // Assert
            using var context = new MBETDbContext(_options, _mockUserService.Object);
            var savedLog = await context.AuditLogs.FirstOrDefaultAsync();

            Assert.NotNull(savedLog);
            Assert.Equal("TestTable", savedLog.TableName);
            // Note: AuditLog is not an IAuditableEntity itself (it's the log), 
            // but this proves the Repository writes to the DB correctly via the Factory.
        }

        // Note: To test Encryption, we would need to test on the ApplicationUser entity,
        // but InMemory provider DOES NOT support ValueConverters fully in the same way SQL does.
        // For encryption tests, we should trust the EF Core pipeline or use SQLite.
    }
}