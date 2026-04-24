using MBET.Core.Entities;
using MBET.Core.Interfaces;
using MBET.Infrastructure.Persistence;
using MBET.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MBET.Tests.Services
{
    public class ProductReviewsTests : IDisposable
    {
        private readonly IDbContextFactory<MBETDbContext> _contextFactory;
        private readonly MBETDbContext _context;
        private readonly ReviewService _service;

        public ProductReviewsTests()
        {
            // Setup In-Memory Database
            var options = new DbContextOptionsBuilder<MBETDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB per test class run
                .Options;

            // Setup Mock User Service to prevent NRE in SaveChangesAsync
            var mockUserService = new Mock<ICurrentUserService>();
            mockUserService.Setup(x => x.UserId).Returns(Guid.NewGuid());

            // We need a factory wrapper for the service
            _contextFactory = new TestDbContextFactory(options, mockUserService.Object);
            _context = _contextFactory.CreateDbContext();
            _service = new ReviewService(_contextFactory);
        }

        // Helper class to mock IDbContextFactory for In-Memory testing
        public class TestDbContextFactory : IDbContextFactory<MBETDbContext>
        {
            private readonly DbContextOptions<MBETDbContext> _options;
            private readonly ICurrentUserService _currentUserService;

            public TestDbContextFactory(DbContextOptions<MBETDbContext> options, ICurrentUserService currentUserService)
            {
                _options = options;
                _currentUserService = currentUserService;
            }

            public MBETDbContext CreateDbContext() => new MBETDbContext(_options, _currentUserService);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task AddOrUpdateReview_ShouldAddNew_WhenNoneExists()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var product = new Product { Id = productId, Title = "Test Product", Rating = 0, ReviewCount = 0 };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Act
            await _service.AddOrUpdateReviewAsync(productId, userId, "User1", 5, "Great!");

            // Assert
            _context.ChangeTracker.Clear(); // Ensure we read from DB, not local cache
            var review = await _context.ProductReviews.FirstOrDefaultAsync();
            Assert.NotNull(review);
            Assert.Equal(5, review.Rating);
            Assert.Equal("Great!", review.Comment);

            // Verify Product Stats Updated
            var updatedProduct = await _context.Products.FindAsync(productId);
            Assert.Equal(5.0, updatedProduct.Rating);
            Assert.Equal(1, updatedProduct.ReviewCount);
        }

        [Fact]
        public async Task AddOrUpdateReview_ShouldUpdateExisting_WhenUserAlreadyReviewed()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var product = new Product { Id = productId, Title = "Test Product" };
            _context.Products.Add(product);

            // Initial Review (3 Stars)
            _context.ProductReviews.Add(new ProductReview { ProductId = productId, UserId = userId, Rating = 3, Comment = "Okay" });
            await _context.SaveChangesAsync();

            // Act: Same user reviews again with 5 Stars
            await _service.AddOrUpdateReviewAsync(productId, userId, "User1", 5, "Actually awesome!");

            // Assert
            _context.ChangeTracker.Clear(); // Ensure we read from DB
            var reviews = await _context.ProductReviews.Where(r => r.ProductId == productId).ToListAsync();
            Assert.Single(reviews); // Should still be only 1 review
            Assert.Equal(5, reviews.First().Rating);
            Assert.Equal("Actually awesome!", reviews.First().Comment);

            // Verify Stats
            var updatedProduct = await _context.Products.FindAsync(productId);
            Assert.Equal(5.0, updatedProduct.Rating);
        }

        [Fact]
        public async Task ProductStats_ShouldCalculateAverageCorrectly()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var product = new Product { Id = productId, Title = "Stats Test" };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Act: Add 3 reviews (5, 4, 3)
            await _service.AddOrUpdateReviewAsync(productId, Guid.NewGuid(), "U1", 5, "A");
            await _service.AddOrUpdateReviewAsync(productId, Guid.NewGuid(), "U2", 4, "B");
            await _service.AddOrUpdateReviewAsync(productId, Guid.NewGuid(), "U3", 3, "C");

            // Assert
            _context.ChangeTracker.Clear(); // Ensure we read from DB
            var updatedProduct = await _context.Products.FindAsync(productId);
            Assert.Equal(3, updatedProduct.ReviewCount);
            Assert.Equal(4.0, updatedProduct.Rating); // (5+4+3)/3 = 4
        }

        [Fact]
        public async Task DeleteReview_ShouldRemoveAndRecalculate()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var user1 = Guid.NewGuid();
            var user2 = Guid.NewGuid();

            _context.Products.Add(new Product { Id = productId, Title = "Delete Test" });

            // Review 1 (ID we will target)
            var r1 = new ProductReview { Id = Guid.NewGuid(), ProductId = productId, UserId = user1, Rating = 5 };
            // Review 2
            var r2 = new ProductReview { Id = Guid.NewGuid(), ProductId = productId, UserId = user2, Rating = 1 };

            _context.ProductReviews.AddRange(r1, r2);
            await _context.SaveChangesAsync();

            // Pre-check stats manually (Avg should be 3)
            await _service.AddOrUpdateReviewAsync(productId, user1, "U1", 5, ""); // Just to trigger calc

            // Act: Delete Review 1
            bool result = await _service.DeleteReviewAsync(r1.Id, user1, isAdmin: false);

            // Assert
            Assert.True(result);
            _context.ChangeTracker.Clear(); // Ensure we read from DB
            var remaining = await _context.ProductReviews.ToListAsync();
            Assert.Single(remaining);
            Assert.Equal(1, remaining.First().Rating); // Only R2 remains

            // Verify Stats Recalculation
            var product = await _context.Products.FindAsync(productId);
            Assert.Equal(1.0, product.Rating); // Avg of just {1} is 1
            Assert.Equal(1, product.ReviewCount);
        }

        [Fact]
        public async Task DeleteReview_ShouldFail_IfUserIsNotOwnerAndNotAdmin()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var otherUser = Guid.NewGuid();

            _context.ProductReviews.Add(new ProductReview { Id = reviewId, UserId = ownerId, Rating = 5 });
            await _context.SaveChangesAsync();

            // Act: Other user tries to delete
            bool result = await _service.DeleteReviewAsync(reviewId, otherUser, isAdmin: false);

            // Assert
            Assert.False(result);
            _context.ChangeTracker.Clear(); // Ensure we read from DB
            Assert.NotNull(await _context.ProductReviews.FindAsync(reviewId)); // Still exists
        }
    }
}