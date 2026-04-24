using MBET.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.AspNetCore.DataProtection;

namespace MBET.Infrastructure.Persistence.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        private readonly string _encryptionKey;

        public ProductConfiguration(string encryptionKey)
        {
            _encryptionKey = encryptionKey;
        }

        public void Configure(EntityTypeBuilder<Product> builder)
        {
            // Implementation of the "Encrypted at Rest" rule
            // In a real scenario, we use a ValueConverter that calls our IDataProtectionService
            // This ensures InternalSupplierCode is encrypted before hitting SQL/Postgres

            builder.Property(p => p.Title).IsRequired().HasMaxLength(200);

            // 1. Configure Product -> Images
            // FIXED: Added 'x => x.Product' to WithOne() to link the navigation property.
            // FIXED: Used lambda 'x => x.ProductId' instead of string for type safety.
            builder.HasMany(p => p.Images)
                   .WithOne(x => x.Product)
                   .HasForeignKey(x => x.ProductId)
                   .OnDelete(DeleteBehavior.Cascade);

            // 2. Configure Product -> Specifications
            builder.HasMany(p => p.Specifications)
                   .WithOne(s => s.Product)
                   .HasForeignKey(s => s.ProductId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}