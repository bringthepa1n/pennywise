using PennyWise.Models;
using PennyWise.Resources.Styles;

namespace PennyWise.Services;

/// <summary>
/// Applies the three theme states required by the assignment.
///
/// Two things happen on every switch:
///  1. the merged colour-role dictionary (LightTheme / DarkTheme, generated
///     from Colors.xaml) is swapped, so every {DynamicResource ...} style and
///     page background re-resolves immediately;
///  2. the platform theme is set. For "System" that is AppTheme.Unspecified,
///     so Android keeps following the OS setting, and RequestedThemeChanged
///     re-applies the matching palette.
/// </summary>
public class ThemeService
{
    private const string PreferenceKey = "pennywise.theme";
    private const string ThemeSlotKey = "Theme";

    public ThemeMode Current { get; private set; } = ThemeMode.System;

    public void Apply(ThemeMode mode, bool persist = true)
    {
        Current = mode;

        if (Application.Current is { } app)
        {
            app.UserAppTheme = mode switch
            {
                ThemeMode.Light => AppTheme.Light,
                ThemeMode.Dark => AppTheme.Dark,
                _ => AppTheme.Unspecified,
            };

            var dark = mode switch
            {
                ThemeMode.Dark => true,
                ThemeMode.Light => false,
                _ => app.PlatformAppTheme == AppTheme.Dark || app.RequestedTheme == AppTheme.Dark,
            };

            SwapRoleDictionary(app, dark);
        }

        if (persist)
        {
            Preferences.Default.Set(PreferenceKey, mode.ToString());
        }
    }

    /// <summary>Re-applies the palette when the OS theme changes while "System" is active.</summary>
    public void OnPlatformThemeChanged(AppTheme theme)
    {
        if (Current == ThemeMode.System && Application.Current is { } app)
        {
            SwapRoleDictionary(app, theme == AppTheme.Dark);
        }
    }

    public ThemeMode LoadSaved()
    {
        var stored = Preferences.Default.Get(PreferenceKey, ThemeMode.System.ToString());
        return Enum.TryParse<ThemeMode>(stored, out var mode) ? mode : ThemeMode.System;
    }

    private static void SwapRoleDictionary(Application app, bool dark)
    {
        // Setting Source from code is not allowed in MAUI (XAML only), so both
        // dictionaries exist as compiled types and only the slot is replaced.
        ResourceDictionary dictionary = dark ? new DarkTheme() : new LightTheme();

        app.Resources[ThemeSlotKey] = dictionary;

        // Keep MergedDictionaries in the documented order:
        // [0] base palette, [1] active roles, [2] styles.
        var merged = app.Resources.MergedDictionaries;
        if (merged.Count > 1)
        {
            var list = merged as IList<ResourceDictionary>;
            if (list is not null)
            {
                list[1] = dictionary;
            }
        }
    }
}
