using PennyWise.Core;
using PennyWise.Models;

namespace PennyWise.Services;

/// <summary>Loads and saves profile.json (name + password hash, never the password).</summary>
public class ProfileService
{
    public const string FileName = "profile.json";

    private readonly JsonStorageService _storage;

    public ProfileService(JsonStorageService storage) => _storage = storage;

    public UserProfile? Current { get; private set; }

    public string? LoadError { get; private set; }

    public bool FileWasCorrupted { get; private set; }

    public async Task<UserProfile> LoadAsync()
    {
        var result = await _storage.LoadAsync<UserProfile>(FileName).ConfigureAwait(false);

        FileWasCorrupted = result.State == LoadState.Corrupted;
        LoadError = result.ErrorMessage;
        Current = result.IsOk ? result.Value : null;

        return Current ?? new UserProfile();
    }

    public async Task<Result> SaveAsync(string name, string password)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Fail("Vyplň jméno.");
        }

        var profile = Current ?? new UserProfile();
        var (hash, salt, iterations) = PasswordHasher.Create(password);

        profile.Name = name.Trim();
        profile.PasswordHash = hash;
        profile.Salt = salt;
        profile.Iterations = iterations;
        profile.HashAlgorithm = "PBKDF2-SHA256";
        profile.UpdatedAt = DateTime.Now;

        var result = await _storage.SaveAsync(FileName, profile).ConfigureAwait(false);

        if (result.Success)
        {
            Current = profile;
            FileWasCorrupted = false;
            LoadError = null;
        }

        return result;
    }

    public bool VerifyPassword(string password)
        => Current is { HasPassword: true } profile
           && PasswordHasher.Verify(password, profile.PasswordHash, profile.Salt, profile.Iterations);
}
