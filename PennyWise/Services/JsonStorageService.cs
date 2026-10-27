using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using PennyWise.Core;

namespace PennyWise.Services;

public enum LoadState
{
    Ok,
    Missing,
    Corrupted,
}

public readonly record struct LoadResult<T>(LoadState State, T? Value, string? ErrorMessage)
{
    public bool IsOk => State == LoadState.Ok;
}

/// <summary>
/// Tiny JSON persistence helper. Files live in FileSystem.AppDataDirectory so
/// the app never needs platform specific paths. Every error is reported as a
/// <see cref="LoadResult{T}"/>; a corrupted file is never silently deleted.
/// </summary>
public class JsonStorageService
{
    public static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly JsonSerializerOptions _options;

    public JsonStorageService() : this(Options)
    {
    }

    public JsonStorageService(JsonSerializerOptions options) => _options = options;

    public string Folder => FileSystem.AppDataDirectory;

    public string PathFor(string fileName) => Path.Combine(Folder, fileName);

    public bool Exists(string fileName) => File.Exists(PathFor(fileName));

    public async Task<Result> SaveAsync<T>(string fileName, T value, CancellationToken cancellationToken = default)
    {
        var path = PathFor(fileName);

        try
        {
            Directory.CreateDirectory(Folder);
            var json = JsonSerializer.Serialize(value, _options);

            // Write to a temp file first so an interrupted write never leaves
            // a half written (corrupted) JSON behind.
            var tempPath = path + ".tmp";
            await File.WriteAllTextAsync(tempPath, json, cancellationToken).ConfigureAwait(false);
            File.Move(tempPath, path, overwrite: true);
            return Result.Ok();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or NotSupportedException)
        {
            return Result.Fail($"Soubor {fileName} se nepodařilo uložit: {ex.Message}");
        }
    }

    public async Task<LoadResult<T>> LoadAsync<T>(string fileName, CancellationToken cancellationToken = default)
        where T : class
    {
        var path = PathFor(fileName);

        if (!File.Exists(path))
        {
            return new LoadResult<T>(LoadState.Missing, null, null);
        }

        try
        {
            var json = await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(json))
            {
                return new LoadResult<T>(LoadState.Corrupted, null, $"{fileName} je prázdný.");
            }

            var value = JsonSerializer.Deserialize<T>(json, _options);

            return value is null
                ? new LoadResult<T>(LoadState.Corrupted, null, $"{fileName} neobsahuje očekávaná data.")
                : new LoadResult<T>(LoadState.Ok, value, null);
        }
        catch (JsonException ex)
        {
            return new LoadResult<T>(LoadState.Corrupted, null, $"{fileName} není platný JSON ({ex.Message}).");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return new LoadResult<T>(LoadState.Corrupted, null, $"{fileName} nelze přečíst: {ex.Message}");
        }
    }

    /// <summary>Raw file content for the JSON screen (formatted when possible).</summary>
    public async Task<LoadResult<string>> LoadRawAsync(string fileName, CancellationToken cancellationToken = default)
    {
        var path = PathFor(fileName);

        if (!File.Exists(path))
        {
            return new LoadResult<string>(LoadState.Missing, null, null);
        }

        try
        {
            var json = await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(json))
            {
                return new LoadResult<string>(LoadState.Corrupted, json, $"{fileName} je prázdný.");
            }

            using var document = JsonDocument.Parse(json);
            var pretty = JsonSerializer.Serialize(document.RootElement, Options);
            return new LoadResult<string>(LoadState.Ok, pretty, null);
        }
        catch (JsonException ex)
        {
            var broken = await SafeReadAsync(path, cancellationToken).ConfigureAwait(false);
            return new LoadResult<string>(LoadState.Corrupted, broken, $"{fileName} není platný JSON: {ex.Message}");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return new LoadResult<string>(LoadState.Corrupted, null, $"{fileName} nelze přečíst: {ex.Message}");
        }
    }

    public Task<Result> DeleteAsync(string fileName)
    {
        try
        {
            var path = PathFor(fileName);
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            return Task.FromResult(Result.Ok());
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return Task.FromResult(Result.Fail($"Soubor {fileName} nelze smazat: {ex.Message}"));
        }
    }

    private static async Task<string?> SafeReadAsync(string path, CancellationToken cancellationToken)
    {
        try
        {
            return await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return null;
        }
    }
}
