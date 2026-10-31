using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Pdb;

namespace Nefarius.Utilities.ExceptionEnricher;

/// <summary>
///     An implementation of <see cref="ISymbolReaderProvider" /> that contacts an online symbol server to resolve debug
///     symbols. Instances are safe to use from multiple threads.
/// </summary>
public sealed class OnlineServerSymbolsResolver : ISymbolReaderProvider, IDisposable
{
    /// <summary>
    ///     Portable PDBs are keyed with this "age" instead of the age found in the module's CodeView debug directory.
    /// </summary>
    private const string PortablePdbAge = "FFFFFFFF";

    private readonly string? _clientName;
    private readonly HttpClient? _httpClient;
    private readonly IHttpClientFactory? _httpClientFactory;
    private readonly bool _throwIfNoSymbol;

    /// <summary>
    ///     Downloaded symbol files by symbol-server key; readers get their own stream over the cached bytes.
    /// </summary>
    private readonly ConcurrentDictionary<string, byte[]> _symbolCache = new(StringComparer.OrdinalIgnoreCase);

    private volatile bool _disposed;

    /// <summary>
    ///     A new instance of <see cref="OnlineServerSymbolsResolver" />.
    /// </summary>
    /// <param name="httpClient">The <see cref="HttpClient" /> to use to contact the symbol server.</param>
    /// <param name="throwIfNoSymbol">
    ///     If set, throws an exception if symbol resolving failed. If false, each missing symbol
    ///     will be resolved as null.
    /// </param>
    public OnlineServerSymbolsResolver(HttpClient httpClient, bool throwIfNoSymbol = true)
    {
        ArgumentNullException.ThrowIfNull(httpClient);

        _httpClient = httpClient;
        _throwIfNoSymbol = throwIfNoSymbol;
    }

