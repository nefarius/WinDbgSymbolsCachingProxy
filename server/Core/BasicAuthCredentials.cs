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

        return Username == other.Username && Password == other.Password;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is BasicAuthCredentials other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(Username, Password);
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