using System.Globalization;

namespace PennyWise.Services;

/// <summary>
/// One place for money formatting. Czech convention: thousands separated by a
/// non-breaking space, "Kč" after the number.
/// </summary>
public static class CurrencyFormatter
{
    private static readonly NumberFormatInfo Czech = new()
    {
        NumberGroupSeparator = "\u00A0",
        NumberDecimalSeparator = ",",
        NumberGroupSizes = new[] { 3 },
    };

    public static string Format(decimal amount) => amount.ToString("N0", Czech) + " Kč";

    public static string FormatSigned(decimal amount, bool income)
        => (income ? "+ " : "- ") + Format(Math.Abs(amount));

    public static bool TryParseAmount(string? text, out decimal amount)
    {
        amount = 0;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var normalized = text.Replace("\u00A0", string.Empty)
            .Replace(" ", string.Empty)
            .Replace("Kč", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace(',', '.');

        return decimal.TryParse(normalized, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out amount)
               && amount > 0 && amount <= 10_000_000m;
    }
}
