using JetBrains.Annotations;

using Microsoft.AspNetCore.Components;

using MudBlazor;

using WinDbgSymbolsCachingProxy.Models;
using WinDbgSymbolsCachingProxy.Services;

namespace WinDbgSymbolsCachingProxy.Components.Pages.Admin;

[UsedImplicitly]
public partial class ApiKeys
{
    [Inject] private ApiKeyService ApiKeyService { get; set; } = null!;
    [Inject] private RoleService RoleService { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    private List<ApiKeyEntity> _keys = [];
    private List<RoleEntity> _roles = [];
    private bool _loading = true;
    private bool _dialogOpen;
    private string _newKeyPlaintext = "";

    private string _newName = "";
    private string _newOwner = "";
    private string _newExpiryText = "";
    private readonly List<string> _newRoles = [];

    private static readonly DialogOptions _dialogOptions = new()
    {
        MaxWidth = MaxWidth.Small,
        FullWidth = true,
        CloseOnEscapeKey = true
    };

    protected override async Task OnInitializedAsync()
    {
        _keys = await ApiKeyService.GetAllAsync();
        _roles = await RoleService.GetAllAsync();
        _loading = false;
    }

    private void OpenCreateDialog()
    {
        _newName = "";
        _newOwner = "";
        _newExpiryText = "";
        _newRoles.Clear();
        _dialogOpen = true;
    }

    private void ToggleNewRole(string roleName, bool add)
    {
        if (add && !_newRoles.Contains(roleName))
            _newRoles.Add(roleName);
        else if (!add)
            _newRoles.Remove(roleName);
    }

    private async Task CreateKeyAsync()
    {
        if (string.IsNullOrWhiteSpace(_newName))
        {
            Snackbar.Add("Name is required.", Severity.Warning);
            return;
        }

        DateTime? expiry = null;
        if (!string.IsNullOrWhiteSpace(_newExpiryText))
        {
            if (!DateTime.TryParse(_newExpiryText, out DateTime parsed))
            {
                Snackbar.Add("Invalid expiry date format.", Severity.Warning);
                return;
            }

            expiry = DateTime.SpecifyKind(parsed, DateTimeKind.Utc);
        }

        try
        {
            (ApiKeyEntity entity, string plaintext) = await ApiKeyService.CreateAsync(
                _newName.Trim(), [.. _newRoles], string.IsNullOrWhiteSpace(_newOwner) ? null : _newOwner.Trim(), expiry);

            _keys.Add(entity);
            _newKeyPlaintext = plaintext;
            _dialogOpen = false;
            Snackbar.Add("API key created. Copy the key from the banner above — it will not be shown again.", Severity.Info);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Create failed: {ex.Message}", Severity.Error);
        }
    }

    private async Task RevokeKeyAsync(ApiKeyEntity key)
    {
        try
        {
            await ApiKeyService.RevokeAsync(key.ID);
            key.Enabled = false;
            Snackbar.Add($"Key '{key.Name}' revoked.", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Revoke failed: {ex.Message}", Severity.Error);
        }
    }

    private async Task DeleteKeyAsync(ApiKeyEntity key)
    {
        try
        {
            await ApiKeyService.DeleteAsync(key.ID);
            _keys.Remove(key);
            Snackbar.Add($"Key '{key.Name}' deleted.", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Delete failed: {ex.Message}", Severity.Error);
        }
    }
}
