using PennyWise.Core;
using PennyWise.Models;
using PennyWise.Services;

namespace PennyWise.ViewModels;

/// <summary>Screen 4: theme switch, name/password with live strength metre, JSON write.</summary>
public class SettingsViewModel : BaseViewModel
{
    private readonly ProfileService _profile;
    private readonly ThemeService _theme;
    private readonly SettingsService _settings;
    private string _name = string.Empty;
    private string _password = string.Empty;
    private bool _showPassword;
    private double _budgetValue;
    private string _budgetText = string.Empty;
    private ThemeMode _selectedTheme;
    private string _strengthLabel = string.Empty;
    private double _strengthProgress;
    private string _statusMessage = string.Empty;
    private string? _loadWarning;
    private bool _hasStoredPassword;

    public SettingsViewModel(ProfileService profile, ThemeService theme, SettingsService settings)
    {
        _profile = profile;
        _theme = theme;
        _settings = settings;
        Title = "Nastavení";

        SaveProfileCommand = new AsyncRelayCommand(SaveProfileAsync);
        SaveBudgetCommand = new AsyncRelayCommand(SaveBudgetAsync);
        SwitchThemeCommand = new RelayCommand(parameter => ApplyTheme(parameter));
        UpdateStrength(string.Empty);
    }

    public AsyncRelayCommand SaveProfileCommand { get; }

    public AsyncRelayCommand SaveBudgetCommand { get; }

    public RelayCommand SwitchThemeCommand { get; }

    public string Name
    {
        get => _name;
        set
        {
            if (SetProperty(ref _name, value))
            {
                OnPropertyChanged(nameof(HasNameError));
            }
        }
    }

    public bool HasNameError => string.IsNullOrWhiteSpace(_name);

    public string Password
    {
        get => _password;
        set
        {
            if (SetProperty(ref _password, value))
            {
                UpdateStrength(value);
            }
        }
    }

    public bool ShowPassword
    {
        get => _showPassword;
        set => SetProperty(ref _showPassword, value);
    }

    public string StrengthLabel
    {
        get => _strengthLabel;
        private set => SetProperty(ref _strengthLabel, value);
    }

    public double StrengthProgress
    {
        get => _strengthProgress;
        private set => SetProperty(ref _strengthProgress, value);
    }

    public bool HasMinLength { get; private set; }

    public bool HasDigits { get; private set; }

    public bool HasLowercase { get; private set; }

    public bool HasUppercase { get; private set; }

    public bool HasSpecial { get; private set; }

    public bool HasStoredPassword
    {
        get => _hasStoredPassword;
        private set
        {
            if (SetProperty(ref _hasStoredPassword, value))
            {
                OnPropertyChanged(nameof(StoredPasswordLabel));
            }
        }
    }

    public ThemeMode SelectedTheme
    {
        get => _selectedTheme;
        private set
        {
            if (SetProperty(ref _selectedTheme, value))
            {
                OnPropertyChanged(nameof(IsSystemTheme));
                OnPropertyChanged(nameof(IsLightTheme));
                OnPropertyChanged(nameof(IsDarkTheme));
                OnPropertyChanged(nameof(ThemeLabel));
            }
        }
    }

    public bool IsSystemTheme => _selectedTheme == ThemeMode.System;

    public bool IsLightTheme => _selectedTheme == ThemeMode.Light;

    public bool IsDarkTheme => _selectedTheme == ThemeMode.Dark;

    public double BudgetValue
    {
        get => _budgetValue;
        set
        {
            if (SetProperty(ref _budgetValue, value))
            {
                BudgetText = Services.CurrencyFormatter.Format(RoundToHundreds((decimal)value));
            }
        }
    }

