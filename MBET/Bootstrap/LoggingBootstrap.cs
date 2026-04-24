using Serilog;
using System.Globalization;


namespace MBET.web.Bootstrap
{
    internal static class LoggingBootstrap
    {
        /// <summary>
        /// Configures Serilog as the primary logging provider.
        /// Reads configuration strictly from appsettings.json.
        /// </summary>
        public static void ConfigureLogging(this WebApplicationBuilder builder)
        {
            // 1. Create the initial logger for startup diagnostics
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
                .CreateBootstrapLogger();

            builder.Host.UseSerilog((context, services, configuration) => configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext());
        }
    }
}
