using Riok.Mapperly.Abstractions;

using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Mappers;

[Mapper]
public partial class SymbolsEntityMapper
{
    public partial SymbolsEntityDto SymbolsEntityToSymbolsEntityDto(SymbolsEntity entity);
}