namespace PennyWise.Services;

/// <summary>
/// Simplified strength evaluation required by the assignment: length plus
/// presence of digits, lowercase, uppercase and special characters.
/// </summary>
public sealed class PasswordStrength
{
    public const int MinLength = 8;

    public bool HasMinLength { get; private init; }

    public bool HasDigits { get; private init; }

    public bool HasLowercase { get; private init; }

    public bool HasUppercase { get; private init; }

    public bool HasSpecial { get; private init; }

    public int Fulfilled =>
        (HasMinLength ? 1 : 0) + (HasDigits ? 1 : 0) + (HasLowercase ? 1 : 0)
        + (HasUppercase ? 1 : 0) + (HasSpecial ? 1 : 0);

    /// <summary>0.0 - 1.0 for the ProgressBar.</summary>
    public double Progress => Fulfilled / 5.0;

    public string LevelLabel => Fulfilled switch
    {
        0 => "Zadejte heslo",
        1 or 2 => "Slabé",
        3 or 4 => "Průměrné",
        _ => "Silné",
    };

    public bool IsValid => Fulfilled == 5;

    public static PasswordStrength Evaluate(string? password)
    {
        var value = password ?? string.Empty;

        return new PasswordStrength
        {
            HasMinLength = value.Length >= MinLength,
            HasDigits = value.Any(char.IsDigit),
            HasLowercase = value.Any(char.IsLower),
            HasUppercase = value.Any(char.IsUpper),
            HasSpecial = value.Any(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c)),
        };
    }
}
