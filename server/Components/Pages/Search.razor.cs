using System.Text.RegularExpressions;

using JetBrains.Annotations;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

using MongoDB.Bson;
using MongoDB.Entities;

using MudBlazor;

using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Components.Pages;

[UsedImplicitly]
public partial class Search
{
    [Inject]
    private DB Db { get; set; } = null!;

    [Inject]
    private IJSRuntime Js { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    private MudMenu _contextMenu = null!;
    private SymbolsEntity? _contextRow;
    private MudDataGrid<SymbolsEntity> _dataGrid;
    private string _searchString;

    /// <summary>
    /// Deletes the currently selected SymbolsEntity, shows a success notification with its IndexPrefix, and reloads the data grid.
    /// </summary>
    /// <param name="obj">Mouse event args from the delete action.</param>
    /// <returns>A task that completes when the delete operation and grid refresh have finished.</returns>
    private async Task OnDeleteClick(MouseEventArgs obj)
    {
        if (_contextRow is not null)
        {
            await Db.DeleteAsync<SymbolsEntity>(_contextRow.ID);

            Snackbar.Add($"Deleted {_contextRow.IndexPrefix}", Severity.Success);

            await _dataGrid.ReloadServerData();
        }
    }

    private async Task OpenMenuContent(DataGridRowClickEventArgs<SymbolsEntity> args)
    {
        _contextRow = args.Item;
        await _contextMenu.OpenMenuAsync(args.MouseEventArgs);
    }

    /// <summary>
    /// Opens the symbol download URL in a new browser tab. No-op if the row has no data (NotFoundAt is set).
    /// </summary>
    private async Task OnDownloadClick(MouseEventArgs _)
    {
        if (_contextRow is null || _contextRow.NotFoundAt.HasValue)
            return;

        var baseUri = Navigation.BaseUri.TrimEnd('/');
        var path = $"download/symbols/{_contextRow.RelativeUri}";
        var url = $"{baseUri}/{path}";
        await Js.InvokeVoidAsync("open", url, "_blank");
    }

    /// <summary>
    /// Loads a page of SymbolsEntity records for the data grid, applying filtering, sorting, and paging.
    /// All operations are performed server-side via MongoDB.Entities PagedSearch (no client-side sorting/filtering).
    /// </summary>
    /// <remarks>
    /// When the search box is empty, the filter matches all documents; otherwise it matches case-insensitively (MongoDB regex)
    /// against FileName, IndexPrefix, SymbolKey, and UpstreamFileName. Sort is applied in MongoDB (including timestamps and all columns).
    /// Paging uses <c>_dataGrid.RowsPerPage</c> and <c>_dataGrid.CurrentPage</c>.
    /// </remarks>
    /// <returns>A GridData&lt;SymbolsEntity&gt; containing the page of items in <c>Items</c> and the total number of matching items in <c>TotalItems</c>.</returns>
    private async Task<GridData<SymbolsEntity>> ServerReload(GridState<SymbolsEntity> state, CancellationToken cancellationToken)
    {
        string normalizedSearch = _searchString?.Trim() ?? "";
        // Case-insensitive "contains" via MongoDB regex; when empty, pattern ".*" matches all
        string pattern = string.IsNullOrEmpty(normalizedSearch) ? ".*" : Regex.Escape(normalizedSearch);
        var regex = new BsonRegularExpression(pattern, "i");

        var query = Db.PagedSearch<SymbolsEntity>()
            .Match(f => f.Or(
                f.Regex(b => b.FileName, regex),
                f.Regex(b => b.IndexPrefix, regex),
                f.Regex(b => b.SymbolKey, regex),
                f.And(f.Ne(b => b.UpstreamFileName, null), f.Regex(b => b.UpstreamFileName, regex))));

        SortDefinition<SymbolsEntity>? sortDefinition = state.SortDefinitions.FirstOrDefault();
        Order order = sortDefinition?.Descending == true ? Order.Descending : Order.Ascending;
        string? sortBy = sortDefinition?.SortBy?.ToString();
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

        (IReadOnlyList<SymbolsEntity> Results, long TotalCount, int PageCount) res = await withSort
            .PageSize(_dataGrid.RowsPerPage)
            .PageNumber(_dataGrid.CurrentPage + 1)
            .ExecuteAsync(cancellationToken);

        return new GridData<SymbolsEntity> { TotalItems = (int)res.TotalCount, Items = res.Results };
    }

    /// <summary>
    /// Reloads the data grid without changing the search filter.
    /// </summary>
    private Task OnRefreshClick(MouseEventArgs _) => _dataGrid.ReloadServerData();

    /// <summary>
    /// Updates the component's search filter and requests the data grid to reload.
    /// </summary>
    /// <param name="text">Search text to apply; null is treated as empty and the value is trimmed. Matching is case-insensitive.</param>
    /// <returns>A task that completes when the data grid has reloaded using the updated search filter.</returns>
    private Task OnSearch(string text)
    {
        _searchString = text?.Trim() ?? "";
        return _dataGrid.ReloadServerData();
    }
}