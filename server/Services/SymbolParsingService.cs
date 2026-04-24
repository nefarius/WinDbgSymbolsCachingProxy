using System.Globalization;

using Kaitai;

using Microsoft.SymbolStore.KeyGenerators;

using Nefarius.Shared.PdbUtils.Extensions;

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
                    EnsureValidSignatureOrThrow(stream, filename);
                    filename = GetOriginalExecutableName(stream)?.ToLowerInvariant() ?? filename;
                    stream.Position = 0;
                    try
                    {
                        indexPrefix = await ParseExecutable(filename, stream);
                    }
                    catch (OverflowException ex)
                    {
                        throw new IncompleteSymbolFileException(
                            $"Executable file {filename} appears incomplete or corrupted (header read but structure overflow). " +
                            "Please make sure the file is fully written before uploading.",
                            ex);
                    }
                    catch (ArgumentOutOfRangeException ex)
                    {
                        throw new IncompleteSymbolFileException(
                            $"Executable file {filename} appears incomplete or corrupted (structure out of range). " +
                            "Please make sure the file is fully written before uploading.",
                            ex);
                    }
                    catch (EndOfStreamException ex)
                    {
                        throw new IncompleteSymbolFileException(
                            $"Executable file {filename} appears incomplete (unexpected end of stream). " +
                            "Please make sure the file is fully written before uploading.",
                            ex);
                    }

                    signature = indexPrefix.Split('/')[1].ToUpper();
                    logger.LogInformation("File {File} (EXE/DLL/SYS) has signature {Signature}", filename,
                        signature);
                    return new SymbolParsingResult(filename, indexPrefix);
                }
            case ".pdb":
                {
                    EnsureValidSignatureOrThrow(stream, filename);
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
    ///     Rejects uploads whose magic bytes do not match the declared file type. Prevents attempting to
    ///     parse zero-filled or truncated files and surfaces a clean, actionable error to the caller.
    /// </summary>
    private static void EnsureValidSignatureOrThrow(MemoryStream stream, string filename)
    {
        long originalPosition = stream.Position;
        try
        {
            stream.Position = 0;
            if (!SymbolFileSignature.HasValidMagic(stream, filename))
            {
                string extension = Path.GetExtension(filename).ToLowerInvariant();
                throw new IncompleteSymbolFileException(
                    $"Uploaded {extension.TrimStart('.').ToUpperInvariant()} file {filename} appears incomplete or corrupted " +
                    "(missing expected magic bytes). Please make sure the file is fully written before uploading.");
            }
        }
        finally
        {
            stream.Position = originalPosition;
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
        try
        {
            return await ParsePdbWithPdbSharp(fileName, stream);
        }
        catch (FailedToParsePdbException)
        {
            throw;
        }
        catch (IncompleteSymbolFileException)
        {
            throw;
        }
        catch (OverflowException ex)
        {
            throw new IncompleteSymbolFileException(
                $"PDB file {fileName} appears incomplete or corrupted (header read but structure overflow). " +
                "Please make sure the file is fully written before uploading.",
                ex);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            throw new IncompleteSymbolFileException(
                $"PDB file {fileName} appears incomplete or corrupted (stream table out of range). " +
                "Please make sure the file is fully written before uploading.",
                ex);
        }
        catch (EndOfStreamException ex)
        {
            throw new IncompleteSymbolFileException(
                $"PDB file {fileName} appears incomplete (unexpected end of stream). " +
                "Please make sure the file is fully written before uploading.",
                ex);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "PDBSharp failed for {File}; using Microsoft.SymbolStore key generation as fallback",
                fileName);
            stream.Position = 0;
            try
            {
                return ParsePdbIndexViaSymStore(fileName, stream);
            }
            catch (OverflowException symstoreEx)
            {
                // The PDB header was readable but its tables reference out-of-bounds data. This is the
                // fingerprint of a file that was only partially written when it was uploaded.
                throw new IncompleteSymbolFileException(
                    $"PDB file {fileName} appears incomplete or corrupted (header read but structure overflow). " +
                    "Please make sure the file is fully written before uploading.",
                    symstoreEx);
            }
            catch (ArgumentOutOfRangeException symstoreEx)
            {
                throw new IncompleteSymbolFileException(
                    $"PDB file {fileName} appears incomplete or corrupted (stream table out of range). " +
                    "Please make sure the file is fully written before uploading.",
                    symstoreEx);
            }
            catch (EndOfStreamException symstoreEx)
            {
                throw new IncompleteSymbolFileException(
                    $"PDB file {fileName} appears incomplete (unexpected end of stream). " +
                    "Please make sure the file is fully written before uploading.",
                    symstoreEx);
            }
        }
    }

    /// <summary>
    ///     Uses Microsoft.SymbolStore key generators on the PDB bytes (same approach as the partial fallbacks inside
    ///     <see cref="ParsePdbWithPdbSharp" /> when PDBSharp cannot read DBI / signature fields).
    /// </summary>
    private PdbParsingResult ParsePdbIndexViaSymStore(string fileName, MemoryStream stream)
    {
        stream.Position = 0;
        const KeyTypeFlags flags = KeyTypeFlags.IdentityKey | KeyTypeFlags.SymbolKey | KeyTypeFlags.ClrKeys;
        List<SymbolStoreKeyWrapper> keys = symStore.GetKeys(flags, fileName, stream).ToList();

        if (keys.Count == 0)
        {
            throw new FailedToParsePdbException(
                $"Couldn't derive a symbol index for {fileName} after PDBSharp failed.");
        }

        string indexPrefix = keys.First().Key.IndexPrefix.ToLowerInvariant();
        return TryBuildPdbParsingResultFromSymStoreIndexPrefix(indexPrefix);
    }

    /// <summary>
    ///     SSQP PDB keys use <c>filename/&lt;Signature&gt;&lt;Age&gt;/...</c> where Signature is the PDB GUID as 32 hex
    ///     digits (N) and Age is hex without leading zeros (e.g. portable PDB age <c>FFFFFFFF</c>).
    /// </summary>
    /// <returns>
    ///     A result with <see cref="PdbParsingResult.Age" /> and <see cref="PdbParsingResult.NewSignature" /> when the
    ///     middle segment matches; otherwise <see cref="PdbParsingResult" /> with only <see cref="PdbParsingResult.IndexPrefix" /> set.
    /// </returns>
    private static PdbParsingResult TryBuildPdbParsingResultFromSymStoreIndexPrefix(string indexPrefix)
    {
        string normalized = indexPrefix.TrimEnd('/').ToLowerInvariant();
        string[] segments = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 2)
            return new PdbParsingResult(indexPrefix);

        string combined = segments[1];
        // GUID (32 hex, N) + age (at least one hex digit; often 1 or 8 for FFFFFFFF)
        if (combined.Length < 33)
            return new PdbParsingResult(indexPrefix);

        ReadOnlySpan<char> guidSpan = combined.AsSpan(0, 32);
        if (!Guid.TryParseExact(guidSpan, "N", out Guid guid))
            return new PdbParsingResult(indexPrefix);

        ReadOnlySpan<char> ageHex = combined.AsSpan(32);
        if (!uint.TryParse(ageHex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint age))
            return new PdbParsingResult(indexPrefix);

        return new PdbParsingResult(indexPrefix, age, Signature: null, NewSignature: guid);
    }

    private async Task<PdbParsingResult> ParsePdbWithPdbSharp(string fileName, MemoryStream stream)
    {
        using PDBFile? pdb = PDBFile.Open(stream);

        if (pdb is null)
        {
            logger.LogWarning("PDBSharp returned null for {File}; using symstore fallback", fileName);
            stream.Position = 0;
            return ParsePdbIndexViaSymStore(fileName, stream);
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
                    const KeyTypeFlags flags = KeyTypeFlags.IdentityKey | KeyTypeFlags.SymbolKey | KeyTypeFlags.ClrKeys;

                    await using DBIReader? dbi = pdb.Services.GetService<DBIReader?>();

                    if (dbi is null)
                    {
                        logger.LogWarning("Couldn't get DBIReader, using symstore fallback");
                        stream.Position = 0;
                        return ParsePdbIndexViaSymStore(fileName, stream);
                    }

                    if (dbi.Header is not DBIHeaderNew hdr)
                    {
                        logger.LogWarning("Couldn't get DBIHeaderNew, using symstore fallback");
                        stream.Position = 0;
                        return ParsePdbIndexViaSymStore(fileName, stream);
                    }

                    uint age = hdr.Age;

                    await using PdbStreamReader? pdbStream = pdb.Services.GetService<PdbStreamReader>();

                    if (pdbStream.NewSignature is null)
                    {
                        logger.LogWarning("Couldn't get NewSignature, using symstore fallback");
                        stream.Position = 0;
                        return ParsePdbIndexViaSymStore(fileName, stream);
                    }

                    Guid guid = pdbStream.NewSignature.Value;

                    string key = $"{guid:N}{age:X}".ToUpperInvariant();
                    string indexPrefix = $"{fileName}/{key}/".ToLowerInvariant();

                    // ReSharper disable once DisposeOnUsingVariable
                    pdb.Dispose();
                    stream.Position = 0;

                    List<SymbolStoreKeyWrapper> keys = symStore.GetKeys(flags, fileName, stream).ToList();

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
    ///     Never throws: on any parser/resource-table failure a warning is logged and <c>null</c> is
    ///     returned so the caller falls back to the upload file name. Mirrors <see cref="GetOriginalPdbName" />.
    /// </summary>
    /// <param name="stream">The source stream.</param>
    /// <returns>The original executable name or null if not found / on parse error.</returns>
    private string? GetOriginalExecutableName(Stream stream)
    {
        long originalPosition = stream.CanSeek ? stream.Position : 0;

        try
        {
            if (stream.CanSeek)
            {
                stream.Position = 0;
            }

            PeFile peFile = new(stream);

            if (peFile.Resources?.VsVersionInfo is null)
            {
                return null;
            }

            if (peFile.Resources.VsVersionInfo.StringFileInfo?.StringTable is not { Length: > 0 } stringTableEntries)
            {
                return null;
            }

            StringTable stringTable = stringTableEntries.First();

            return string.IsNullOrWhiteSpace(stringTable.OriginalFilename)
                ? null
                : stringTable.OriginalFilename;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Failed to extract original executable name from stream. Falling back to upload name.");
            return null;
        }
        finally
        {
            if (stream.CanSeek)
            {
                stream.Position = originalPosition;
            }
        }
    }

    /// <summary>
    ///     Grabs the original file name from the PDB stream.
    /// </summary>
    /// <param name="stream">The source stream.</param>
    /// <returns>The PDB name or null if not found.</returns>
    private string? GetOriginalPdbName(Stream stream)
    {
        long originalPosition = stream.CanSeek ? stream.Position : 0;

        try
        {
            if (stream.CanSeek)
            {
                stream.Position = 0;
            }

            MsPdb pdb = new(new KaitaiStream(stream));
            return pdb.GetOriginalPdbName();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to extract original PDB file name from stream. Falling back to upload name.");
            return null;
        }
        finally
        {
            if (stream.CanSeek)
            {
                stream.Position = originalPosition;
            }
        }
    }

    private record PdbParsingResult(
        string IndexPrefix,
        uint? Age = null,
        uint? Signature = null,
        Guid? NewSignature = null);
}