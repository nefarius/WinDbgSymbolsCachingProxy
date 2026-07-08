using MongoDB.Entities;

namespace WinDbgSymbolsCachingProxy.Models;

/// <summary>
///     Represents an OIDC-authenticated user, JIT-provisioned on first login.
///     Roles determine what the user is permitted to do.
/// </summary>
[Collection("users")]
public class UserEntity : Entity
{
    /// <summary>OIDC subject claim (<c>sub</c>) — unique, stable identifier from the identity provider.</summary>
    public string Subject { get; set; } = null!;

    /// <summary>User's e-mail address as reported by the identity provider.</summary>
    public string? Email { get; set; }

    /// <summary>User's display name as reported by the identity provider.</summary>
    public string? DisplayName { get; set; }

    /// <summary>Names of <see cref="RoleEntity"/> records assigned to this user.</summary>
    public List<string> RoleNames { get; set; } = [];

    /// <summary>UTC timestamp when this user record was first created.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>UTC timestamp of the user's most recent successful login.</summary>
    public DateTime? LastLoginAt { get; set; }
}
