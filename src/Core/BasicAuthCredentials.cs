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
    public string Username { get; init; } = null!;

    public string Password { get; init; } = null!;

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

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is BasicAuthCredentials other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Username, Password);
    }

    public static bool operator ==(BasicAuthCredentials? left, BasicAuthCredentials? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(BasicAuthCredentials? left, BasicAuthCredentials? right)
    {
        return !Equals(left, right);
    }
}