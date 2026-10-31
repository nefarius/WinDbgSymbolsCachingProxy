using System.Diagnostics.CodeAnalysis;

namespace WinDbgSymbolsCachingProxy.Core;

/// <summary>
///     Represents basic credentials required for Basic Authentication.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public sealed class BasicAuthCredentials : IEquatable<BasicAuthCredentials>
{
    /// <summary>Gets the username for Basic Authentication.</summary>
    public string Username { get; init; } = null!;

    /// <summary>Gets the password for Basic Authentication.</summary>
    public string Password { get; init; } = null!;

    /// <inheritdoc />
    public bool Equals(BasicAuthCredentials? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        // A missing username/password never matches anything (and is distinct from an explicitly empty one).
        if (Username is null || Password is null || other.Username is null || other.Password is null)
        {
            return false;
        }

        // Evaluate both comparisons (no short-circuit) in constant time to avoid leaking prefix matches.
        bool userMatches = FixedTimeEquals(Username, other.Username);
        bool passwordMatches = FixedTimeEquals(Password, other.Password);
        return userMatches & passwordMatches;
    }

    private static bool FixedTimeEquals(string a, string b)
    {
        byte[] left = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(a));
        byte[] right = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(b));
        return System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(left, right);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is BasicAuthCredentials other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        // Never derive hash codes from the password.
        return Username?.GetHashCode() ?? 0;
    }

    /// <summary>Equality operator.</summary>
    public static bool operator ==(BasicAuthCredentials? left, BasicAuthCredentials? right)
    {
        return Equals(left, right);
    }

    /// <summary>Inequality operator.</summary>
    public static bool operator !=(BasicAuthCredentials? left, BasicAuthCredentials? right)
    {
        return !Equals(left, right);
    }
}