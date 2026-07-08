using Microsoft.Extensions.Caching.Memory;

using MongoDB.Entities;

using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Services;

/// <summary>
///     CRUD operations for <see cref="RoleEntity"/> records plus permission-resolution helpers.
///     Each role's permission set is cached individually so any single-role change is immediately
///     evictable and the union for multi-role principals is always computed from fresh per-role data.
/// </summary>
public sealed class RoleService(DB db, IMemoryCache cache)
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public async Task<List<RoleEntity>> GetAllAsync(CancellationToken ct = default)
        => await db.Find<RoleEntity>().ExecuteAsync(ct);

    public async Task<RoleEntity?> GetByNameAsync(string name, CancellationToken ct = default)
        => await db.Find<RoleEntity>().Match(r => r.Name == name).ExecuteFirstAsync(ct);

    public async Task<RoleEntity?> GetByIdAsync(string id, CancellationToken ct = default)
        => await db.Find<RoleEntity>().MatchID(id).ExecuteFirstAsync(ct);

    /// <summary>
    ///     Returns the union of all permissions granted by the given role names.
    ///     Each role's permission list is cached individually; the union is computed on every call
    ///     so that changing or deleting any single role takes effect immediately without needing to
    ///     enumerate every possible combination.
    /// </summary>
    public async Task<List<string>> GetPermissionsForRolesAsync(
        IEnumerable<string> roleNames,
        CancellationToken ct = default)
    {
        string[] names = roleNames as string[] ?? roleNames.ToArray();
        if (names.Length == 0)
            return [];

        List<string> result = [];
        foreach (string name in names)
            result.AddRange(await GetPermissionsForRoleAsync(name, ct));

        return result.Distinct().ToList();
    }

    public async Task SaveAsync(RoleEntity role, CancellationToken ct = default)
    {
        await db.SaveAsync(role, ct);
        InvalidateCacheFor(role.Name);
    }

    /// <summary>
    ///     Deletes a role by id. Throws <see cref="InvalidOperationException"/> for system roles
    ///     so that crafted requests cannot bypass the UI-side disabled check.
    /// </summary>
    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        RoleEntity? role = await GetByIdAsync(id, ct);
        if (role is null)
            return;

        if (role.IsSystemRole)
            throw new InvalidOperationException($"System role '{role.Name}' cannot be deleted.");

        await db.DeleteAsync<RoleEntity>(id, ct);
        InvalidateCacheFor(role.Name);
    }

    /// <summary>Evicts the cached permission list for a single named role.</summary>
    public void InvalidateCacheFor(string roleName)
        => cache.Remove(CacheKey(roleName));

    // ── private helpers ───────────────────────────────────────────────────────

    private static string CacheKey(string roleName) => $"role-perms:{roleName}";

    private async Task<List<string>> GetPermissionsForRoleAsync(string roleName, CancellationToken ct)
    {
        string key = CacheKey(roleName);
        if (cache.TryGetValue(key, out List<string>? cached))
            return cached!;

        RoleEntity? role = await GetByNameAsync(roleName, ct);
        List<string> perms = role?.Permissions ?? [];
        cache.Set(key, perms, CacheDuration);
        return perms;
    }
}
