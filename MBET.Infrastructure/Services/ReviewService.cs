using MBET.Core.Entities;
using MBET.Core.Interfaces;
using MBET.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MBET.Infrastructure.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IDbContextFactory<MBETDbContext> _contextFactory;

        public ReviewService(IDbContextFactory<MBETDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<ProductReview>> GetReviewsByProductAsync(Guid productId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.ProductReviews
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<ProductReview> AddOrUpdateReviewAsync(Guid productId, Guid userId, string userName, int rating, string? comment)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            // 1. Check for existing review
            var existingReview = await context.ProductReviews
                .FirstOrDefaultAsync(r => r.ProductId == productId && r.UserId == userId);

            ProductReview reviewToReturn;

            if (existingReview != null)
            {
                // UPDATE
                existingReview.Rating = rating;
                existingReview.Comment = comment;
                existingReview.UserName = userName; // Update name in case user changed it
                existingReview.LastModifiedAt = DateTime.UtcNow;
                
                context.ProductReviews.Update(existingReview);
                reviewToReturn = existingReview;
            }
            else
            {
                // INSERT
                var newReview = new ProductReview
                {
                    ProductId = productId,
                    UserId = userId,
                    UserName = userName,
                    Rating = rating,
                    Comment = comment
                };
                
                context.ProductReviews.Add(newReview);
                reviewToReturn = newReview;
            }

            await context.SaveChangesAsync();

            // 2. Recalculate Product Stats
            await UpdateProductStatsAsync(context, productId);

            return reviewToReturn;
        }

        public async Task<bool> DeleteReviewAsync(Guid reviewId, Guid userId, bool isAdmin)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var review = await context.ProductReviews.FindAsync(reviewId);
            if (review == null) return false;

            // Permission Check
            if (!isAdmin && review.UserId != userId) return false;

            var productId = review.ProductId; // Store ID for recalculation

            context.ProductReviews.Remove(review);
            await context.SaveChangesAsync();

            // Recalculate Product Stats
            await UpdateProductStatsAsync(context, productId);

            return true;
        }

        private async Task UpdateProductStatsAsync(MBETDbContext context, Guid productId)
        {
            // Calculate stats using DB Aggregation
            var stats = await context.ProductReviews
                .Where(r => r.ProductId == productId)
                .GroupBy(r => r.ProductId)
                .Select(g => new
                {
                    Count = g.Count(),
                    Avg = g.Average(r => (double)r.Rating)
                })
                .FirstOrDefaultAsync();

            var product = await context.Products.FindAsync(productId);
            if (product != null)
            {
                if (stats != null)
                {
                    product.Rating = Math.Round(stats.Avg, 1); // Round to 1 decimal place
                    product.ReviewCount = stats.Count;
                }
                else
                {
                    // No reviews left
                    product.Rating = 5.0; // Reset to default
                    product.ReviewCount = 0;
                }
                
                await context.SaveChangesAsync();
            }
        }
    }
}