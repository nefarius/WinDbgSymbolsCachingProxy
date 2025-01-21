using MongoDB.Driver.Linq;
using MongoDB.Entities;

using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Components.Pages;

public partial class Search
{
    private string _searchString;
    
    private IEnumerable<SymbolsEntity> _elements = new List<SymbolsEntity>();

    protected override async Task OnInitializedAsync()
    {
        _elements = await DB.Queryable<SymbolsEntity>().Take(100).ToListAsync();
    }
}