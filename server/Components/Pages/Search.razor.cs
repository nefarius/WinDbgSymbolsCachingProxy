using System.Text.RegularExpressions;

using JetBrains.Annotations;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Entities;

using MudBlazor;

using WinDbgSymbolsCachingProxy.Core;
using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Components.Pages;

internal enum SymbolStatusFilter
{
    All,
    Cached,
    Custom,
    NotFound
}

[UsedImplicitly]
public partial class Search
{
    private const string CompactTimestampFormat = "yyyy-MM-dd HH:mm";
    private const string FullTimestampFormat = "yyyy-MM-dd HH:mm:ss";

    [Inject]
    private DB Db { get; set; } = null!;

    [Inject]
    private IJSRuntime Js { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    [Inject]
    private IAuthorizationService AuthorizationService { get; set; } = null!;

    [Inject]
    private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;

    private MudMenu _contextMenu = null!;
    private SymbolsEntity? _contextRow;
    private MudDataGrid<SymbolsEntity> _dataGrid = null!;
    private int _lastPageItemCount;
    private string _searchString = "";
    private SymbolStatusFilter _statusFilter = SymbolStatusFilter.All;

    /// <summary>
    /// Deletes the currently selected SymbolsEntity, shows a success notification with its IndexPrefix, and reloads the data grid.
    /// </summary>
    /// <param name="obj">Mouse event args from the delete action.</param>
    /// <returns>A task that completes when the delete operation and grid refresh have finished.</returns>
    private async Task OnDeleteClick(MouseEventArgs obj)
    {
        if (_contextRow is null)
            return;

        // Server-side permission re-check: the page is accessible with SymbolsDownload,
        // so we must verify SymbolsDelete before performing the destructive action.
        AuthenticationState state = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        AuthorizationResult authResult = await AuthorizationService.AuthorizeAsync(
            state.User, null, Permissions.SymbolsDelete);

        if (!authResult.Succeeded)
            return;

        await Db.DeleteAsync<SymbolsEntity>(_contextRow.ID);
        Snackbar.Add($"Deleted {_contextRow.IndexPrefix}", Severity.Success);

        if (_dataGrid.CurrentPage > 0 && _lastPageItemCount <= 1)
            _dataGrid.NavigateTo(Page.Previous);
        else
            await _dataGrid.ReloadServerData();
    }

    private async Task OpenMenuContent(DataGridRowClickEventArgs<SymbolsEntity> args)
    {
        _contextRow = args.Item;
        await _contextMenu.OpenMenuAsync(args.MouseEventArgs);
    }

    /// <summary>
    /// Opens the symbol download URL in a new browser tab. No-op if the row has no data (NotFoundAt is set).
    /// </summary>
    private Task OnDownloadClick(MouseEventArgs _) => DownloadAsync(_contextRow);

    private Task OnCopyDownloadLinkClick(MouseEventArgs _) => CopyDownloadLinkAsync(_contextRow);

    private async Task DownloadAsync(SymbolsEntity? entity)
    {
        if (entity is null || entity.NotFoundAt.HasValue)
            return;

        await Js.InvokeVoidAsync("open", BuildDownloadUrl(entity), "_blank");
    }

    private Task CopyDownloadLinkAsync(SymbolsEntity? entity)
    {
        if (entity is null || entity.NotFoundAt.HasValue)
            return Task.CompletedTask;

        return CopyTextAsync(BuildDownloadUrl(entity), "Link copied");
    }

    private Task CopyFieldAsync(string? value) => CopyTextAsync(value, "Copied");

    private Task CopyAliasesAsync(IEnumerable<string> aliases) =>
        CopyFieldAsync(string.Join(", ", aliases));

    private string BuildDownloadUrl(SymbolsEntity entity) =>
        $"{Navigation.BaseUri.TrimEnd('/')}/download/symbols/{entity.RelativeUri}";

    private async Task CopyTextAsync(string? text, string successMessage)
    {
        if (string.IsNullOrEmpty(text))
            return;

        bool copied;
        try
        {
            copied = await Js.InvokeAsync<bool>("symbolsProxy.copyText", text);
        }
        catch (JSException)
        {
            copied = false;
        }

        if (copied)
            Snackbar.Add(successMessage, Severity.Success);
        else
            Snackbar.Add($"Could not copy. Value: {text}", Severity.Error);
    }

    private static string? FormatTimestamp(DateTime? value) =>
        value?.ToString(FullTimestampFormat);

    /// <summary>
    /// Loads a page of SymbolsEntity records for the data grid, applying filtering, sorting, and paging.
    /// All operations are performed server-side via MongoDB.Entities PagedSearch (no client-side sorting/filtering).
    /// </summary>
    /// <remarks>
    /// When the search box is empty, no text filter is applied. Otherwise matching is case-insensitive (MongoDB regex)
    /// against FileName, IndexPrefix, SymbolKey, UpstreamFileName, and any entry in AlternateRequestSymbols.
    /// An optional status filter (Cached / Custom / Not found) is combined with the text search.
    /// Sort is applied in MongoDB, with ID as a stable tie-breaker. Paging uses <c>state.Page</c> and <c>state.PageSize</c>.
    /// </remarks>
    /// <returns>A GridData&lt;SymbolsEntity&gt; containing the page of items in <c>Items</c> and the total number of matching items in <c>TotalItems</c>.</returns>
    private async Task<GridData<SymbolsEntity>> ServerReload(GridState<SymbolsEntity> state, CancellationToken cancellationToken)
    {
        var fb = Builders<SymbolsEntity>.Filter;
        MongoDB.Driver.FilterDefinition<SymbolsEntity> searchFilter = MongoDB.Driver.FilterDefinition<SymbolsEntity>.Empty;

        string normalizedSearch = _searchString.Trim();
        if (!string.IsNullOrEmpty(normalizedSearch))
        {
            var regex = new BsonRegularExpression(Regex.Escape(normalizedSearch), "i");
            searchFilter = fb.Or(
                fb.Regex(e => e.FileName, regex),
                fb.Regex(e => e.IndexPrefix, regex),
                fb.Regex(e => e.SymbolKey, regex),
                fb.Regex(e => e.UpstreamFileName, regex),
                fb.Regex(e => e.AlternateRequestSymbols, regex));
        }

        MongoDB.Driver.FilterDefinition<SymbolsEntity> statusFilter = _statusFilter switch
        {
            SymbolStatusFilter.Cached => fb.And(fb.Eq(e => e.IsCustom, false), fb.Eq(e => e.NotFoundAt, null)),
            SymbolStatusFilter.Custom => fb.Eq(e => e.IsCustom, true),
            SymbolStatusFilter.NotFound => fb.Ne(e => e.NotFoundAt, null),
            _ => MongoDB.Driver.FilterDefinition<SymbolsEntity>.Empty
        };

        var query = Db.PagedSearch<SymbolsEntity>().Match(fb.And(searchFilter, statusFilter));
        MudBlazor.SortDefinition<SymbolsEntity>? sortDefinition = state.SortDefinitions.FirstOrDefault();
        Order order = sortDefinition?.Descending == true ? Order.Descending : Order.Ascending;
        string? sortBy = sortDefinition?.SortBy;
        var withSort = sortBy switch
        {
            nameof(SymbolsEntity.FileName) => query.Sort(b => b.FileName, order),
            nameof(SymbolsEntity.IndexPrefix) => query.Sort(b => b.IndexPrefix, order),
            nameof(SymbolsEntity.SymbolKey) => query.Sort(b => b.SymbolKey, order),
            nameof(SymbolsEntity.UpstreamFileName) => query.Sort(b => b.UpstreamFileName, order),
            nameof(SymbolsEntity.CreatedAt) => query.Sort(b => b.CreatedAt, order),
            nameof(SymbolsEntity.UploadedAt) => query.Sort(b => b.UploadedAt, order),
            nameof(SymbolsEntity.NotFoundAt) => query.Sort(b => b.NotFoundAt, order),
            nameof(SymbolsEntity.IsCustom) => query.Sort(b => b.IsCustom, order),
            nameof(SymbolsEntity.AccessedCount) => query.Sort(b => b.AccessedCount, order),
            nameof(SymbolsEntity.LastAccessedAt) => query.Sort(b => b.LastAccessedAt, order),
            _ => query.Sort(b => b.FileName, Order.Ascending)
        };

        int pageSize = state.PageSize > 0 ? state.PageSize : 25;
        int pageNumber = Math.Max(0, state.Page) + 1;

        (IReadOnlyList<SymbolsEntity> Results, long TotalCount, int PageCount) res = await withSort
            .Sort(b => b.ID, Order.Ascending)
            .PageSize(pageSize)
            .PageNumber(pageNumber)
            .ExecuteAsync(cancellationToken);

        _lastPageItemCount = res.Results.Count;
        return new GridData<SymbolsEntity> { TotalItems = (int)res.TotalCount, Items = res.Results };
    }

    /// <summary>
    /// Reloads the data grid without changing the search filter.
    /// </summary>
    private Task OnRefreshClick(MouseEventArgs _) => _dataGrid.ReloadServerData();

    /// <summary>
    /// Updates the component's search filter and requests the data grid to reload from the first page.
    /// </summary>
    /// <param name="text">Search text to apply; null is treated as empty. Matching is case-insensitive.</param>
    /// <returns>A task that completes when the data grid has reloaded using the updated search filter.</returns>
    private Task OnSearch(string text)
    {
        _searchString = text ?? "";
        return ReloadFromFirstPageAsync();
    }

    private Task OnStatusFilterChanged(SymbolStatusFilter filter)
    {
        _statusFilter = filter;
        return ReloadFromFirstPageAsync();
    }

    private Task ReloadFromFirstPageAsync()
    {
        if (_dataGrid.CurrentPage != 0)
        {
            _dataGrid.NavigateTo(Page.First);
            return Task.CompletedTask;
        }

        return _dataGrid.ReloadServerData();
    }
}
