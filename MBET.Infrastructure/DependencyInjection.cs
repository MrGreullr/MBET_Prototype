using MBET.Core.Entities;
using MBET.Core.Interfaces;
using MBET.Core.Entities.Identity;
using MBET.Infrastructure.Persistence;
using MBET.Infrastructure.Repositories;
using MBET.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace MBET.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var provider = configuration.GetValue("DatabaseProvider", "SqlServer");
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // 1. Define Database Options Logic (Reusable)
            Action<DbContextOptionsBuilder> dbOptions = options =>
            {
                if (provider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
                {
                    options.UseNpgsql(connectionString,
                        b => b.MigrationsAssembly(typeof(MBETDbContext).Assembly.FullName));
                }
                else
                {
                    options.UseSqlServer(connectionString,
                        b => b.MigrationsAssembly(typeof(MBETDbContext).Assembly.FullName));
                }
            };

            // 2. Register DB Context Factory (For Blazor Components & Repositories)
            // CRITICAL: Lifetime must be Scoped because MBETDbContext depends on ICurrentUserService (which is Scoped)
            services.AddDbContextFactory<MBETDbContext>(dbOptions, lifetime: ServiceLifetime.Scoped);

            // 3. Register Standard DB Context (For Identity & Controllers)
            // Identity uses this instance to manage Users/Roles via HttpContext
            services.AddDbContext<MBETDbContext>(dbOptions);

            // 4. Identity Configuration
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                // Password settings (Dev-friendly)
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings
                options.User.RequireUniqueEmail = true;

                // SignIn settings
                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddEntityFrameworkStores<MBETDbContext>()
            .AddDefaultTokenProviders();

            // 5. Register Repositories
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped(typeof(IProductRepository), typeof(ProductRepository));
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IOrderService, OrderService>();

            // 6. Register Global Settings Service
            services.AddScoped<ISettingsService, SettingsService>();

            // 7. Register Cart Service
            services.AddScoped<CartService>();

            // 8. Register Local Storage Service
            services.AddScoped<IStorageService, LocalStorageService>();

            // 8. Register Review Service
            services.AddScoped<IReviewService, ReviewService>();

            // 9. Register Product Service
            services.AddScoped<IProductService, ProductService>();

            return services;
        }
    }
}