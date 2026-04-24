using MBET.Core.Entities.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;


using System;
using System.Collections.Generic;
using System.Text;

namespace MBET.Infrastructure.Persistence.Configurations
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        private readonly string _encryptionKey;

        public ApplicationUserConfiguration(string encryptionKey)
        {
            _encryptionKey = encryptionKey;
        }

        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            // 1. Identity Basics
            builder.ToTable("Users");
            builder.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
            builder.Property(u => u.LastName).HasMaxLength(100).IsRequired();
            builder.Property(u => u.IsActive).HasDefaultValue(true);
            builder.HasIndex(u => u.TenantId);

            // 2. Encryption Logic
            // We instantiate the converter here using the key passed from the Context
            var encryptionConverter = new AesEncryptionConverter(_encryptionKey);

            // --- IDENTITY PII ---
            builder.Property(u => u.FirstName).HasConversion(encryptionConverter);
            builder.Property(u => u.LastName).HasConversion(encryptionConverter);
            builder.Property(u => u.PhoneNumber).HasConversion(encryptionConverter);

            // --- SHIPPING PII ---
            builder.Property(u => u.ShippingStreet).HasConversion(encryptionConverter);
            builder.Property(u => u.ShippingCity).HasConversion(encryptionConverter);
            builder.Property(u => u.ShippingZipCode).HasConversion(encryptionConverter);

            // --- BILLING PII ---
            builder.Property(u => u.BillingStreet).HasConversion(encryptionConverter);
            builder.Property(u => u.BillingCity).HasConversion(encryptionConverter);
            builder.Property(u => u.BillingZipCode).HasConversion(encryptionConverter);
        }
    }
}
