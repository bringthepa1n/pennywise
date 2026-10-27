using PennyWise.Services;

namespace PennyWise;

public partial class App : Application
{
    private readonly IServiceProvider _services;

    public App(IServiceProvider services)
    {
        InitializeComponent();
        _services = services;

        // Apply the saved theme before the first window is created so there is
        // no flash of the wrong colour scheme.
        var themeService = _services.GetRequiredService<ThemeService>();
        themeService.Apply(themeService.LoadSaved(), persist: false);

        // Keep the palette in sync when the user flips the OS theme while the
        // app follows the system setting.
        RequestedThemeChanged += (_, args) => themeService.OnPlatformThemeChanged(args.RequestedTheme);
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var shell = _services.GetRequiredService<AppShell>();
        return new Window(shell);
    }
}
