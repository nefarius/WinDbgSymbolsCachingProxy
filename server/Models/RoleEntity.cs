using MongoDB.Entities;

namespace WinDbgSymbolsCachingProxy.Models;

/// <summary>
///     An in-app role that aggregates a set of <see cref="Core.Permissions"/> strings.
///     Roles are assigned to <see cref="UserEntity"/> and <see cref="ApiKeyEntity"/> records.
/// </summary>
[Collection("roles")]
public class RoleEntity : Entity
{
    /// <summary>Unique, human-readable role name (e.g. <c>Admin</c>, <c>Uploader</c>).</summary>
    public string Name { get; set; } = null!;

    /// <summary>Optional description shown in the administration UI.</summary>
    public string? Description { get; set; }

    /// <summary>List of <see cref="Core.Permissions"/> constants granted by this role.</summary>
    public List<string> Permissions { get; set; } = [];

    /// <summary>System roles are seeded automatically and cannot be deleted via the UI.</summary>
    public bool IsSystemRole { get; set; }
}
