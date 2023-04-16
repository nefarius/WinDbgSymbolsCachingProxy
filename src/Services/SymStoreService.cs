using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;

using Microsoft.SymbolStore;
using Microsoft.SymbolStore.KeyGenerators;
using Microsoft.SymbolStore.SymbolStores;

using WinDbgSymbolsCachingProxy.Core;

namespace WinDbgSymbolsCachingProxy.Services;

public sealed class SymStoreService
{
    private static readonly HashSet<string> ValidExtensions =
        new(new[] { "", ".exe", ".dll", ".pdb", ".so", ".dbg", ".dylib", ".dwarf" });

    private static readonly HashSet<string> ValidSourceExtensions = new(new[] { ".cs", ".vb", ".h", ".cpp", ".inl" });
    private readonly List<string> CacheDirectories = new();
    private readonly List<string> InputFilePaths = new();
    private readonly List<ServerInfo> SymbolServers = new();
    private bool Debugging;
    private bool ForceWindowsPdbs;
    private bool Modules;
    private bool OutputByInputFile;
    private string OutputDirectory;
    private bool Packages;
    private bool Subdirectories;
    private bool Symbols;
    private ITracer Tracer;

    internal async Task DownloadFiles()
    {
        using (SymbolStore symbolStore = BuildSymbolStore())
        {
            bool verifyPackages = Packages && OutputDirectory == null;

            KeyTypeFlags flags = KeyTypeFlags.None;
            if (Symbols)
            {
                flags |= KeyTypeFlags.SymbolKey;
            }

            if (Modules)
            {
                flags |= KeyTypeFlags.IdentityKey;
            }

            if (Debugging)
            {
                flags |= KeyTypeFlags.ClrKeys;
            }

            if (flags == KeyTypeFlags.None)
            {
                if (verifyPackages)
                {
                    // If we are verifing symbol packages then only check the identity and clr keys.
                    flags = KeyTypeFlags.IdentityKey | KeyTypeFlags.ClrKeys;
                }
                else
                {
                    // The default is to download everything
                    flags = KeyTypeFlags.IdentityKey | KeyTypeFlags.SymbolKey | KeyTypeFlags.ClrKeys;
                }
            }

            if (ForceWindowsPdbs)
            {
                flags |= KeyTypeFlags.ForceWindowsPdbs;
            }

            foreach (SymbolStoreKeyWrapper wrapper in GetKeys(flags).Distinct())
            {
                SymbolStoreKey key = wrapper.Key;
                Tracer.Information("Key: {0} - {1}{2}", key.Index, key.FullPathName, key.IsClrSpecialFile ? "*" : "");

                if (symbolStore != null)
                {
                    using (SymbolStoreFile symbolFile = await symbolStore.GetFile(key, CancellationToken.None))
                    {
                        if (symbolFile != null)
                        {
                            await WriteFile(symbolFile, wrapper);
                        }

                        // If there is no output directory verify the file exists in the symbol store
                        if (OutputDirectory == null)
                        {
                            Tracer.WriteLine("Key {0}found {1} - {2}", symbolFile != null ? "" : "NOT ", key.Index,
                                key.FullPathName);
                        }
                    }
                }
            }
        }
    }

    [Obsolete("We don't want to deal with disk read and write, do MemoryStream etc. only.")]
    private async Task WriteFile(SymbolStoreFile file, SymbolStoreKeyWrapper wrapper)
    {
        if (OutputDirectory != null)
        {
            await WriteFileToDirectory(file.Stream, wrapper.Key.FullPathName, OutputDirectory);
        }

        if (OutputByInputFile && wrapper.InputFile != null)
        {
            await WriteFileToDirectory(file.Stream, wrapper.Key.FullPathName, Path.GetDirectoryName(wrapper.InputFile));
        }
    }

    [Obsolete("We don't want to deal with disk read and write, do MemoryStream etc. only.")]
    private async Task WriteFileToDirectory(Stream stream, string fileName, string destinationDirectory)
    {
        stream.Position = 0;
        string destination = Path.Combine(destinationDirectory, Path.GetFileName(fileName.Replace('\\', '/')));
        if (File.Exists(destination))
        {
            Tracer.Warning("Writing: {0} already exists", destination);
        }
        else
        {
            Tracer.WriteLine("Writing: {0}", destination);
            using (Stream destinationStream = File.OpenWrite(destination))
            {
                await stream.CopyToAsync(destinationStream);
            }
        }
    }

