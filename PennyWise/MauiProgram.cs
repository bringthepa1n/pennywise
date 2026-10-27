using Microsoft.Extensions.Logging;
using PennyWise.Services;
using PennyWise.ViewModels;
using PennyWise.Views;

namespace PennyWise;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Storage + domain services (single instances keep the in-memory state
        // consistent between the dashboard, the form and the JSON screen).
        builder.Services.AddSingleton<JsonStorageService>();
        builder.Services.AddSingleton<TransactionService>();
        builder.Services.AddSingleton<ProfileService>();
        builder.Services.AddSingleton<SettingsService>();
        builder.Services.AddSingleton<ThemeService>();

        // View models
        builder.Services.AddSingleton<DashboardViewModel>();
        builder.Services.AddTransient<AddTransactionViewModel>();
        builder.Services.AddTransient<WebViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<JsonViewModel>();

        // Pages
        builder.Services.AddSingleton<DashboardPage>();
        builder.Services.AddTransient<AddTransactionPage>();
        builder.Services.AddTransient<WebPage>();
        builder.Services.AddTransient<Settings>();
        builder.Services.AddTransient<JsonPage>();
        builder.Services.AddSingleton<AppShell>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
