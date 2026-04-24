using MBET.Core.Entities;
using MBET.Core.Interfaces;
using MBET.Infrastructure.Persistence;
using MBET.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace MBET.Tests.Infrastructure
{
    public class SettingsServiceTests : IDisposable
    {
        private readonly DbContextOptions<MBETDbContext> _options;
        private readonly Mock<ICurrentUserService> _mockUserService;

        public SettingsServiceTests()
        {
            // Use unique database name per test to prevent leakage
            _options = new DbContextOptionsBuilder<MBETDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _mockUserService = new Mock<ICurrentUserService>();
            _mockUserService.Setup(s => s.UserId).Returns(Guid.NewGuid());

            // IMPORTANT: Clear the static cache before every test
            ResetStaticCache();
        }

        public void Dispose()
        {
            ResetStaticCache();
        }

        private void ResetStaticCache()
        {
            // Use reflection to reset the private static _cachedSettings field
            var field = typeof(SettingsService).GetField("_cachedSettings", BindingFlags.Static | BindingFlags.NonPublic);
            field?.SetValue(null, null);
        }

        private SettingsService CreateService()
        {
            var mockFactory = new Mock<IDbContextFactory<MBETDbContext>>();

            // CRITICAL FIX: Return a NEW context instance every time, not a reused variable.
            // The service will Dispose() the context it gets, so we cannot give it the same one twice.
            mockFactory.Setup(f => f.CreateDbContextAsync(default))
                       .ReturnsAsync(() => new MBETDbContext(_options, _mockUserService.Object));

            return new SettingsService(mockFactory.Object);
        }

        [Fact]
        public async Task GetSettingsAsync_ShouldCreateDefault_WhenDbIsEmpty()
        {
            // Arrange
            var service = CreateService();

            // Act
            var settings = await service.GetSettingsAsync();

            // Assert
            Assert.NotNull(settings);
            Assert.Equal("Modular Blazor E-Commerce Template Solution", settings.SiteName); // Default value check

            using var context = new MBETDbContext(_options, _mockUserService.Object);
            Assert.Equal(1, await context.GlobalSettings.CountAsync());
        }

        [Fact]
        public async Task GetSettingsAsync_ShouldReturnCachedInstance_OnSecondCall()
        {
            // Arrange
            var service = CreateService();

            // Act
            var firstCall = await service.GetSettingsAsync();

            // Modify DB behind the scenes to prove we are reading from cache, not DB
            using (var context = new MBETDbContext(_options, _mockUserService.Object))
            {
                var dbSettings = await context.GlobalSettings.FirstAsync();
                dbSettings.SiteName = "DB Changed";
                await context.SaveChangesAsync();
            }

            var secondCall = await service.GetSettingsAsync();

            // Assert
            Assert.Equal("Modular Blazor E-Commerce Template Solution", secondCall.SiteName); // Should still have old name from cache
            Assert.Same(firstCall, secondCall); // Same object reference
        }

        [Fact]
        public async Task UpdateSettingsAsync_ShouldUpdateDb_AndInvalidateCache()
        {
            // Arrange
            var service = CreateService();

            // Initial load to populate cache/DB
            var settings = await service.GetSettingsAsync();
            settings.SiteName = "New Name";
            settings.IsMaintenanceMode = true;

            // Act
            await service.UpdateSettingsAsync(settings);

            // Assert
            // 1. Verify DB is updated
            using (var verifyContext = new MBETDbContext(_options, _mockUserService.Object))
            {
                var dbSettings = await verifyContext.GlobalSettings.FirstAsync();
                Assert.Equal("New Name", dbSettings.SiteName);
                Assert.True(dbSettings.IsMaintenanceMode);
            }

            // 2. Verify Cache is updated (fetching again should give new values)
            var newSettings = await service.GetSettingsAsync();
            Assert.Equal("New Name", newSettings.SiteName);
        }

        [Fact]
        public async Task UpdateSettingsAsync_ShouldHandleFeatureCollection_Correctly()
        {
            // Arrange
            var service = CreateService();

            var settings = await service.GetSettingsAsync();

            // Add a feature
            settings.Features = new List<SiteFeature>
            {
                new SiteFeature { Title = "Feature 1", Icon = "Icon1", Order = 1 }
            };

            // Act 1: Save initial list
            await service.UpdateSettingsAsync(settings);

            // Assert 1
            using (var verifyContext = new MBETDbContext(_options, _mockUserService.Object))
            {
                var dbSettings = await verifyContext.GlobalSettings.Include(s => s.Features).FirstAsync();
                Assert.Single(dbSettings.Features);
                Assert.Equal("Feature 1", dbSettings.Features[0].Title);
            }

            // Act 2: Modify (Update 1, Add 1)
            // Note: In a real app, IDs are set by DB. In tests, we might need to be careful if EF tracks them.
            // Since we are getting 'settings' from cache, let's modify it directly.
            settings.Features[0].Title = "Feature 1 Updated";
            settings.Features.Add(new SiteFeature { Title = "Feature 2", Icon = "Icon2", Order = 2 });

            await service.UpdateSettingsAsync(settings);

            // Assert 2
            using (var verifyContext = new MBETDbContext(_options, _mockUserService.Object))
            {
                var dbSettings = await verifyContext.GlobalSettings.Include(s => s.Features).FirstAsync();
                Assert.Equal(2, dbSettings.Features.Count);
                Assert.Contains(dbSettings.Features, f => f.Title == "Feature 1 Updated");
                Assert.Contains(dbSettings.Features, f => f.Title == "Feature 2");
            }
        }
    }
}