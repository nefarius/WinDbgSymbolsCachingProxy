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
        => (await GetOrCreateWithStatusAsync(subject, email, displayName, ct)).User;

    /// <summary>
    ///     Same as <see cref="GetOrCreateAsync"/> but also reports whether the user record was just created.
    ///     Existing users are updated with partial updates so concurrent role edits are never overwritten.
    /// </summary>
    public async Task<(UserEntity User, bool Created)> GetOrCreateWithStatusAsync(
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

            Update<UserEntity> update = db.Update<UserEntity>()
                .MatchID(user.ID)
                .Modify(b => b.Set(u => u.LastLoginAt, user.LastLoginAt));
            if (email is not null) update = update.Modify(b => b.Set(u => u.Email, email));
            if (displayName is not null) update = update.Modify(b => b.Set(u => u.DisplayName, displayName));
            await update.ExecuteAsync(ct);
            return (user, false);
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
        return (user, true);
    }

    /// <summary>Replaces only the role list of a user (partial update).</summary>
    public async Task SetRolesAsync(UserEntity user, List<string> roleNames, CancellationToken ct = default)
    {
        await db.Update<UserEntity>()
            .MatchID(user.ID)
            .Modify(b => b.Set(u => u.RoleNames, roleNames))
            .ExecuteAsync(ct);
        user.RoleNames = roleNames;
    }

    /// <summary>Returns whether any user currently holds the Admin role.</summary>
    public async Task<bool> AnyAdminAsync(CancellationToken ct = default)
        => (await db.Find<UserEntity>().Match(u => u.RoleNames.Contains("Admin")).ExecuteAnyAsync(ct));

    /// <summary>Atomically adds a role to a user without rewriting the whole document.</summary>
    public async Task AddRoleAsync(UserEntity user, string roleName, CancellationToken ct = default)
    {
        await db.Update<UserEntity>()
            .MatchID(user.ID)
            .Modify(b => b.AddToSet(u => u.RoleNames, roleName))
            .ExecuteAsync(ct);
        if (!user.RoleNames.Contains(roleName))
            user.RoleNames.Add(roleName);
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
