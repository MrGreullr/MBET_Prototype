using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

namespace MBET.web.Bootstrap
{
    public static class LocalizationExtensions
    {
        public static IServiceCollection AddLocalizationSupport(this IServiceCollection services)
        {
            // FIX: Remove 'options.ResourcesPath = "Resources"'.
            // Since we use the 'L' marker class which sits inside the Resources folder/namespace,
            // we rely on Type-based lookup which is much more robust.
            services.AddLocalization();

            return services;
        }

        public static IApplicationBuilder UseLocalizationSupport(this IApplicationBuilder app)
        {
            var supportedCultures = new[]
            {
                new CultureInfo("en-US"), // Default
                new CultureInfo("fr-FR"), // French
                new CultureInfo("ar-DZ")  // Arabic (Algeria)
            };

            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en-US"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures,
                // Critical: This ensures the cookie "AspNetCore.Culture" takes precedence
                RequestCultureProviders = new List<IRequestCultureProvider>
                {
                    new CookieRequestCultureProvider()
                }
            });

            return app;
        }
    }
}