using System.Collections.ObjectModel;
using PennyWise.Core;
using PennyWise.Services;

namespace PennyWise.ViewModels;

public sealed class JsonFileView
{
    public JsonFileView(string fileName, LoadState state, string? content, string? error)
    {
        FileName = fileName;
        State = state;
        Content = content ?? string.Empty;
        Error = error;
    }

    public string FileName { get; }

    public LoadState State { get; }

    public string Content { get; }

    public string? Error { get; }

    public bool HasContent => !string.IsNullOrEmpty(Content);

    public bool HasError => !string.IsNullOrEmpty(Error);

    public string StatusText => State switch
    {
        LoadState.Missing => "Soubor zatím neexistuje",
        LoadState.Corrupted => "Soubor je poškozený",
        _ => "Platný JSON",
    };
}

/// <summary>Screen 5: shows the JSON files the app works with.</summary>
public class JsonViewModel : BaseViewModel
{
    private readonly JsonStorageService _storage;
    private readonly string _folder = string.Empty;

    public JsonViewModel(JsonStorageService storage)
    {
        _storage = storage;
        Title = "JSON data";
        _folder = storage.Folder;
        ReloadCommand = new AsyncRelayCommand(ReloadAsync);
    }

    public ObservableCollection<JsonFileView> Files { get; } = new();

    public AsyncRelayCommand ReloadCommand { get; }

    public string Folder => _folder;

    public string FilesSummary => Files.Count == 0
        ? "Soubory nejsou načtené."
        : $"{Files.Count(f => f.HasContent)} z {Files.Count} souborů se podařilo zobrazit.";

    public async Task ReloadAsync()
    {
        Files.Clear();

        foreach (var name in new[]
                 {
                     ProfileService.FileName,
                     TransactionService.FileName,
                     SettingsService.FileName,
                 })
        {
            var result = await _storage.LoadRawAsync(name);
            Files.Add(new JsonFileView(name, result.State, result.Value, result.ErrorMessage));
        }

        OnPropertyChanged(nameof(FilesSummary));
    }
}
