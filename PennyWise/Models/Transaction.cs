namespace PennyWise.Models;

/// <summary>One movement on the account; persisted in transactions.json.</summary>
public class Transaction
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public TransactionKind Kind { get; set; } = TransactionKind.Expense;

    public Category Category { get; set; } = Category.Other;

    /// <summary>Priority 1-5 picked on the slider in the add form.</summary>
    public int Priority { get; set; } = 3;

    public bool IsRecurring { get; set; }

    public bool IsImportant { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [System.Text.Json.Serialization.JsonIgnore]
    public string KindLabel => Kind == TransactionKind.Income ? "Příjem" : "Výdaj";

    [System.Text.Json.Serialization.JsonIgnore]
    public string CategoryLabel => Category.ToCzechLabel();

    [System.Text.Json.Serialization.JsonIgnore]
    public string SignedAmount => Kind == TransactionKind.Income
        ? $"+ {Amount:N0} Kč"
        : $"- {Amount:N0} Kč";

    [System.Text.Json.Serialization.JsonIgnore]
    public string DateLabel => CreatedAt.ToString("d. M. yyyy");

    [System.Text.Json.Serialization.JsonIgnore]
    public string PriorityLabel => $"Priorita {Priority}";
}
