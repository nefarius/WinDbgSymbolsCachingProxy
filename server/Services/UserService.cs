using MongoDB.Entities;

using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Services;

/// <summary>
///     CRUD operations for <see cref="UserEntity"/> records.
///     Users are JIT-provisioned on first OIDC login via <see cref="GetOrCreateAsync"/>.
/// </summary>
public sealed class UserService(DB db, RoleService roleService)
{
    public async Task<UserEntity?> FindBySubjectAsync(string subject, CancellationToken ct = default)
        => await db.Find<UserEntity>().Match(u => u.Subject == subject).ExecuteFirstAsync(ct);

    /// <summary>
    ///     Retrieves an existing user or creates a new record on first login.
    ///     Updates e-mail and display name on every call to reflect identity-provider changes.
    /// </summary>
    public async Task<UserEntity> GetOrCreateAsync(
        string subject,
        string? email,
        string? displayName,
        CancellationToken ct = default)
    {
        UserEntity? user = await FindBySubjectAsync(subject, ct);

        if (user is not null)
        {
            user.LastLoginAt = DateTime.UtcNow;
            if (email is not null) user.Email = email;
            if (displayName is not null) user.DisplayName = displayName;
            await db.SaveAsync(user, ct);
            return user;
        }

        user = new UserEntity
        {
            Subject = subject,
            Email = email,
            DisplayName = displayName,
            RoleNames = [],
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow
        };
        await db.SaveAsync(user, ct);
        return user;
    }

    public async Task<List<string>> GetEffectivePermissionsAsync(string subject, CancellationToken ct = default)
    {
        UserEntity? user = await FindBySubjectAsync(subject, ct);
        if (user is null) return [];
        return await roleService.GetPermissionsForRolesAsync(user.RoleNames, ct);
    }

    public async Task<List<UserEntity>> GetAllAsync(CancellationToken ct = default)
        => await db.Find<UserEntity>().ExecuteAsync(ct);

    public async Task SaveAsync(UserEntity user, CancellationToken ct = default)
        => await db.SaveAsync(user, ct);

    public async Task DeleteAsync(string id, CancellationToken ct = default)
        => await db.DeleteAsync<UserEntity>(id, ct);
}
