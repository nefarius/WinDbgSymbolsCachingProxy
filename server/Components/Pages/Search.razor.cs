using MongoDB.Driver.Linq;
using MongoDB.Entities;

using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Components.Pages;

public partial class Search
{
    private IEnumerable<SymbolsEntity> _elements = new List<SymbolsEntity>();
    private string _searchString;

    private Func<SymbolsEntity, bool> QuickFilter => x =>
    {
        if (string.IsNullOrWhiteSpace(_searchString))
        {
            return true;
        }

        if (x.FileName.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (x.IndexPrefix.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (x.SymbolKey.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    };

    protected override async Task OnInitializedAsync()
    {
        _elements = await DB.Queryable<SymbolsEntity>().Take(100).ToListAsync();
    }
}