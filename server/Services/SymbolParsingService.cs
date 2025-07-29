using Kaitai;

using Microsoft.SymbolStore.KeyGenerators;

using PeNet;
using PeNet.Header.Resource;

using Smx.PDBSharp;

using WinDbgSymbolsCachingProxy.Core;
using WinDbgSymbolsCachingProxy.Core.Exceptions;
using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Services;

internal sealed class SymbolParsingService(ILogger<SymbolParsingService> logger, SymStoreService symStore)
{
    /// <summary>
    ///     Extracts a <see cref="SymbolParsingResult" /> from a given symbol file.
    /// </summary>
    /// <param name="filename">The file name of the symbol.</param>
    /// <param name="stream">A <see cref="Stream" /> containing the binary content.</param>
    /// <param name="ct">Optional <see cref="CancellationToken" />.</param>
    /// <returns>A <see cref="SymbolParsingResult" />.</returns>
    /// <exception cref="UnsupportedFileTypeException">An unsupported file type/extension was provided.</exception>
    /// <exception cref="FailedToParseExecutableException">Failed to parse the provided executable (.exe, .dll, ...).</exception>
    /// <exception cref="FailedToParsePdbException">Failed to parse the provided debug symbols file (.pdb).</exception>
    public async Task<SymbolParsingResult> ParseSymbol(string filename, Stream stream, CancellationToken ct = default)
    {
        using MemoryStream ms = new();
        await stream.CopyToAsync(ms, 1024 * 64, ct);
        ms.Position = 0;

        return await ParseSymbol(filename, ms, ct);
    }

    /// <summary>
    ///     Extracts a <see cref="SymbolParsingResult" /> from a given symbol file.
    /// </summary>
    /// <param name="filename">The file name of the symbol.</param>
    /// <param name="stream">A <see cref="MemoryStream" /> containing the binary content.</param>
    /// <param name="ct">Optional <see cref="CancellationToken" />.</param>
    /// <returns>A <see cref="SymbolParsingResult" />.</returns>
    /// <exception cref="UnsupportedFileTypeException">An unsupported file type/extension was provided.</exception>
    /// <exception cref="FailedToParseExecutableException">Failed to parse the provided executable (.exe, .dll, ...).</exception>
    /// <exception cref="FailedToParsePdbException">Failed to parse the provided debug symbols file (.pdb).</exception>
    public async Task<SymbolParsingResult> ParseSymbol(string filename, MemoryStream stream,
        CancellationToken ct = default)
    {
        filename = filename.ToLowerInvariant();
        string extension = Path.GetExtension(filename).ToLowerInvariant();

        string indexPrefix;
        string signature;

        switch (extension)
        {
            case ".exe":
            case ".dll":
            case ".sys":
                {
                    filename = GetOriginalExecutableName(stream)?.ToLowerInvariant() ?? filename;
                    stream.Position = 0;
                    indexPrefix = await ParseExecutable(filename, stream);
                    signature = indexPrefix.Split('/')[1].ToUpper();
                    logger.LogInformation("File {File} (EXE/DLL/SYS) has signature {Signature}", filename,
                        signature);
                    return new SymbolParsingResult(filename, indexPrefix);
                }
            case ".pdb":
                {
                    filename = GetOriginalPdbName(stream)?.ToLowerInvariant() ?? filename;
                    stream.Position = 0;
                    PdbParsingResult result = await ParsePdb(filename, stream);
                    indexPrefix = result.IndexPrefix;
                    signature = indexPrefix.Split('/')[1].ToUpper();
                    logger.LogInformation("File {File} (PDB) has signature {Signature}", filename, signature);
                    return new SymbolParsingResult(filename, indexPrefix, result.Age, result.Signature,
                        result.NewSignature);
                }
            default:
                throw new UnsupportedFileTypeException($"File {filename} has unsupported extension.");
        }
    }

    /// <summary>
    ///     Parses various portable executable formats.
    /// </summary>
    /// <param name="fileName">The file name (with extension, without the path).</param>
    /// <param name="stream">The stream containing the file content.</param>
    /// <returns>The index prefix of the symbol.</returns>
    private Task<string> ParseExecutable(string fileName, MemoryStream stream)
    {
        const KeyTypeFlags flags = KeyTypeFlags.IdentityKey | KeyTypeFlags.SymbolKey | KeyTypeFlags.ClrKeys;

        IEnumerable<SymbolStoreKeyWrapper> keys = symStore.GetKeys(flags, fileName, stream).ToList();

        if (!keys.Any())
        {
            throw new FailedToParseExecutableException($"Couldn't get any keys for file {fileName}.");
        }

        SymbolStoreKeyWrapper key = keys.First();

        return Task.FromResult(key.Key.IndexPrefix.ToLowerInvariant());
    }

