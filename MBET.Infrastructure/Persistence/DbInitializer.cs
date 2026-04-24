using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MBET.Core.Entities.Identity;
using MBET.Core.Entities; // Required for Category
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MBET.Infrastructure.Persistence
{
    public static class DbInitializer
    {
        // CHANGED: Added MBETDbContext context parameter
        public static async Task SeedAsync(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, MBETDbContext context)
        {
            // 1. Define Roles
            string[] roleNames = { "SuperAdmin", "Administrator", "ShopManager", "SupportAgent", "Customer" };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new ApplicationRole(roleName, $"Standard {roleName} role"));
                }
            }

            // 2. Create SuperAdmin if not exists
            var adminEmail = "admin@mbet.io";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Super",
                    LastName = "Admin",
                    EmailConfirmed = true,
                    IsActive = true,
                    IsVIP = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123456");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "SuperAdmin");
                    await userManager.AddToRoleAsync(adminUser, "Administrator");
                }
            }

            // 3. Seed Categories (NEW)
            // Check if any categories exist
            if (!await context.Categories.AnyAsync())
            {
                var categories = new List<Category>
                {
                    new Category
                    {
                        Name = "GPU",
                        Slug = "gpu",
                        Description = "Graphics Processing Units for high-performance gaming and rendering.",
                        Icon = "Icons.Material.Filled.Memory"
                    },
                    new Category
                    {
                        Name = "Cooling",
                        Slug = "cooling",
                        Description = "Advanced thermal solutions including AIOs and custom blocks.",
                        Icon = "Icons.Material.Filled.AcUnit"
                    },
                    new Category
                    {
                        Name = "Peripherals",
                        Slug = "peripherals",
                        Description = "Keyboards, mice, and headsets designed for precision.",
                        Icon = "Icons.Material.Filled.Keyboard"
                    },
                    new Category
                    {
                        Name = "Memory",
                        Slug = "memory",
                        Description = "High-speed RAM modules for next-gen platforms.",
                        Icon = "Icons.Material.Filled.SdStorage"
                    },
                    new Category
                    {
                        Name = "Motherboards",
                        Slug = "motherboards",
                        Description = "The backbone of your system.",
                        Icon = "Icons.Material.Filled.DeveloperBoard"
                    }
                };

                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
            }
        }
    }
}