using JetBrains.Annotations;

using Microsoft.AspNetCore.Components;

using MudBlazor;

using WinDbgSymbolsCachingProxy.Models;
using WinDbgSymbolsCachingProxy.Services;

namespace WinDbgSymbolsCachingProxy.Components.Pages.Admin;

[UsedImplicitly]
public partial class Roles
{
    [Inject] private RoleService RoleService { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    private List<RoleEntity> _roles = [];
    private bool _loading = true;
    private bool _dialogOpen;
    private RoleEntity? _editingRole;

    private static readonly DialogOptions _dialogOptions = new()
    {
        MaxWidth = MaxWidth.Small,
        FullWidth = true,
        CloseOnEscapeKey = true
    };

    protected override async Task OnInitializedAsync()
    {
        await ReloadAsync();
        _loading = false;
    }

    private async Task ReloadAsync()
    {
        _roles = await RoleService.GetAllAsync();
    }

    private void OpenCreateDialog()
    {
        _editingRole = new RoleEntity();
        _dialogOpen = true;
    }

    private void OpenEditDialog(RoleEntity role)
    {
        _editingRole = new RoleEntity
        {
            ID = role.ID,
            Name = role.Name,
            Description = role.Description,
            Permissions = [.. role.Permissions],
            IsSystemRole = role.IsSystemRole
        };
        _dialogOpen = true;
    }

    private void CloseDialog()
    {
        _dialogOpen = false;
        _editingRole = null;
    }

    private void TogglePermission(string permission, bool add)
    {
        if (_editingRole is null) return;

        if (add && !_editingRole.Permissions.Contains(permission))
            _editingRole.Permissions.Add(permission);
        else if (!add)
            _editingRole.Permissions.Remove(permission);
    }

    private async Task SaveRoleAsync()
    {
        if (_editingRole is null) return;

        try
        {
            await RoleService.SaveAsync(_editingRole);
            Snackbar.Add($"Role '{_editingRole.Name}' saved.", Severity.Success);
            CloseDialog();
            await ReloadAsync();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Save failed: {ex.Message}", Severity.Error);
        }
    }

    private async Task DeleteRoleAsync(RoleEntity role)
    {
        try
        {
            await RoleService.DeleteAsync(role.ID);
            Snackbar.Add($"Role '{role.Name}' deleted.", Severity.Success);
            await ReloadAsync();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Delete failed: {ex.Message}", Severity.Error);
        }
    }
}
