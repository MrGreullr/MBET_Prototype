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
    public class ProductRepositoryTests
    {
        private readonly DbContextOptions<MBETDbContext> _options;
        private readonly Mock<ICurrentUserService> _mockUserService;

        public ProductRepositoryTests()
        {
            _options = new DbContextOptionsBuilder<MBETDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _mockUserService = new Mock<ICurrentUserService>();
            _mockUserService.Setup(s => s.UserId).Returns(Guid.NewGuid());
        }

        private async Task SeedDataAsync()
        {
            using var context = new MBETDbContext(_options, _mockUserService.Object);

            var cat1 = new Category { Id = Guid.NewGuid(), Name = "GPU", IsActive = true };
            var cat2 = new Category { Id = Guid.NewGuid(), Name = "CPU", IsActive = true };
            context.Categories.AddRange(cat1, cat2);

            context.Products.AddRange(
                // Old Priority
                new Product { Title = "Old Priority", CategoryId = cat1.Id, Price = 100, IsPriority = true, IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-10) },
                // New Priority
                new Product { Title = "New Priority", CategoryId = cat1.Id, Price = 200, IsPriority = true, IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-1) },
                // New Regular
                new Product { Title = "New Regular", CategoryId = cat1.Id, Price = 50, IsPriority = false, IsActive = true, CreatedAt = DateTime.UtcNow },
                // Hidden Product (IsVisible = false)
                new Product { Title = "Hidden Gem", CategoryId = cat1.Id, Price = 500, IsActive = true, IsVisible = false, CreatedAt = DateTime.UtcNow },
                // Deleted Product (IsActive = false)
                new Product { Title = "Deleted Item", CategoryId = cat1.Id, Price = 0, IsActive = false, IsVisible = true, CreatedAt = DateTime.UtcNow }
            );

            await context.SaveChangesAsync();
        }

        private ProductRepository CreateRepo()
        {
            var mockFactory = new Mock<IDbContextFactory<MBETDbContext>>();
            // Ensure a NEW context is returned every time CreateDbContextAsync is called
            mockFactory.Setup(f => f.CreateDbContextAsync(default))
                       .ReturnsAsync(() => new MBETDbContext(_options, _mockUserService.Object));

            return new ProductRepository(mockFactory.Object);
        }

        [Fact]
        public async Task GetProductsAsync_ShouldFilterByPriceAndCategory()
        {
            // ... existing test code ...
            await SeedDataAsync();
            var repo = CreateRepo();

            using var context = new MBETDbContext(_options, _mockUserService.Object);
            var gpuCat = await context.Categories.FirstAsync(c => c.Name == "GPU");

            var result = await repo.GetProductsAsync(categoryId: gpuCat.Id, maxPrice: 150);

            Assert.Contains(result.Items, p => p.Title == "Old Priority");
            Assert.Contains(result.Items, p => p.Title == "New Regular");
            Assert.DoesNotContain(result.Items, p => p.Title == "New Priority");
        }

        [Fact]
        public async Task GetLandingProductsAsync_ShouldRespect_NewestMode()
        {
            // Arrange
            await SeedDataAsync();
            var repo = CreateRepo();

            // Act
            var result = await repo.GetLandingProductsAsync(3, ProductDisplayMode.Newest);
            var list = result.ToList();

            // Assert: Should be ordered by Date Descending
            Assert.Equal("New Regular", list[0].Title);
            Assert.Equal("New Priority", list[1].Title);
            Assert.Equal("Old Priority", list[2].Title);

            // Should NOT contain hidden items
            Assert.DoesNotContain(list, p => p.Title == "Hidden Gem");
            // Should NOT contain deleted items
            Assert.DoesNotContain(list, p => p.Title == "Deleted Item");
        }

        [Fact]
        public async Task GetLandingProductsAsync_ShouldRespect_PriorityMode()
        {
            // Arrange
            await SeedDataAsync();
            var repo = CreateRepo();

            // Act
            var result = await repo.GetLandingProductsAsync(3, ProductDisplayMode.Priority);
            var list = result.ToList();

            // Assert: Priority items first, ordered by date desc within priority
            Assert.Equal("New Priority", list[0].Title); // Priority + Newer
            Assert.Equal("Old Priority", list[1].Title); // Priority + Older
            Assert.Equal("New Regular", list[2].Title);  // Non-Priority
        }

        [Fact]
        public async Task GetLandingProductsAsync_ShouldRespect_CountLimit()
        {
            // Arrange
            await SeedDataAsync();
            var repo = CreateRepo();

            // Act
            var result = await repo.GetLandingProductsAsync(1, ProductDisplayMode.Newest);

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public async Task GetLandingProductsAsync_ShouldReturnItems_InRandomMode()
        {
            // Arrange
            await SeedDataAsync();
            var repo = CreateRepo();

            // Act
            var result = await repo.GetLandingProductsAsync(3, ProductDisplayMode.Random);

            // Assert: We can't predict order, but we can verify it returns active/visible items
            Assert.Equal(3, result.Count());
            Assert.DoesNotContain(result, p => p.Title == "Hidden Gem");
            Assert.DoesNotContain(result, p => p.Title == "Deleted Item");
        }
    }
}