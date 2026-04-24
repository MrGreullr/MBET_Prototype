using MBET.Core.Entities;
using MBET.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MBET.Infrastructure.Persistence.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {

        private readonly string _encryptionKey;

        public OrderConfiguration(string encryptionKey)
        {
            _encryptionKey = encryptionKey;
        }
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            // Relationship: When an Order is deleted, delete all its items (Cascade)
            builder.HasMany(o => o.Items)
                   .WithOne(i => i.Order)
                   .HasForeignKey(i => i.OrderId)
                   .OnDelete(DeleteBehavior.Cascade);

            // SQL Server Requirement: Define precision for money
            builder.Property(o => o.Subtotal).HasPrecision(18, 2);
            builder.Property(o => o.TaxAmount).HasPrecision(18, 2);
            builder.Property(o => o.ShippingFee).HasPrecision(18, 2);
            builder.Property(o => o.GrandTotal).HasPrecision(18, 2);

            // 2. Encryption Logic
            var encryptionConverter = new AesEncryptionConverter(_encryptionKey);

            builder.Property(u => u.CustomerName).HasConversion(encryptionConverter);
            builder.Property(u => u.CustomerPhone).HasConversion(encryptionConverter);
            builder.Property(u => u.ShippingAddress).HasConversion(encryptionConverter);

        }
    }

    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            // SQL Server Requirement: Define precision for money
            builder.Property(i => i.UnitPrice)
                   .HasPrecision(18, 2);
        }
    }
}