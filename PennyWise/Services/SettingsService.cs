using PennyWise.Core;
using PennyWise.Models;

namespace PennyWise.Services;

/// <summary>Monthly budget + theme choice; stored in appsettings.json.</summary>
public class SettingsService
{
    public const string FileName = "appsettings.json";

    private readonly JsonStorageService _storage;

    public SettingsService(JsonStorageService storage) => _storage = storage;

    public AppSettings Current { get; private set; } = AppSettings.Default;

    public async Task<AppSettings> LoadAsync()
    {
        var result = await _storage.LoadAsync<AppSettings>(FileName).ConfigureAwait(false);
        Current = result.IsOk && result.Value is not null ? result.Value : AppSettings.Default;
        return Current;
    }

    public async Task<Result> SaveAsync(AppSettings settings)
    {
        var result = await _storage.SaveAsync(FileName, settings).ConfigureAwait(false);
        if (result.Success)
        {
            Current = settings;
        }

        return result;
    }
}
