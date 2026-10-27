using System.Collections.ObjectModel;
using PennyWise.Core;
using PennyWise.Models;
using PennyWise.Services;

namespace PennyWise.ViewModels;

public class DashboardViewModel : BaseViewModel
{
    private readonly TransactionService _transactions;
    private readonly SettingsService _settings;
    private Category? _activeCategory;
    private bool _onlyLargeExpenses;
    private decimal _balance;
    private decimal _income;
    private decimal _expenses;
    private double _budgetProgress;
    private string _budgetLabel = string.Empty;

    public DashboardViewModel(TransactionService transactions, SettingsService settings)
    {
        _transactions = transactions;
        _settings = settings;
        Title = "Přehled";

        AddCommand = new AsyncRelayCommand(AddAsync);
        FilterCommand = new RelayCommand(parameter => ToggleCategory(parameter as string));
        ClearFiltersCommand = new RelayCommand(ClearFilters);
        RefreshCommand = new AsyncRelayCommand(LoadAsync);

        _transactions.Changed += (_, _) => _ = LoadAsync();
    }

    public ObservableCollection<TransactionItem> VisibleTransactions { get; } = new();

    public IReadOnlyList<string> Categories { get; } = new[]
    {
        "Jídlo", "Bydlení", "Doprava", "Zábava", "Zdraví", "Ostatní",
    };

    public AsyncRelayCommand AddCommand { get; }

    public RelayCommand FilterCommand { get; }

    public RelayCommand ClearFiltersCommand { get; }

    public AsyncRelayCommand RefreshCommand { get; }

    public decimal Balance
    {
        get => _balance;
        private set => SetProperty(ref _balance, value);
    }

    public decimal Income
    {
        get => _income;
        private set => SetProperty(ref _income, value);
    }

    public decimal Expenses
    {
        get => _expenses;
        private set => SetProperty(ref _expenses, value);
    }

    public double BudgetProgress
    {
        get => _budgetProgress;
        private set => SetProperty(ref _budgetProgress, value);
    }

    public string BudgetLabel
    {
        get => _budgetLabel;
        private set => SetProperty(ref _budgetLabel, value);
    }

    public bool OnlyLargeExpenses
    {
        get => _onlyLargeExpenses;
        set
        {
            if (SetProperty(ref _onlyLargeExpenses, value))
            {
                ApplyFilter();
            }
        }
    }

    public string FilterSummary => _activeCategory is null && !_onlyLargeExpenses
        ? "Zobrazeny všechny záznamy"
        : $"Kategorie: {(_activeCategory?.ToCzechLabel() ?? "vše")}{(_onlyLargeExpenses ? ", jen velké výdaje" : string.Empty)}";

    public async Task LoadAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        try
        {
            await _transactions.EnsureLoadedAsync();
            await _settings.LoadAsync();

            Balance = _transactions.Balance;
            Income = _transactions.TotalIncome;
            Expenses = _transactions.TotalExpenses;

            var budget = _settings.Current.MonthlyBudget;
            var ratio = budget <= 0 ? 0 : (double)(Expenses / budget);
            BudgetProgress = Math.Clamp(ratio, 0, 1);
            BudgetLabel = budget <= 0
                ? "Rozpočet není nastaven"
                : $"{Services.CurrencyFormatter.Format(Expenses)} z {Services.CurrencyFormatter.Format(budget)} ({ratio:P0})";

            ApplyFilter();
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void ToggleCategory(string? categoryLabel)
    {
        if (categoryLabel is null)
        {
            return;
        }

        var category = Enum.GetValues<Category>()
            .FirstOrDefault(c => c.ToCzechLabel() == categoryLabel);

        _activeCategory = _activeCategory == category ? null : category;
        ApplyFilter();
    }

    private void ClearFilters()
    {
        _activeCategory = null;
        OnlyLargeExpenses = false;
        ApplyFilter();
    }

    /// <summary>True when the chip for this category label is the active filter.</summary>
    public bool IsCategorySelected(string categoryLabel)
    {
        var category = Enum.GetValues<Category>()
            .FirstOrDefault(c => c.ToCzechLabel() == categoryLabel);

        return _activeCategory == category;
    }

    public void ApplyFilter()
    {
        IEnumerable<Transaction> query = _transactions.All;

        if (_activeCategory is { } category)
        {
            query = query.Where(t => t.Category == category);
        }

        if (_onlyLargeExpenses)
        {
            // "Velké výdaje" = expensive expense, or anything flagged important.
            query = query.Where(t => t.Kind == TransactionKind.Expense && (t.Amount >= 1000m || t.IsImportant));
        }

        VisibleTransactions.Clear();
        foreach (var item in query.OrderByDescending(t => t.CreatedAt))
        {
            VisibleTransactions.Add(new TransactionItem(item));
        }

        OnPropertyChanged(nameof(FilterSummary));
        OnPropertyChanged(nameof(HasTransactions));
    }

    public bool HasTransactions => VisibleTransactions.Count > 0;

    private async Task AddAsync()
    {
        if (Shell.Current is { } shell)
        {
            await shell.GoToAsync(nameof(Views.AddTransactionPage));
        }
    }
}