    /// <summary>
    ///     A new instance of <see cref="OnlineServerSymbolsResolver" />.
    /// </summary>
    /// <param name="clientName">The name of the http client to request via the <paramref name="httpClientFactory" />.</param>
    /// <param name="throwIfNoSymbol">
    ///     If set, throws an exception if symbol resolving failed. If false, each missing symbol
    ///     will be resolved as null.
    /// </param>
    /// <param name="httpClientFactory">An <see cref="IHttpClientFactory" /> instance, when used with DI.</param>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public OnlineServerSymbolsResolver(IHttpClientFactory httpClientFactory, string clientName,
        bool throwIfNoSymbol = true)
    {
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentException.ThrowIfNullOrEmpty(clientName);

        _httpClientFactory = httpClientFactory;
        _clientName = clientName;
        _throwIfNoSymbol = throwIfNoSymbol;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _disposed = true;
        _symbolCache.Clear();
    }

    /// <inheritdoc />
    public ISymbolReader? GetSymbolReader(ModuleDefinition module, string fileName)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(module);

        // Symbols embedded in the assembly itself need no lookup at all.
        ISymbolReader? embedded = TryRead(() => new EmbeddedPortablePdbReaderProvider().GetSymbolReader(module, fileName));
        if (embedded is not null)
        {
            return embedded;
        }

        // Some symbols might already be in the assembly directory.
        if (File.Exists(fileName))
        {
            ISymbolReader? local = TryRead(() => new PdbReaderProvider().GetSymbolReader(module, fileName));
            if (local is not null)
            {
                return local;
            }
        }

        if (!TryGetCodeViewInfo(module, out Guid guid, out uint age, out string pdbFileName))
        {
            return Fail<ISymbolReader>(() =>
                new InvalidOperationException($"No debug information found for file: {fileName}"));
        }

        foreach (string key in GetCandidateKeys(guid, age))
        {
            byte[]? data = GetSymbolBytes(pdbFileName, key, out HttpStatusCode? status);
            if (data is not null)
            {
                try
                {
                    return new PdbReaderProvider().GetSymbolReader(module, new MemoryStream(data, false));
                }
                catch (Exception ex) when (!_throwIfNoSymbol &&
                                           ex is not OutOfMemoryException and not OperationCanceledException)
                {
                    // Unusable symbol data must not stay cached; try the next candidate key.
                    _symbolCache.TryRemove($"{pdbFileName}/{key}", out _);
                    continue;
                }
            }

            if (status is not (null or HttpStatusCode.NotFound))
            {
                return Fail<ISymbolReader>(() =>
                    new HttpRequestException($"Couldn't download remote symbol for file: {fileName} (HTTP {(int)status.Value})"));
            }
        }

        return Fail<ISymbolReader>(() =>
            new HttpRequestException($"Couldn't find remote nor local symbol for file: {fileName}"));
    }

    /// <inheritdoc />
    public ISymbolReader? GetSymbolReader(ModuleDefinition module, Stream symbolStream)
    {
        ArgumentNullException.ThrowIfNull(module);
        ArgumentNullException.ThrowIfNull(symbolStream);

        return new PdbReaderProvider().GetSymbolReader(module, symbolStream);
    }

    /// <summary>
    ///     Symbol server keys to try: the module's own age first (native/Windows PDBs), then the portable PDB convention.
    /// </summary>
    private static IEnumerable<string> GetCandidateKeys(Guid guid, uint age)
    {
        string prefix = guid.ToString("N");
        string primary = $"{prefix}{age:X}";
        string portable = $"{prefix}{PortablePdbAge}";

        yield return portable;

        if (!string.Equals(primary, portable, StringComparison.OrdinalIgnoreCase))
        {
            yield return primary;
        }
    }

    private byte[]? GetSymbolBytes(string pdbFileName, string key, out HttpStatusCode? status)
    {
        status = null;
        string cacheKey = $"{pdbFileName}/{key}";

        if (_symbolCache.TryGetValue(cacheKey, out byte[]? cached))
        {
            return cached;
        }

        HttpClient client = _httpClient ?? _httpClientFactory?.CreateClient(_clientName!)
            ?? throw new InvalidOperationException("Neither http client factory nor http client instance was provided");

        string escapedName = Uri.EscapeDataString(pdbFileName);

        // Relative (no leading slash) so a path component in the client's base address is preserved.
        using HttpRequestMessage request = new(HttpMethod.Get, $"download/symbols/{escapedName}/{key}/{escapedName}");

        HttpResponseMessage response;
        try
        {
            response = client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException or InvalidOperationException)
        {
            if (_throwIfNoSymbol)
            {
                throw;
            }

            return null;
        }

        using (response)
        {
            status = response.StatusCode;
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            byte[] data;
            try
            {
                using Stream content = response.Content.ReadAsStream();
                using MemoryStream buffer = new();
                content.CopyTo(buffer);
                data = buffer.ToArray();
            }
            catch (Exception ex) when (ex is HttpRequestException or IOException or OperationCanceledException
                                           or InvalidOperationException)
            {
                if (_throwIfNoSymbol)
                {
                    throw;
                }

                // Treated like a transient failure: nothing cached, the caller moves on to the next candidate key.
                status = null;
                return null;
            }

            _symbolCache[cacheKey] = data;
            return data;
        }
    }

    /// <summary>
    ///     Reads the CodeView (RSDS) record of the module: PDB GUID, age and file name.
    /// </summary>
    private static bool TryGetCodeViewInfo(ModuleDefinition module, out Guid guid, out uint age, out string pdbFileName)
    {
        guid = Guid.Empty;
        age = 0;
        pdbFileName = string.Empty;

        ImageDebugHeader? header = module.GetDebugHeader();
        if (header is null)
        {
            return false;
        }

        foreach (ImageDebugHeaderEntry entry in header.Entries.Where(e => e.Directory.Type == ImageDebugType.CodeView))
        {
            byte[] data = entry.Data;

            // "RSDS" + 16 byte GUID + 4 byte age + zero-terminated UTF-8 path
            if (data.Length < 25 || data[0] != 'R' || data[1] != 'S' || data[2] != 'D' || data[3] != 'S')
            {
                continue;
            }

            guid = new Guid(data.AsSpan(4, 16));
            age = BitConverter.ToUInt32(data, 20);

            int end = Array.IndexOf(data, (byte)0, 24);
            if (end < 0)
            {
                end = data.Length;
            }

            string path = Encoding.UTF8.GetString(data, 24, end - 24);
            // The path was recorded on the build machine and may use either separator regardless of this OS.
            pdbFileName = path[(path.LastIndexOfAny(['\\', '/']) + 1)..];

            return pdbFileName.Length > 0;
        }

        return false;
    }

    private static T? TryRead<T>(Func<T?> read) where T : class
    {
        try
        {
            return read();
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            return null;
        }
    }

    private T? Fail<T>(Func<Exception> createException) where T : class
    {
        if (_throwIfNoSymbol)
        {
            throw createException();
        }

        return null;
    }
}
