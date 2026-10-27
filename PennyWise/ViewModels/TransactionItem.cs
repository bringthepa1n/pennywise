using PennyWise.Models;
using PennyWise.Services;

namespace PennyWise.ViewModels;

/// <summary>
/// Read-only projection of a <see cref="Transaction"/> for the dashboard list.
/// Keeping formatting here keeps the XAML free of converters for money.
/// </summary>
public class TransactionItem
{
    public TransactionItem(Transaction source)
    {
        Source = source;
        Initials = string.IsNullOrWhiteSpace(source.Title)
            ? "?"
            : source.Title.Trim()[..1].ToUpperInvariant();
    }

    public Transaction Source { get; }

    public Guid Id => Source.Id;

    public string Title => Source.Title;

    public string Initials { get; }

    public string CategoryLabel => Source.CategoryLabel;

    public string DateLabel => Source.DateLabel;

    public string PriorityLabel => Source.PriorityLabel;

    public string SignedAmount => CurrencyFormatter.FormatSigned(Source.Amount, Source.Kind == TransactionKind.Income);

    public bool IsIncome => Source.Kind == TransactionKind.Income;

    public string AmountStyle => IsIncome ? "AmountPositive" : "AmountNegative";

    public string FlagsLabel
    {
        get
        {
            var flags = new List<string>();
            if (Source.IsRecurring)
            {
                flags.Add("pravidelně");
            }

            if (Source.IsImportant)
            {
                flags.Add("důležité");
            }

            return string.Join(" · ", flags);
        }
    }
}
