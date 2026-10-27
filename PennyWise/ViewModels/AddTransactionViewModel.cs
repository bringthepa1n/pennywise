using PennyWise.Core;
using PennyWise.Models;
using PennyWise.Services;

namespace PennyWise.ViewModels;

/// <summary>
/// Screen 2. The save button is an AsyncRelayCommand with a re-entrancy guard,
/// so a double tap cannot start two submissions.
/// </summary>
public class AddTransactionViewModel : BaseViewModel
{
    public const int SaveDelayMilliseconds = 2000;

    private readonly TransactionService _transactions;
    private readonly Random _random = new();
    private string _title = string.Empty;
    private string _amountText = string.Empty;
    private double _priority = 3;
    private bool _isRecurring;
    private bool _isImportant;
    private bool _simulateFailure;
    private bool _isIncome;
    private int _selectedCategoryIndex;
    private string? _titleError;
    private string? _amountError;
    private string _statusMessage = string.Empty;

    public AddTransactionViewModel(TransactionService transactions)
    {
        _transactions = transactions;
        Title = "Nový záznam";
        SaveCommand = new AsyncRelayCommand(SaveAsync);
        ToggleKindCommand = new RelayCommand(parameter => SetKind(parameter as string == "Income"));
    }

    public AsyncRelayCommand SaveCommand { get; }

    public RelayCommand ToggleKindCommand { get; }

    public IReadOnlyList<string> Categories { get; } = Enum.GetValues<Category>()
        .Where(c => c != Category.Income)
        .Select(c => c.ToCzechLabel())
        .ToList();

    public string FormTitle
    {
        get => _title;
        set
        {
            if (SetProperty(ref _title, value))
            {
                TitleError = null;
                OnPropertyChanged(nameof(TitleLiveFeedback));
            }
        }
    }

    public string AmountText
    {
        get => _amountText;
        set
        {
            if (SetProperty(ref _amountText, value))
            {
                AmountError = null;
                OnPropertyChanged(nameof(AmountPreview));
            }
        }
    }

    public double Priority
    {
        get => _priority;
        set
        {
            if (SetProperty(ref _priority, value))
            {
                OnPropertyChanged(nameof(PriorityWhole));
                OnPropertyChanged(nameof(PriorityLabel));
            }
        }
    }

    /// <summary>Whole number shown next to the slider; the slider itself is rounded in code-behind.</summary>
    public int PriorityWhole => (int)Math.Round(_priority);

    public string PriorityLabel => $"Priorita {PriorityWhole} z 5";

    public bool IsRecurring
    {
        get => _isRecurring;
        set
        {
            if (SetProperty(ref _isRecurring, value))
            {
                OnPropertyChanged(nameof(OptionSummary));
            }
        }
    }

    public bool IsImportant
    {
        get => _isImportant;
        set
        {
            if (SetProperty(ref _isImportant, value))
            {
                OnPropertyChanged(nameof(OptionSummary));
            }
        }
    }

    public bool IsIncome
    {
        get => _isIncome;
        set
        {
            if (SetProperty(ref _isIncome, value))
            {
                OnPropertyChanged(nameof(IsExpense));
                OnPropertyChanged(nameof(KindSummary));
                OnPropertyChanged(nameof(AmountPreview));
            }
        }
    }

    public bool IsExpense => !_isIncome;

    public string KindSummary => IsIncome ? "Záznam se přičte k příjmům." : "Záznam se odečte od výdajů.";

    public string TitleLiveFeedback => string.IsNullOrWhiteSpace(FormTitle)
        ? "Název se zobrazí v přehledu i v JSON souboru."
        : $"Zůstane {60 - FormTitle.Length} znaků.";

    public string AmountPreview => CurrencyFormatter.TryParseAmount(AmountText, out var amount)
        ? $"Uloží se {(IsIncome ? "+" : "-")} {CurrencyFormatter.Format(amount)}"
        : "Částka musí být kladné číslo.";

