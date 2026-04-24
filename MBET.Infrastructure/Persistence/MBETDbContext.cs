using MBET.Core.Entities;
using MBET.Core.Interfaces;
using MBET.Infrastructure.Entities;
using MBET.Core.Entities.Identity;
using MBET.Infrastructure.Persistence.Configurations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MBET.Infrastructure.Persistence
{
    public class MBETDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        private readonly ICurrentUserService _currentUserService;

        // TODO: Move key to secure configuration (this is just a place holder!)
        private readonly string _encryptionKey = "12345678901234567890123456789012";

        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<GlobalSettings> GlobalSettings { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Product> Products { get; set; }

        public DbSet<ProductSpecification> ProductSpecifications { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<ProductReview> ProductReviews { get; set; }






        // Constructor accepting ICurrentUserService for auditing
        public MBETDbContext(
            DbContextOptions<MBETDbContext> options,
            ICurrentUserService currentUserService)
            : base(options)
        {
            _currentUserService = currentUserService;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // 1. Rename Identity Tables
            builder.Entity<ApplicationUser>(b => b.ToTable("Users"));
            builder.Entity<ApplicationRole>(b => b.ToTable("Roles"));
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<Guid>>(b => b.ToTable("UserRoles"));
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<Guid>>(b => b.ToTable("UserClaims"));
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<Guid>>(b => b.ToTable("UserLogins"));
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityRoleClaim<Guid>>(b => b.ToTable("RoleClaims"));
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserToken<Guid>>(b => b.ToTable("UserTokens"));

            // 2. Apply External Configurations (Encryption, etc.)
            builder.ApplyConfiguration(new ApplicationUserConfiguration(_encryptionKey));
            builder.ApplyConfiguration(new ProductConfiguration(_encryptionKey));
            builder.ApplyConfiguration(new CartConfiguration());
            builder.ApplyConfiguration(new CartItemConfiguration());
            builder.ApplyConfiguration(new OrderConfiguration(_encryptionKey));
            builder.ApplyConfiguration(new OrderItemConfiguration());
            builder.ApplyConfiguration(new ProductReviewConfiguration());




            // 3. Fix Decimal Precisions (SQL Server Requirement)
            builder.Entity<GlobalSettings>(b =>
            {
                b.Property(p => p.DefaultTaxRate).HasPrecision(18, 4); // 4 decimal places for tax (e.g. 0.1900)
                b.Property(p => p.FreeShippingThreshold).HasPrecision(18, 2); // Currency
            });

            builder.Entity<ApplicationUser>(b =>
            {
                // Fix for warning "No store type was specified for BanDuratyionDays"
                b.Property(u => u.BanDuratyionDays).HasPrecision(18, 2);
            });
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            // 1. Handle IAuditableEntity (Timestamps)
            foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        entry.Entity.CreatedBy = _currentUserService.UserId;
                        break;

                    case EntityState.Modified:
                        entry.Entity.LastModifiedAt = DateTime.UtcNow;
                        entry.Entity.LastModifiedBy = _currentUserService.UserId;
                        break;
                }
            }

            // 2. Commit to DB
            var result = await base.SaveChangesAsync(cancellationToken);

            return result;
        }
    }
}