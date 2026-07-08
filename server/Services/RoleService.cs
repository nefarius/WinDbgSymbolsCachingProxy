using Microsoft.Extensions.Caching.Memory;

using MongoDB.Entities;

using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Services;

/// <summary>
///     CRUD operations for <see cref="RoleEntity"/> records plus permission-resolution helpers.
///     Resolved permission sets are cached in memory to reduce MongoDB round-trips.
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
    ///     Returns the union of all permissions granted by the given role names,
    ///     with short-lived cache to amortise DB reads across requests.
    /// </summary>
    public async Task<List<string>> GetPermissionsForRolesAsync(
        IEnumerable<string> roleNames,
        CancellationToken ct = default)
    {
        string[] names = roleNames as string[] ?? roleNames.ToArray();
        if (names.Length == 0)
            return [];

        string cacheKey = $"role-perms:{string.Join(",", names.OrderBy(n => n))}";

        if (cache.TryGetValue(cacheKey, out List<string>? cached))
            return cached!;

        List<RoleEntity> roles = await db.Find<RoleEntity>()
            .Match(r => names.Contains(r.Name))
            .ExecuteAsync(ct);

        List<string> perms = roles.SelectMany(r => r.Permissions).Distinct().ToList();
        cache.Set(cacheKey, perms, CacheDuration);
        return perms;
    }

    public async Task SaveAsync(RoleEntity role, CancellationToken ct = default)
    {
        await db.SaveAsync(role, ct);
        InvalidateCacheFor(role.Name);
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
        => await db.DeleteAsync<RoleEntity>(id, ct);

    public void InvalidateCacheFor(string roleName)
    {
        // The combined key cannot be precisely evicted without a full cache scan;
        // entries expire naturally within CacheDuration. Force immediate eviction
        // for the single-role key used by API-key permission lookups.
        cache.Remove($"role-perms:{roleName}");
    }
}
