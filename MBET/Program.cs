using MBET.Core.Entities.Identity;
using MBET.Core.Interfaces;
using MBET.Infrastructure;
using MBET.Infrastructure.Persistence;
using MBET.web.Bootstrap;
using MBET.web.Components;
using MBET.web.Services;
using MBET.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Globalization;

namespace MBET
{
    public class Program
    {
        // CHANGED: async Task Main allows non-blocking database operations on startup
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.ConfigureLogging();

            try
            {
                Log.Information("Starting MBET Platform...");

                builder.Services.AddControllers();

                // 1. ADD LOCALIZATION SERVICES (Isolated)
                builder.Services.AddLocalizationSupport();
                // --- ALGERIA MARKET SUPPORT (Default: English) ---
                builder.Services.Configure<RequestLocalizationOptions>(options =>
                {
                    var supportedCultures = new[]
                    {
                        new CultureInfo("en-US"), // Default
                        new CultureInfo("ar-DZ"), // Algeria (Arabic)
                        new CultureInfo("fr-FR")  // Algeria (Business French)
                    };

                    // KEEP ENGLISH AS DEFAULT
                    options.DefaultRequestCulture = new RequestCulture("en-US");
                    options.SupportedCultures = supportedCultures;
                    options.SupportedUICultures = supportedCultures;
                });

                // --- CRITICAL: REGISTER CURRENT USER SERVICE ---
                // 2. Add HttpContextAccessor so we can access the user
                builder.Services.AddHttpContextAccessor();

                // 3. Map the Interface to the Concrete Web Implementation
                builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

                builder.Services.AddPresentationLayer();

                // Add services to the container.
                builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

                // 4. ADD INFRASTRUCTURE & AUTHENTICATION 
                // This connects the Database and Identity configuration we built
                builder.Services.AddInfrastructure(builder.Configuration);

                // This connects the Blazor UI to the Identity System so the UI knows when you log in/out
                builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<ApplicationUser>>();

                // 4.5 ADD APPLICATION SERVICES
                builder.Services.AddTransient<IProductEditorService, ProductEditorService>();

                var app = builder.Build();

                // 5. AUTOMATIC DATABASE MIGRATION & SEEDING
                // This block runs immediately on startup to ensure the DB exists and is up to date.
                using (var scope = app.Services.CreateScope())
                {
                    try
                    {
                        var context = scope.ServiceProvider.GetRequiredService<MBETDbContext>();

                        Log.Information("Checking for database migrations...");

                        // MigrateAsync is safer in async context
                        await context.Database.MigrateAsync();

                        // Run Seeder
                        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

                        await DbInitializer.SeedAsync(userManager, roleManager, context);

                        Log.Information("Database seeded successfully.");
                        Log.Information("Database is up to date.");
                    }
                    catch (Exception ex)
                    {
                        // If migration fails, we must stop the app because it cannot function without a valid DB.
                        Log.Fatal(ex, "An error occurred while initializing the database.");
                        throw;
                    }
                }

                if (!app.Environment.IsDevelopment())
                {
                    app.UseExceptionHandler("/Error");
                    app.UseHsts();
                }

                // a. Create the uploads directory if it doesn't exist
                var uploadPath = Path.Combine(app.Environment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                // b. Standard Static Files (wwwroot)
                app.UseStaticFiles();

                // c. EXPLICITLY serve the uploads folder
                // This fixes the 404 issue on SmarterASP/IIS for runtime-created folders
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadPath),
                    RequestPath = "/uploads"
                });

                app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

                app.UseHttpsRedirection();
                app.UseAntiforgery();
                app.MapStaticAssets();

                // 6. APPLY LOCALIZATION MIDDLEWARE (Isolated)
                app.UseLocalizationSupport();

                // 7. ENABLE AUTHENTICATION & AUTHORIZATION (New)
                // Must be placed AFTER StaticAssets/Localization and BEFORE MapControllers/MapRazorComponents
                app.UseAuthentication();
                app.UseAuthorization();
                app.MapControllers();

                app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

                // CHANGED: RunAsync allows the thread to yield while the server is running
                await app.RunAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application start-up failed");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}