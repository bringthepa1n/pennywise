using PennyWise.Models;

namespace PennyWise.Services;

/// <summary>
/// In-memory transaction list backed by transactions.json. The five seed
/// records make screen 1 useful before the user adds anything.
/// </summary>
public class TransactionService
{
    public const string FileName = "transactions.json";

    private readonly JsonStorageService _storage;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private List<Transaction> _transactions = new();
    private bool _loaded;

    public TransactionService(JsonStorageService storage) => _storage = storage;

    public event EventHandler? Changed;

    public IReadOnlyList<Transaction> All => _transactions;

    public async Task EnsureLoadedAsync()
    {
        if (_loaded)
        {
            return;
        }

        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_loaded)
            {
                return;
            }

            var result = await _storage.LoadAsync<TransactionCollection>(FileName).ConfigureAwait(false);
            var hasData = result.IsOk && result.Value is not null && result.Value.Items.Count > 0;

            // Missing or corrupted file: start from the seed records and persist
            // them right away, so the JSON screen always has a valid document.
            _transactions = hasData ? result.Value!.Items : Seed();

            if (!hasData)
            {
                await PersistAsync().ConfigureAwait(false);
            }

            _loaded = true;
        }
        finally
        {
            _gate.Release();
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }

    public async Task<Core.Result> AddAsync(Transaction transaction)
    {
        await EnsureLoadedAsync().ConfigureAwait(false);

        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            _transactions.Insert(0, transaction);
            var result = await PersistAsync().ConfigureAwait(false);
            if (!result.Success)
            {
                _transactions.Remove(transaction);
            }

            return result;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<Core.Result> ReplaceAllAsync(IEnumerable<Transaction> transactions)
    {
        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            _transactions = transactions.ToList();
            _loaded = true;
            var result = await PersistAsync().ConfigureAwait(false);
            if (result.Success)
            {
                Changed?.Invoke(this, EventArgs.Empty);
            }

            return result;
        }
        finally
        {
            _gate.Release();
        }
    }

    public decimal TotalIncome => _transactions.Where(t => t.Kind == TransactionKind.Income).Sum(t => t.Amount);

    public decimal TotalExpenses => _transactions.Where(t => t.Kind == TransactionKind.Expense).Sum(t => t.Amount);

    public decimal Balance => TotalIncome - TotalExpenses;

    private Task<Core.Result> PersistAsync()
        => _storage.SaveAsync(FileName, new TransactionCollection { Items = _transactions });

    private static List<Transaction> Seed()
    {
        var today = DateTime.Now;

        return new List<Transaction>
        {
            new()
            {
                Title = "Výplata", Amount = 32000m, Kind = TransactionKind.Income, Category = Category.Income,
                Priority = 5, IsRecurring = true, IsImportant = true, CreatedAt = today.AddDays(-1),
            },
            new()
            {
                Title = "Nájem a energie", Amount = 11500m, Kind = TransactionKind.Expense, Category = Category.Housing,
                Priority = 5, IsRecurring = true, IsImportant = true, CreatedAt = today.AddDays(-2),
            },
            new()
            {
                Title = "Nákup potravin", Amount = 1840m, Kind = TransactionKind.Expense, Category = Category.Food,
                Priority = 3, IsRecurring = false, IsImportant = false, CreatedAt = today.AddDays(-3),
            },
            new()
            {
                Title = "Měsíční kupon MHD", Amount = 550m, Kind = TransactionKind.Expense, Category = Category.Transport,
                Priority = 2, IsRecurring = true, IsImportant = false, CreatedAt = today.AddDays(-5),
            },
            new()
            {
                Title = "Kino a večeře", Amount = 920m, Kind = TransactionKind.Expense, Category = Category.Fun,
                Priority = 1, IsRecurring = false, IsImportant = false, CreatedAt = today.AddDays(-6),
            },
            new()
            {
                Title = "Lékárna", Amount = 340m, Kind = TransactionKind.Expense, Category = Category.Health,
                Priority = 4, IsRecurring = false, IsImportant = true, CreatedAt = today.AddDays(-8),
            },
        };
    }

    private sealed class TransactionCollection
    {
        public List<Transaction> Items { get; set; } = new();
    }
}
