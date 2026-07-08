using System.Security.Cryptography;
using System.Text;

using MongoDB.Entities;

using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Services;

/// <summary>
///     Creates, validates, and manages <see cref="ApiKeyEntity"/> records used by non-interactive clients.
/// </summary>
public sealed class ApiKeyService(DB db, RoleService roleService)
{
    /// <summary>
    ///     Generates a new cryptographically random, URL-safe API key (43 characters, ~256 bits of entropy).
    /// </summary>
    public static string GenerateKey()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');

    /// <summary>Returns the lowercase hex SHA-256 digest of the plaintext key.</summary>
    public static string HashKey(string key)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(key))).ToLowerInvariant();

    /// <summary>
    ///     Creates a new API key. The plaintext is returned exactly once; only the hash is persisted.
    /// </summary>
    public async Task<(ApiKeyEntity Entity, string Plaintext)> CreateAsync(
        string name,
        List<string> roleNames,
        string? owner,
        DateTime? expiresAt,
        CancellationToken ct = default)
    {
        string plaintext = GenerateKey();
        ApiKeyEntity entity = new()
        {
            Name = name,
            KeyHash = HashKey(plaintext),
            RoleNames = roleNames,
            Owner = owner,
            ExpiresAt = expiresAt,
            Enabled = true,
            CreatedAt = DateTime.UtcNow
        };
        await db.SaveAsync(entity, ct);
        return (entity, plaintext);
    }

    /// <summary>
    ///     Validates a plaintext API key.
    ///     Returns the matching entity and updates <see cref="ApiKeyEntity.LastUsedAt"/>, or null when invalid.
    /// </summary>
    public async Task<ApiKeyEntity?> ValidateAsync(string key, CancellationToken ct = default)
    {
        string hash = HashKey(key);

        ApiKeyEntity? entity = await db.Find<ApiKeyEntity>()
            .Match(k => k.KeyHash == hash && k.Enabled)
            .ExecuteFirstAsync(ct);

        if (entity is null)
            return null;

        if (entity.ExpiresAt.HasValue && entity.ExpiresAt < DateTime.UtcNow)
            return null;

        entity.LastUsedAt = DateTime.UtcNow;
        await db.SaveAsync(entity, ct);
        return entity;
    }

    public async Task<List<string>> GetEffectivePermissionsAsync(
        ApiKeyEntity apiKey,
        CancellationToken ct = default)
        => await roleService.GetPermissionsForRolesAsync(apiKey.RoleNames, ct);

    public async Task<List<ApiKeyEntity>> GetAllAsync(CancellationToken ct = default)
        => await db.Find<ApiKeyEntity>().ExecuteAsync(ct);

    public async Task RevokeAsync(string id, CancellationToken ct = default)
    {
        ApiKeyEntity? key = await db.Find<ApiKeyEntity>().MatchID(id).ExecuteFirstAsync(ct);
        if (key is null) return;
        key.Enabled = false;
        await db.SaveAsync(key, ct);
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
        => await db.DeleteAsync<ApiKeyEntity>(id, ct);
}