    [SuppressMessage("ReSharper", "ConvertIfStatementToConditionalTernaryExpression")]
    [SuppressMessage("ReSharper", "LoopCanBeConvertedToQuery")]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    private SymbolStore? BuildSymbolStore()
    {
        SymbolStore? store = null;

        foreach (ServerInfo server in ((IEnumerable<ServerInfo>)SymbolServers).Reverse())
        {
            if (server.InternalSymwebServer)
            {
                store = new SymwebHttpSymbolStore(Tracer, store, server.Uri, server.PersonalAccessToken);
            }
            else
            {
                store = new HttpSymbolStore(Tracer, store, server.Uri, server.PersonalAccessToken);
            }
        }

        foreach (string cache in ((IEnumerable<string>)CacheDirectories).Reverse())
        {
            store = new CacheSymbolStore(Tracer, store, cache);
        }

        return store;
    }

    private IEnumerable<SymbolStoreKeyWrapper> GetKeys(KeyTypeFlags flags)
    {
        IEnumerable<string> inputFiles = InputFilePaths.SelectMany(file =>
        {
            string directory = Path.GetDirectoryName(file);
            string pattern = Path.GetFileName(file);
            return Directory.EnumerateFiles(string.IsNullOrWhiteSpace(directory) ? "." : directory, pattern,
                Subdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        });

        if (!inputFiles.Any())
        {
            throw new ArgumentException("Input files not found");
        }

        foreach (string inputFile in inputFiles)
        {
            foreach (KeyGenerator generator in GetKeyGenerators(inputFile))
            {
                foreach (SymbolStoreKeyWrapper wrapper in generator.GetKeys(flags)
                             .Select(key => new SymbolStoreKeyWrapper(key, inputFile)))
                {
                    yield return wrapper;
                }
            }
        }
    }

    private IEnumerable<KeyGenerator> GetKeyGenerators(string inputFile)
    {
        if (Packages)
        {
            // The package file needs to be opened for read/write so the zip archive can be created in update mode.
            using (Stream inputStream = File.Open(inputFile, FileMode.Open, FileAccess.ReadWrite, FileShare.Read))
            {
                // This needs to be update mode so streams created below are seekable which is required by
                // the key generation code. Because of this the zip archives should not be disposed which
                // would attempt to write any changes to the input file. It isn't neccesary either because
                // the actual file stream is disposed.
                ZipArchive archive = new(inputStream, ZipArchiveMode.Update, true);

                // For each entry in the nuget package
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    // Only files (no directories) with the proper extensions are processed
                    if (ShouldIndex(entry.FullName))
                    {
                        using (Stream zipFileStream = entry.Open())
                        {
                            SymbolStoreFile file = new(zipFileStream, entry.FullName);
                            yield return new FileKeyGenerator(Tracer, file);
                        }
                    }
                }
            }
        }
        else
        {
            using (Stream inputStream = File.Open(inputFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                SymbolStoreFile file = new(inputStream, inputFile);
                string extension = Path.GetExtension(inputFile);
                if (ValidSourceExtensions.Contains(extension))
                {
                    yield return new SourceFileKeyGenerator(Tracer, file);
                }
                else
                {
                    yield return new FileKeyGenerator(Tracer, file);
                }
            }
        }
    }

    /// <summary>
    ///     Gets <see cref="SymbolStoreKeyWrapper"/>s for one or more given files.
    /// </summary>
    /// <param name="flags">The <see cref="KeyTypeFlags"/> to use for analysis.</param>
    /// <param name="inputFiles">A list of tuples containing the file name and a stream to the file contents.</param>
    /// <returns>One or more <see cref="SymbolStoreKeyWrapper"/>s.</returns>
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
    ///     Gets <see cref="KeyGenerator"/>s for a given file.
    /// </summary>
    /// <param name="inputFile">The file name to analyze.</param>
    /// <param name="inputStream">The stream for the contents of the file.</param>
    /// <param name="isPackaged">True if the provided file is a zipped archive of multiple files, false otherwise (default).</param>
    /// <returns>One or more <see cref="KeyGenerator"/>s</returns>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public IEnumerable<KeyGenerator> GetKeyGenerators(string inputFile, Stream inputStream, bool isPackaged = false)
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
                yield return new FileKeyGenerator(Tracer, file);
            }
        }
        else
        {
            SymbolStoreFile file = new(inputStream, inputFile);
            string extension = Path.GetExtension(inputFile);
            if (ValidSourceExtensions.Contains(extension))
            {
                yield return new SourceFileKeyGenerator(Tracer, file);
            }
            else
            {
                yield return new FileKeyGenerator(Tracer, file);
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

    private struct ServerInfo
    {
        public Uri Uri { get; }
        public string PersonalAccessToken { get; }
        public bool InternalSymwebServer { get; }
    }
}