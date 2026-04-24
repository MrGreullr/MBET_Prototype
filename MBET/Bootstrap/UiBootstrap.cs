using MudBlazor.Services;

namespace MBET.web.Bootstrap
{
    /// <summary>
    /// Registers UI framework services (MudBlazor).
    /// </summary>
    internal static class UiBootstrap
    {
        /// <summary>
        /// Registers UI framework services (MudBlazor).
        /// </summary>
        public static void AddPresentationLayer(this IServiceCollection services)
        {
            services.AddRazorComponents()
                .AddInteractiveServerComponents();

            services.AddMudServices(config =>
            {
                config.SnackbarConfiguration.PositionClass = MudBlazor.Defaults.Classes.Position.BottomRight;
                config.SnackbarConfiguration.PreventDuplicates = false;
                config.SnackbarConfiguration.NewestOnTop = false;
                config.SnackbarConfiguration.ShowCloseIcon = true;
                config.SnackbarConfiguration.VisibleStateDuration = 5000;
                config.SnackbarConfiguration.HideTransitionDuration = 500;
                config.SnackbarConfiguration.ShowTransitionDuration = 500;
            });
        }
    }
}
    