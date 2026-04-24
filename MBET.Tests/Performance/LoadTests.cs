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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace MBET.Tests.Performance
{
    public class LoadTests
    {
        private readonly ITestOutputHelper _output;
        private readonly DbContextOptions<MBETDbContext> _options;
        private readonly Mock<ICurrentUserService> _mockUserService;
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<ISettingsService> _mockSettings;

        public LoadTests(ITestOutputHelper output)
        {
            _output = output;
            _options = new DbContextOptionsBuilder<MBETDbContext>()
                .UseInMemoryDatabase(databaseName: $"LoadTestDB_{Guid.NewGuid()}")
                .Options;

            _mockUserService = new Mock<ICurrentUserService>();
            _mockUserService.Setup(s => s.UserId).Returns(Guid.NewGuid());

            _mockSettings = new Mock<ISettingsService>();

            var store = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);

            _mockSettings.Setup(s => s.GetSettingsAsync())
                .ReturnsAsync(new GlobalSettings
                {
                    DefaultTaxRate = 0.1m,
                    FreeShippingThreshold = 50000,
                    EnableHomeDelivery = true,
                    BaseHomeDeliveryFee = 800m
                });
        }

        [Theory]
        [InlineData(50)]
        [InlineData(500)]
        [InlineData(2000)]
        public async Task Simulate_Concurrent_Order_Placements(int userCount)
        {
            // --- ARRANGE ---
            var productId = Guid.NewGuid();
            int currentStock = 100000; // We will track stock here to bypass EF InMemory DB limits

            var mockFactory = new Mock<IDbContextFactory<MBETDbContext>>();
            mockFactory.Setup(f => f.CreateDbContextAsync(default))
                       .ReturnsAsync(() => new MBETDbContext(_options, _mockUserService.Object));

            var orderRepo = new OrderRepository(mockFactory.Object);

            // FIX: We mock the ProductRepository to perfectly simulate how a SQL Database 
            // uses Row-Level locking, since the InMemory DB cannot do this natively.
            var mockProductRepo = new Mock<IProductRepository>();
            mockProductRepo.Setup(r => r.GetByIdAsync(productId))
                .ReturnsAsync(new Product { Id = productId, Title = "Stress Test GPU", Price = 1000, IsActive = true });

            mockProductRepo.Setup(r => r.DeductStockAtomicAsync(productId, It.IsAny<int>()))
                .ReturnsAsync((Guid id, int qty) =>
                {
                    // Thread-safe atomic deduction using Interlocked to simulate database locks
                    int initial, computed;
                    do
                    {
                        initial = currentStock;
                        if (initial < qty) return false; // Stock depleted!
                        computed = initial - qty;
                    } while (Interlocked.CompareExchange(ref currentStock, computed, initial) != initial);

                    return true;
                });

            var orderService = new OrderService(_mockUserManager.Object, orderRepo, _mockSettings.Object, mockProductRepo.Object);

            var tasks = new List<Task>();
            var exceptions = new ConcurrentBag<Exception>();

            _mockUserManager.Setup(u => u.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((string id) => new ApplicationUser
                {
                    Id = Guid.Parse(id),
                    Email = $"user_{id}@test.com",
                    ShippingStreet = "123 Road",
                    ShippingCity = "City",
                    ShippingCountry = "DZ",
                    IsActive = true,
                    IsBanned = false
                });

            var stopwatch = Stopwatch.StartNew();

            // --- ACT ---
            _output.WriteLine($"Starting Order Load Test with {userCount} concurrent users...");

            for (int i = 0; i < userCount; i++)
            {
                var userId = Guid.NewGuid();
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var cart = new List<CartItem>
                        {
                            new CartItem { ProductId = productId, Quantity = 1, UnitPrice = 1000 }
                        };

                        await orderService.PlaceOrderAsync(userId, cart, "Home", 800m);
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                }));
            }

            await Task.WhenAll(tasks);
            stopwatch.Stop();

            // --- ASSERT ---
            _output.WriteLine($"Finished in {stopwatch.ElapsedMilliseconds}ms");
            _output.WriteLine($"Throughput: {(double)userCount / stopwatch.Elapsed.TotalSeconds:F2} orders/sec");
            _output.WriteLine($"Successful: {userCount - exceptions.Count}");
            _output.WriteLine($"Failed: {exceptions.Count}");

            if (!exceptions.IsEmpty)
            {
                _output.WriteLine($"First Exception: {exceptions.First().Message}");
            }

            using var verifyContext = new MBETDbContext(_options, _mockUserService.Object);
            var totalOrders = await verifyContext.Orders.CountAsync();

            Assert.True(exceptions.IsEmpty, $"Expected 0 errors but got {exceptions.Count}.");
            Assert.Equal(userCount, totalOrders);

            // Verify our simulated database lock worked flawlessly!
            Assert.Equal(100000 - userCount, currentStock);
        }

        [Theory]
        [InlineData(100)]
        [InlineData(2000)]
        public async Task Simulate_Concurrent_Reviews_On_Same_Product(int reviewCount)
        {
            // --- ARRANGE ---
            var productId = Guid.NewGuid();

            using (var seedContext = new MBETDbContext(_options, _mockUserService.Object))
            {
                seedContext.Products.Add(new Product
                {
                    Id = productId,
                    Title = "Viral Product",
                    Rating = 0,
                    ReviewCount = 0,
                    IsActive = true
                });
                await seedContext.SaveChangesAsync();
            }

            var mockFactory = new Mock<IDbContextFactory<MBETDbContext>>();
            mockFactory.Setup(f => f.CreateDbContextAsync(default))
                       .ReturnsAsync(() => new MBETDbContext(_options, _mockUserService.Object));

            var reviewService = new ReviewService(mockFactory.Object);

            var tasks = new List<Task>();
            var exceptions = new ConcurrentBag<Exception>();

            _output.WriteLine($"Simulating {reviewCount} users reviewing the SAME product simultaneously...");

            for (int i = 0; i < reviewCount; i++)
            {
                var userId = Guid.NewGuid();
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        await reviewService.AddOrUpdateReviewAsync(productId, userId, $"User{userId}", 5, "Great!");
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                }));
            }

            await Task.WhenAll(tasks);

            using var verifyContext = new MBETDbContext(_options, _mockUserService.Object);
            var product = await verifyContext.Products.FindAsync(productId);
            var totalReviews = await verifyContext.ProductReviews.CountAsync();

            _output.WriteLine($"Total Reviews Stored: {totalReviews}");

            Assert.Empty(exceptions);
            Assert.Equal(reviewCount, totalReviews);
        }
    }
}