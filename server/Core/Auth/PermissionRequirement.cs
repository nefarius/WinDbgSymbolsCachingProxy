using Microsoft.AspNetCore.Authorization;

namespace WinDbgSymbolsCachingProxy.Core.Auth;

/// <summary>
///     Authorization requirement for a specific <see cref="Permissions"/> constant.
///     Evaluated by <see cref="PermissionAuthorizationHandler"/>.
/// </summary>
internal sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
