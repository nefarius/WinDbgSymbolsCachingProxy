using Riok.Mapperly.Abstractions;

using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Mappers;

[Mapper]
public partial class SymbolsEntityMapper
{
#pragma warning disable RMG020
    [MapperIgnoreTarget(nameof(SymbolsEntityDto.Blob))] 
    public partial SymbolsEntityDto SymbolsEntityToSymbolsEntityDto(SymbolsEntity entity);
#pragma warning restore RMG020
}