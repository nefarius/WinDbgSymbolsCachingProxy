using Riok.Mapperly.Abstractions;

using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Mappers;

/// <summary>
///     Maps properties between <see cref="SymbolsEntity" /> and <see cref="SymbolsEntityDto" /> objects.
/// </summary>
[Mapper]
public partial class SymbolsEntityMapper
{
#pragma warning disable RMG020
    /// <summary>
    ///     Maps a <see cref="SymbolsEntity" /> to a <see cref="SymbolsEntityDto" />.
    /// </summary>
    /// <param name="entity">The <see cref="SymbolsEntity" /> object to map from.</param>
    /// <returns>A new <see cref="SymbolsEntityDto" /> object with mapped properties.</returns>
    [MapperIgnoreTarget(nameof(SymbolsEntityDto.Blob))]
    public partial SymbolsEntityDto SymbolsEntityToSymbolsEntityDto(SymbolsEntity entity);
#pragma warning restore RMG020
}