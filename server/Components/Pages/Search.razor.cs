using JetBrains.Annotations;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

using MongoDB.Entities;

using MudBlazor;

using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Components.Pages;

[UsedImplicitly]
public partial class Search
{
    [Inject]
    private DB Db { get; set; } = null!;

    private MudMenu _contextMenu = null!;
    private SymbolsEntity? _contextRow;
    private MudDataGrid<SymbolsEntity> _dataGrid;
    private string _searchString;

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
    
    private async Task<GridData<SymbolsEntity>> ServerReload(GridState<SymbolsEntity> state)
    {
        string normalizedSearch = _searchString?.ToLowerInvariant() ?? "";

        (IReadOnlyList<SymbolsEntity> Results, long TotalCount, int PageCount) res = await Db
            .PagedSearch<SymbolsEntity>()
            .Match(b => string.IsNullOrEmpty(normalizedSearch) ||
                        b.FileName.Contains(normalizedSearch) ||
                        b.IndexPrefix.Contains(normalizedSearch) ||
                        b.SymbolKey.Contains(normalizedSearch) ||
                        (!string.IsNullOrEmpty(b.UpstreamFileName) && b.UpstreamFileName.Contains(normalizedSearch))
            )
            .Sort(b => b.FileName, Order.Ascending)
            .PageSize(_dataGrid.RowsPerPage)
            .PageNumber(_dataGrid.CurrentPage + 1)
            .ExecuteAsync();

        IEnumerable<SymbolsEntity> data = res.Results;

        SortDefinition<SymbolsEntity>? sortDefinition = state.SortDefinitions.FirstOrDefault();

        if (sortDefinition != null)
        {
            data = sortDefinition.SortBy switch
            {
                nameof(SymbolsEntity.FileName) => data.OrderByDirection(
                    sortDefinition.Descending ? SortDirection.Descending : SortDirection.Ascending, o => o.FileName),
                nameof(SymbolsEntity.IndexPrefix) => data.OrderByDirection(
                    sortDefinition.Descending ? SortDirection.Descending : SortDirection.Ascending, o => o.IndexPrefix),
                nameof(SymbolsEntity.SymbolKey) => data.OrderByDirection(
                    sortDefinition.Descending ? SortDirection.Descending : SortDirection.Ascending, o => o.SymbolKey),
                nameof(SymbolsEntity.UpstreamFileName) => data.OrderByDirection(
                    sortDefinition.Descending ? SortDirection.Descending : SortDirection.Ascending,
                    o => o.UpstreamFileName),
                _ => data
            };
        }

        return new GridData<SymbolsEntity> { TotalItems = (int)res.TotalCount, Items = data };
    }

    private Task OnSearch(string text)
    {
        _searchString = text?.ToLowerInvariant() ?? "";
        return _dataGrid.ReloadServerData();
    }
}