using Microsoft.AspNetCore.Components.Web;

using MongoDB.Entities;

using MudBlazor;

using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Components.Pages;

public partial class Search
{
    private MudMenu _contextMenu = null!;
    private SymbolsEntity? _contextRow;
    private MudDataGrid<SymbolsEntity> _dataGrid;
    private string _searchString;

    private Task OnDeleteClick(MouseEventArgs obj)
    {
        if (_contextRow is not null)
        {
            // TODO: delete symbol
        }

        throw new NotImplementedException();
    }

    private async Task OpenMenuContent(DataGridRowClickEventArgs<SymbolsEntity> args)
    {
        _contextRow = args.Item;
        await _contextMenu.OpenMenuAsync(args.MouseEventArgs);
    }

    /// <summary>
    ///     Here we simulate getting the paged, filtered and ordered data from the server
    /// </summary>
    private async Task<GridData<SymbolsEntity>> ServerReload(GridState<SymbolsEntity> state)
    {
        (IReadOnlyList<SymbolsEntity> Results, long TotalCount, int PageCount) res = await DB
            .PagedSearch<SymbolsEntity>()
            .Match(b => string.IsNullOrEmpty(_searchString) ||
                        b.FileName.Contains(_searchString) ||
                        b.IndexPrefix.Contains(_searchString) ||
                        b.SymbolKey.Contains(_searchString) ||
                        (!string.IsNullOrEmpty(b.UpstreamFileName) && b.UpstreamFileName.Contains(_searchString))
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
        _searchString = text;
        return _dataGrid.ReloadServerData();
    }
}