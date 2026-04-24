using MBET.Core.Entities;
using MBET.Core.Interfaces;
using MBET.Infrastructure.Persistence;
using MBET.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MBET.Tests.Security
{
    public class SqlInjectionTests
    {
        private readonly DbContextOptions<MBETDbContext> _options;

        public SqlInjectionTests()
        {
            // Use SQLite In-Memory to mimic relational behavior better than "InMemory" provider
            _options = new DbContextOptionsBuilder<MBETDbContext>()
                .UseSqlite("DataSource=:memory:")
                .Options;
        }

        [Fact]
        public async Task Repository_ShouldNotBeVulnerableToSqlInjection_OnFind()
        {
            // Arrange
            var connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
            connection.Open();
            var options = new DbContextOptionsBuilder<MBETDbContext>().UseSqlite(connection).Options;

            using (var context = new MBETDbContext(options, new Mock<ICurrentUserService>().Object))
            {
                context.Database.EnsureCreated();
                // Seed a log entry
                context.AuditLogs.Add(new AuditLog { TableName = "SafeTable", Timestamp = DateTime.UtcNow });
                await context.SaveChangesAsync();
            }

            // Act
            // Attempt a classic SQL injection payload as input
            // If the repo was concatenating strings: "SafeTable' OR '1'='1" would return ALL rows.
            var maliciousInput = "SafeTable' OR '1'='1";

            var mockFactory = new Mock<IDbContextFactory<MBETDbContext>>();
            mockFactory.Setup(f => f.CreateDbContextAsync(default))
                       .ReturnsAsync(() => new MBETDbContext(options, new Mock<ICurrentUserService>().Object));

            // We also mock the sync version for completeness if your repo uses it
            mockFactory.Setup(f => f.CreateDbContext())
                       .Returns(() => new MBETDbContext(options, new Mock<ICurrentUserService>().Object));

            var repo = new Repository<AuditLog>(mockFactory.Object);

            // This predicate uses LINQ, which EF Core parameterizes automatically.
            // We are testing that "Malicious Input" is treated as a literal string value, not SQL command.
            var results = await repo.FindAsync(l => l.TableName == maliciousInput);

            // Assert
            // Should return 0 results because no table is named literally "SafeTable' OR '1'='1"
            // If Injection worked, it would return the "SafeTable" entry we seeded.
            Assert.Empty(results);
        }
    }
}