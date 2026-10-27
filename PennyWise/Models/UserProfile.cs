using System.Text.Json.Serialization;

namespace PennyWise.Models;

/// <summary>
/// Stored in profile.json. The password itself is never persisted, only a
/// PBKDF2 hash together with its salt and iteration count.
/// </summary>
public class UserProfile
{
    public string Name { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string Salt { get; set; } = string.Empty;

    public int Iterations { get; set; }

    public string HashAlgorithm { get; set; } = "PBKDF2-SHA256";

    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    [JsonIgnore]
    public bool HasPassword => !string.IsNullOrEmpty(PasswordHash);
}