    public bool HasStatusMessage => !string.IsNullOrEmpty(StatusMessage);

    public bool SimulateFailure
    {
        get => _simulateFailure;
        set => SetProperty(ref _simulateFailure, value);
    }

    public int SelectedCategoryIndex
    {
        get => _selectedCategoryIndex;
        set => SetProperty(ref _selectedCategoryIndex, value);
    }

    public string? TitleError
    {
        get => _titleError;
        private set
        {
            if (SetProperty(ref _titleError, value))
            {
                OnPropertyChanged(nameof(HasTitleError));
            }
        }
    }

    public bool HasTitleError => !string.IsNullOrEmpty(_titleError);

    public string? AmountError
    {
        get => _amountError;
        private set
        {
            if (SetProperty(ref _amountError, value))
            {
                OnPropertyChanged(nameof(HasAmountError));
            }
        }
    }

    public bool HasAmountError => !string.IsNullOrEmpty(_amountError);

    public string StatusMessage
    {
        get => _statusMessage;
        private set
        {
            if (SetProperty(ref _statusMessage, value))
            {
                OnPropertyChanged(nameof(HasStatusMessage));
            }
        }
    }

    public bool Validate()
    {
        TitleError = string.IsNullOrWhiteSpace(FormTitle) ? "Zadej název záznamu." : null;

        AmountError = CurrencyFormatter.TryParseAmount(AmountText, out _)
            ? null
            : "Zadej kladnou částku, např. 249.50 nebo 249,50.";

        return !HasTitleError && !HasAmountError;
    }

    public void RefreshOptionSummary()
    {
        OnPropertyChanged(nameof(OptionSummary));
    }

    public string OptionSummary
    {
        get
        {
            var parts = new List<string>();
            if (IsRecurring)
            {
                parts.Add("pravidelná");
            }

            if (IsImportant)
            {
                parts.Add("důležité");
            }

            return parts.Count == 0 ? "Bez doplňkových voleb." : "Označeno jako " + string.Join(" a ", parts) + ".";
        }
    }

    private void SetKind(bool income)
    {
        IsIncome = income;
        SaveCommand.RaiseCanExecuteChanged();
    }

    private async Task SaveAsync()
    {
        if (!Validate())
        {
            StatusMessage = "Zkontroluj vyznačená pole.";
            return;
        }

        IsBusy = true;

        try
        {
            StatusMessage = "Ukládám záznam…";
            await Task.Delay(SaveDelayMilliseconds);

            // Deterministic switch for the teacher, plus a small random chance so
            // the failure path also shows up without switching anything.
            var randomFailure = _random.NextDouble() < 0.15;
            if (SimulateFailure || randomFailure)
            {
                StatusMessage = "Uložení selhalo. Data zůstala ve formuláři.";
                return;
            }

            CurrencyFormatter.TryParseAmount(AmountText, out var amount);
            var category = Enum.GetValues<Category>()
                .Where(c => c != Category.Income)
                .ElementAtOrDefault(SelectedCategoryIndex);

            var transaction = new Transaction
            {
                Title = FormTitle.Trim(),
                Amount = amount,
                Kind = IsIncome ? TransactionKind.Income : TransactionKind.Expense,
                Category = IsIncome ? Category.Income : category,
                Priority = PriorityWhole,
                IsRecurring = IsRecurring,
                IsImportant = IsImportant,
                CreatedAt = DateTime.Now,
            };

            var result = await _transactions.AddAsync(transaction);
            if (!result.Success)
            {
                StatusMessage = result.ErrorMessage ?? "Záznam se nepodařilo uložit.";
                return;
            }

            StatusMessage = string.Empty;
            var page = Shell.Current?.CurrentPage;
            if (page is not null)
            {
                await page.DisplayAlert("Úspěch", "Záznam byl uložen", "OK");
            }

            if (Shell.Current is { } shell)
            {
                await shell.GoToAsync("..");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
