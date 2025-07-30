using Riok.Mapperly.Abstractions;

using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Mappers;

[Mapper]
public partial class SymbolsEntityMapper
{
#pragma warning disable RMG020
    public partial SymbolsEntityDto SymbolsEntityToSymbolsEntityDto(SymbolsEntity entity);
#pragma warning restore RMG020
}