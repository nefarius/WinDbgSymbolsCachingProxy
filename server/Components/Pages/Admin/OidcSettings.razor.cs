using JetBrains.Annotations;

using Microsoft.AspNetCore.Components;

using MongoDB.Entities;

using MudBlazor;

using WinDbgSymbolsCachingProxy.Models;
using WinDbgSymbolsCachingProxy.Services;

namespace WinDbgSymbolsCachingProxy.Components.Pages.Admin;

[UsedImplicitly]
public partial class OidcSettings
{
    [Inject] private DB Db { get; set; } = null!;
    [Inject] private OidcConfigProvider OidcConfig { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    private OidcConfigEntity _model = new();
    private string _scopesText = "openid profile email";
    private string _bootstrapText = "";
    private bool _loading = true;
    private bool _saving;
    private string _currentModeName = "Basic auth";

    protected override async Task OnInitializedAsync()
    {
        _currentModeName = OidcConfig.IsOidcEnabled ? "OIDC" : "Basic auth";

        OidcConfigEntity? existing = await Db.Find<OidcConfigEntity>().ExecuteFirstAsync();
        if (existing is not null)
        {
            _model = existing;
        }

        _scopesText = string.Join(" ", _model.Scopes);
        _bootstrapText = string.Join("\n", _model.BootstrapAdminIdentifiers);
        _loading = false;
    }

    private async Task SaveAsync()
    {
        _saving = true;
        try
        {
            _model.Scopes = _scopesText
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();

            _model.BootstrapAdminIdentifiers = _bootstrapText
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();

            await Db.SaveAsync(_model);
            Snackbar.Add("Settings saved. Restart the service to apply.", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Save failed: {ex.Message}", Severity.Error);
        }
        finally
        {
            _saving = false;
        }
    }
}