    public string BudgetText
    {
        get => _budgetText;
        private set => SetProperty(ref _budgetText, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set
        {
            if (SetProperty(ref _statusMessage, value))
            {
                OnPropertyChanged(nameof(HasStatus));
            }
        }
    }

    public bool HasStatus => !string.IsNullOrEmpty(_statusMessage);

    public string ThemeLabel => _selectedTheme switch
    {
        ThemeMode.Light => "Světlý",
        ThemeMode.Dark => "Tmavý",
        _ => "Podle systému",
    };

    public string StoredPasswordLabel => HasStoredPassword
        ? "Profil je uložen v profile.json."
        : "Profil zatím není uložen.";

    public Color RuleLengthColor => RuleColor(HasMinLength, "Primary");

    public Color RuleDigitColor => RuleColor(HasDigits, "Primary");

    public Color RuleLowerColor => RuleColor(HasLowercase, "Primary");

    public Color RuleUpperColor => RuleColor(HasUppercase, "Primary");

    public Color RuleSpecialColor => RuleColor(HasSpecial, "Primary");

    private static Color RuleColor(bool fulfilled, string accentKey)
    {
        if (fulfilled)
        {
            return Application.Current?.Resources.TryGetValue(accentKey, out var accent) == true && accent is Color color
                ? color
                : Colors.SeaGreen;
        }

        return Colors.Transparent;
    }

    public string? LoadWarning
    {
        get => _loadWarning;
        private set
        {
            if (SetProperty(ref _loadWarning, value))
            {
                OnPropertyChanged(nameof(HasLoadWarning));
            }
        }
    }

    public bool HasLoadWarning => !string.IsNullOrEmpty(_loadWarning);

    public async Task LoadAsync()
    {
        var profile = await _profile.LoadAsync();
        Name = profile.Name;
        HasStoredPassword = profile.HasPassword;

        LoadWarning = _profile.FileWasCorrupted
            ? $"Uložený profil nelze načíst ({_profile.LoadError}). Nové uložení soubor přepíše."
            : null;

        await _settings.LoadAsync();
        SelectedTheme = _settings.Current.Theme;
        _theme.Apply(SelectedTheme, persist: false);

        BudgetValue = (double)_settings.Current.MonthlyBudget;
        UpdateStrength(string.Empty);
    }

    public void ApplyTheme(object? parameter)
    {
        var mode = parameter?.ToString() switch
        {
            "Light" => ThemeMode.Light,
            "Dark" => ThemeMode.Dark,
            _ => ThemeMode.System,
        };

        SelectedTheme = mode;
        _theme.Apply(mode);
        _settings.Current.Theme = mode;
        _ = _settings.SaveAsync(_settings.Current);
    }

    public bool ValidateForm()
    {
        var valid = true;

        if (string.IsNullOrWhiteSpace(Name))
        {
            valid = false;
        }

        var strength = PasswordStrength.Evaluate(Password);
        if (!strength.IsValid)
        {
            valid = false;
        }

        return valid;
    }

    private void UpdateStrength(string? password)
    {
        var strength = PasswordStrength.Evaluate(password);

        HasMinLength = strength.HasMinLength;
        HasDigits = strength.HasDigits;
        HasLowercase = strength.HasLowercase;
        HasUppercase = strength.HasUppercase;
        HasSpecial = strength.HasSpecial;

        StrengthProgress = strength.Progress;
        StrengthLabel = strength.LevelLabel;

        OnPropertyChanged(nameof(HasMinLength));
        OnPropertyChanged(nameof(HasDigits));
        OnPropertyChanged(nameof(HasLowercase));
        OnPropertyChanged(nameof(HasUppercase));
        OnPropertyChanged(nameof(HasSpecial));
        OnPropertyChanged(nameof(RuleLengthColor));
        OnPropertyChanged(nameof(RuleDigitColor));
        OnPropertyChanged(nameof(RuleLowerColor));
        OnPropertyChanged(nameof(RuleUpperColor));
        OnPropertyChanged(nameof(RuleSpecialColor));
    }

    private async Task SaveProfileAsync()
    {
        if (!ValidateForm())
        {
            StatusMessage = "Vyplň jméno a heslo splňující všech 5 pravidel.";
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _profile.SaveAsync(Name, Password);

            if (!result.Success)
            {
                StatusMessage = result.ErrorMessage ?? "Uložení selhalo.";
                return;
            }

            HasStoredPassword = true;
            Password = string.Empty;
            StatusMessage = $"Uloženo do {ProfileService.FileName}.";
            await ShowAlert("Hotovo", $"Profil byl uložen do {ProfileService.FileName} (heslo jen jako PBKDF2 hash).");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SaveBudgetAsync()
    {
        IsBusy = true;
        try
        {
            _settings.Current.MonthlyBudget = RoundToHundreds((decimal)BudgetValue);
            _settings.Current.Theme = SelectedTheme;

            var result = await _settings.SaveAsync(_settings.Current);
            StatusMessage = result.Success
                ? $"Rozpočet uložen do {SettingsService.FileName}."
                : result.ErrorMessage ?? "Uložení selhalo.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>Budget is displayed and stored in whole hundreds of crowns.</summary>
    private static decimal RoundToHundreds(decimal value)
        => Math.Round(value / 100m, MidpointRounding.AwayFromZero) * 100m;

    private static async Task ShowAlert(string title, string message)
    {
        if (Shell.Current?.CurrentPage is { } page)
        {
            await page.DisplayAlert(title, message, "OK");
        }
    }
}
