using System.Text.Json.Serialization;

namespace PennyWise.Models;

/// <summary>Monthly budget and theme choice; stored in appsettings.json.</summary>
public class AppSettings
{
    public decimal MonthlyBudget { get; set; } = 25000m;

    /// <summary>Light, Dark or System.</summary>
    public ThemeMode Theme { get; set; } = ThemeMode.System;

    [JsonIgnore]
    public static AppSettings Default => new();
}

public enum ThemeMode
{
    System,
    Light,
    Dark,
}
