using JetBrains.Annotations;

using Microsoft.AspNetCore.Components;

using MudBlazor;

using WinDbgSymbolsCachingProxy.Models;
using WinDbgSymbolsCachingProxy.Services;

namespace WinDbgSymbolsCachingProxy.Components.Pages.Admin;

[UsedImplicitly]
public partial class Users
{
    [Inject] private UserService UserService { get; set; } = null!;
    [Inject] private RoleService RoleService { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    private List<UserEntity> _users = [];
    private List<RoleEntity> _roles = [];
    private bool _loading = true;

    protected override async Task OnInitializedAsync()
    {
        _users = await UserService.GetAllAsync();
        _roles = await RoleService.GetAllAsync();
        _loading = false;
    }

    private async Task OnRolesChanged(UserEntity user, IEnumerable<string> newRoles)
    {
        user.RoleNames = newRoles.ToList();
        try
        {
            await UserService.SaveAsync(user);
            Snackbar.Add($"Roles updated for {user.DisplayName ?? user.Subject}.", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Failed to save roles: {ex.Message}", Severity.Error);
        }
    }

    private async Task DeleteUserAsync(UserEntity user)
    {
        try
        {
            await UserService.DeleteAsync(user.ID);
            _users.Remove(user);
            Snackbar.Add("User deleted.", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Delete failed: {ex.Message}", Severity.Error);
        }
    }
}