    /// <summary>
    ///     Parses various versions of PDB files.
    /// </summary>
    /// <param name="fileName">The file name (with extension, without the path).</param>
    /// <param name="stream">The stream containing the file content.</param>
    /// <returns>The index prefix of the PDB.</returns>
    /// <exception cref="FailedToParsePdbException">Thrown on error.</exception>
    private async Task<PdbParsingResult> ParsePdb(string fileName, MemoryStream stream)
    {
        using PDBFile? pdb = PDBFile.Open(stream);

        if (pdb is null)
        {
            throw new FailedToParsePdbException($"Couldn't parse {fileName} as PDB file.");
        }

        switch (pdb.Type)
        {
            case PDBType.Old:
                throw new FailedToParsePdbException($"The uploaded PDB {fileName} version is not supported.");
            case PDBType.Small:
                {
                    await using DBIReader dbi = pdb.Services.GetService<DBIReader>();

                    if (dbi.Header is not DBIHeaderNew hdr)
                    {
                        throw new FailedToParsePdbException("Failed to parse PDB header.");
                    }

                    PdbStreamReader? pdbStream = pdb.Services.GetService<PdbStreamReader>();

                    if (pdbStream is null)
                    {
                        throw new FailedToParsePdbException("Failed to get PDB stream.");
                    }

                    uint age = hdr.Age;
                    uint signature = pdbStream.Signature;

                    string key = $"{signature:X}{age:X}".ToUpperInvariant();
                    string indexPrefix = $"{fileName}/{key}/".ToLowerInvariant();

                    return new PdbParsingResult(indexPrefix, age, signature);
                }
            case PDBType.Big:
                {
                    List<SymbolStoreKeyWrapper> keys;
                    const KeyTypeFlags flags = KeyTypeFlags.IdentityKey | KeyTypeFlags.SymbolKey | KeyTypeFlags.ClrKeys;

                    await using DBIReader? dbi = pdb.Services.GetService<DBIReader?>();

                    if (dbi is null)
                    {
                        logger.LogWarning("Couldn't get DBIReader, using symstore fallback");

                        keys = symStore.GetKeys(flags, fileName, stream).ToList();

                        return new PdbParsingResult(keys.First().Key.IndexPrefix.ToLowerInvariant());
                    }

                    if (dbi.Header is not DBIHeaderNew hdr)
                    {
                        logger.LogWarning("Couldn't get DBIHeaderNew, using symstore fallback");

                        keys = symStore.GetKeys(flags, fileName, stream).ToList();

                        return new PdbParsingResult(keys.First().Key.IndexPrefix.ToLowerInvariant());
                    }

                    uint age = hdr.Age;

                    await using PdbStreamReader? pdbStream = pdb.Services.GetService<PdbStreamReader>();

                    if (pdbStream.NewSignature is null)
                    {
                        logger.LogWarning("Couldn't get NewSignature, using symstore fallback");

                        keys = symStore.GetKeys(flags, fileName, stream).ToList();

                        return new PdbParsingResult(keys.First().Key.IndexPrefix.ToLowerInvariant());
                    }

                    Guid guid = pdbStream.NewSignature.Value;

                    string key = $"{guid:N}{age:X}".ToUpperInvariant();
                    string indexPrefix = $"{fileName}/{key}/".ToLowerInvariant();

                    // ReSharper disable once DisposeOnUsingVariable
                    pdb.Dispose();
                    stream.Position = 0;

                    keys = symStore.GetKeys(flags, fileName, stream).ToList();

                    if (keys.Count == 0)
                    {
                        throw new FailedToParsePdbException("Failed to get symstore keys.");
                    }

                    SymbolStoreKeyWrapper symStoreKey = keys.First();

                    if (!indexPrefix.Equals(symStoreKey.Key.IndexPrefix, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new FailedToParsePdbException("PDB parsing signature+age mismatches symstore keys.",
                            (int)age, indexPrefix);
                    }

                    return new PdbParsingResult(indexPrefix, age, NewSignature: guid);
                }
            default:
                throw new FailedToParsePdbException($"Couldn't find the signature of PDB {fileName}.");
        }
    }

    /// <summary>
    ///     Grabs the original file name from the <c>.EXE</c>, <c>.DLL</c> or <c>.SYS</c> stream.
    /// </summary>
    /// <param name="stream">The source stream.</param>
    /// <returns>The original executable name or null if not found.</returns>
    private static string? GetOriginalExecutableName(Stream stream)
    {
        PeFile peFile = new(stream);

        if (peFile.Resources?.VsVersionInfo is null)
        {
            return null;
        }

        if (peFile.Resources.VsVersionInfo.StringFileInfo.StringTable.Length == 0)
        {
            return null;
        }

        StringTable stringTable = peFile.Resources.VsVersionInfo.StringFileInfo.StringTable.First();

        return stringTable.OriginalFilename;
    }

    /// <summary>
    ///     Grabs the original file name from the PDB stream.
    /// </summary>
    /// <param name="stream">The source stream.</param>
    /// <returns>The PDB name or null if not found.</returns>
    private static string? GetOriginalPdbName(Stream stream)
    {
        MsPdb pdb = new(new KaitaiStream(stream));
        MsPdb.UModuleInfo? pdbModule = pdb.DbiStream.ModulesList.Items
            .FirstOrDefault(info => info.Module.EcInfo.PdbFilenameIndex != 0);

        if (pdbModule is null)
        {
            return null;
        }

        uint index = pdbModule.Module.EcInfo.PdbFilenameIndex;
        string? pdbPathName = pdb.DbiStream.EcInfo.Strings.Strings.FirstOrDefault(s => s.CharsIndex == index)?.String;

        // required to work on Linux
        string? normalizedPath = pdbPathName?.Replace('\\', Path.DirectorySeparatorChar);

        return string.IsNullOrEmpty(normalizedPath) ? null : Path.GetFileName(normalizedPath);
    }

    private record PdbParsingResult(
        string IndexPrefix,
        uint? Age = null,
        uint? Signature = null,
        Guid? NewSignature = null);
}