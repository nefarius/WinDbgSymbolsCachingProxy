using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;

using Microsoft.SymbolStore;
using Microsoft.SymbolStore.KeyGenerators;

using WinDbgSymbolsCachingProxy.Core;

namespace WinDbgSymbolsCachingProxy.Services;

public sealed class SymStoreService
{
    private static readonly HashSet<string> ValidSourceExtensions = [..new[] { ".cs", ".vb", ".h", ".cpp", ".inl" }];
    private readonly ITracer _tracer;

    public SymStoreService(ITracer tracer)
    {
        _tracer = tracer;
    }

    private static HashSet<string> ValidExtensions { get; } =
        new(new[] { ".sys", ".exe", ".dll", ".pdb", ".so", ".dbg", ".dylib", ".dwarf" });

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public IEnumerable<SymbolStoreKeyWrapper> GetKeys(KeyTypeFlags flags, string inputFile, Stream inputStream)
    {
        Tuple<string, Stream> tuple = new(inputFile, inputStream);

        return GetKeys(flags, new[] { tuple });
    }

    /// <summary>
    ///     Gets <see cref="SymbolStoreKeyWrapper" />s for one or more given files.
    /// </summary>
    /// <param name="flags">The <see cref="KeyTypeFlags" /> to use for analysis.</param>
    /// <param name="inputFiles">A list of tuples containing the file name and a stream to the file contents.</param>
    /// <returns>One or more <see cref="SymbolStoreKeyWrapper" />s.</returns>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public IEnumerable<SymbolStoreKeyWrapper> GetKeys(KeyTypeFlags flags, IEnumerable<Tuple<string, Stream>> inputFiles)
    {
        foreach ((string inputFile, Stream inputStream) in inputFiles)
        {
            foreach (KeyGenerator generator in GetKeyGenerators(inputFile, inputStream))
            {
                foreach (SymbolStoreKeyWrapper wrapper in generator.GetKeys(flags)
                             .Select(key => new SymbolStoreKeyWrapper(key, inputFile)))
                {
                    yield return wrapper;
                }
            }
        }
    }

    /// <summary>
    ///     Gets <see cref="KeyGenerator" />s for a given file.
    /// </summary>
    /// <param name="inputFile">The file name to analyze.</param>
    /// <param name="inputStream">The stream for the contents of the file.</param>
    /// <param name="isPackaged">True if the provided file is a zipped archive of multiple files, false otherwise (default).</param>
    /// <returns>One or more <see cref="KeyGenerator" />s</returns>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    private IEnumerable<KeyGenerator> GetKeyGenerators(string inputFile, Stream inputStream, bool isPackaged = false)
    {
        if (isPackaged)
        {
            // This needs to be update mode so streams created below are seekable which is required by
            // the key generation code. Because of this the zip archives should not be disposed which
            // would attempt to write any changes to the input file. It isn't necessary either because
            // the actual file stream is disposed.
            ZipArchive archive = new(inputStream, ZipArchiveMode.Update, true);

            // For each entry in the nuget package
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                // Only files (no directories) with the proper extensions are processed
                if (!ShouldIndex(entry.FullName))
                {
                    continue;
                }

                using Stream zipFileStream = entry.Open();
                SymbolStoreFile file = new(zipFileStream, entry.FullName);
                yield return new FileKeyGenerator(_tracer, file);
            }
        }
        else
        {
            SymbolStoreFile file = new(inputStream, inputFile);
            string extension = Path.GetExtension(inputFile);
            if (ValidSourceExtensions.Contains(extension))
            {
                yield return new SourceFileKeyGenerator(_tracer, file);
            }
            else
            {
                yield return new FileKeyGenerator(_tracer, file);
            }
        }
    }

    private static bool ShouldIndex(string fullName)
    {
        if (fullName.EndsWith("/") || fullName.EndsWith("_.pdb"))
        {
            return false;
        }

        string extension = Path.GetExtension(fullName);
        return ValidExtensions.Contains(extension);
    }
}