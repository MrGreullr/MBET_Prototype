using MBET.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MBET.Infrastructure.Persistence.Configurations
{
    public class ProductReviewConfiguration : IEntityTypeConfiguration<ProductReview>
    {
        public void Configure(EntityTypeBuilder<ProductReview> builder)
        {
            // Table Name
            builder.ToTable("ProductReviews");

            // Constraints
            builder.Property(r => r.UserName).IsRequired().HasMaxLength(100);
            builder.Property(r => r.Comment).HasMaxLength(2000);
            builder.Property(r => r.Rating).IsRequired();

            // Unique Index: One review per user per product
            // This prevents duplicate reviews at the database level
            builder.HasIndex(r => new { r.ProductId, r.UserId }).IsUnique();
        }
    }
}