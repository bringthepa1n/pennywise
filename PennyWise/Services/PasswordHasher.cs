using System.Security.Cryptography;

namespace PennyWise.Services;

/// <summary>
/// PBKDF2-HMAC-SHA256 with a random 16-byte salt. The assignment does not
/// prescribe an algorithm; PBKDF2 is used because it ships with .NET (no extra
/// NuGet package on the school machine) and is a standard password KDF.
/// </summary>
public static class PasswordHasher
{
    public const int SaltSize = 16;
    public const int KeySize = 32;
    public const int DefaultIterations = 100_000;

    public static (string Hash, string Salt, int Iterations) Create(string password, int iterations = DefaultIterations)
    {
        ArgumentNullException.ThrowIfNull(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Derive(password, salt, iterations, KeySize);

        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt), iterations);
    }

    public static bool Verify(string password, string hashBase64, string saltBase64, int iterations)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashBase64) || string.IsNullOrEmpty(saltBase64)
            || iterations <= 0)
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(saltBase64);
            var expected = Convert.FromBase64String(hashBase64);
            var actual = Derive(password, salt, iterations, expected.Length);
            return CryptographicOperations.FixedTimeEquals(expected, actual);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static byte[] Derive(string password, byte[] salt, int iterations, int length)
        => Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, length);
}
