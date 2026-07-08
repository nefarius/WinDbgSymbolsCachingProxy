using MongoDB.Entities;

namespace WinDbgSymbolsCachingProxy.Models;

/// <summary>
///     A long-lived API key used by non-interactive clients (harvesting agents, WinDbg URL parameters, CI pipelines).
///     The plaintext key is shown exactly once at creation time; only the SHA-256 hash is persisted.
/// </summary>
[Collection("api-keys")]
public class ApiKeyEntity : Entity
{
    /// <summary>Human-readable label for this key (e.g. <c>HarvestingAgent-prod</c>).</summary>
    public string Name { get; set; } = null!;

    /// <summary>SHA-256 hex digest of the plaintext key used for constant-time comparison.</summary>
    public string KeyHash { get; set; } = null!;

    /// <summary>Names of <see cref="RoleEntity"/> records whose permissions this key inherits.</summary>
    public List<string> RoleNames { get; set; } = [];

    /// <summary>Optional free-text owner or service name, for documentation purposes.</summary>
    public string? Owner { get; set; }

    /// <summary>Optional UTC expiry; null means the key never expires.</summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>Whether the key is active. Revoked keys are disabled but not deleted.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>UTC timestamp of the most recent successful use of this key.</summary>
    public DateTime? LastUsedAt { get; set; }

    /// <summary>UTC timestamp when this key was created.</summary>
    public DateTime CreatedAt { get; set; }
}
