using MBET.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MBET.Core.Interfaces
{
    public interface IReviewService
    {
        /// <summary>
        /// Retrieves reviews for a product, ordered by newest first.
        /// </summary>
        Task<List<ProductReview>> GetReviewsByProductAsync(Guid productId);

        /// <summary>
        /// Adds a new review or updates an existing one if the user has already voted.
        /// Automatically recalculates the Product's average rating.
        /// </summary>
        Task<ProductReview> AddOrUpdateReviewAsync(Guid productId, Guid userId, string userName, int rating, string? comment);

        /// <summary>
        /// Deletes a review (Owner or Admin) and recalculates rating.
        /// </summary>
        Task<bool> DeleteReviewAsync(Guid reviewId, Guid userId, bool isAdmin);
    }
}