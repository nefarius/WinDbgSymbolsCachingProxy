using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;

using Microsoft.Diagnostics.Runtime;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Pdb;

namespace Nefarius.Utilities.ExceptionEnricher;

/// <summary>
///     An implementation of <see cref="ISymbolReaderProvider" /> that contacts an online symbol server to resolve debug
///     symbols.
/// </summary>
public sealed class OnlineServerSymbolsResolver : ISymbolReaderProvider, IDisposable
{
    private readonly string? _clientName;
    private readonly HttpClient? _httpClient;
    private readonly IHttpClientFactory? _httpClientFactory;
    private readonly Dictionary<string, Stream> _streamsCache = new();
    private readonly bool _throwIfNoSymbol;

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
        _httpClientFactory = httpClientFactory;
        _clientName = clientName;
        _throwIfNoSymbol = throwIfNoSymbol;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        foreach (KeyValuePair<string, Stream> stream in _streamsCache)
        {
            stream.Value.Dispose();
        }
    }

    /// <inheritdoc />
    public ISymbolReader? GetSymbolReader(ModuleDefinition module, string fileName)
    {
        if (_streamsCache.TryGetValue(fileName, out Stream? stream))
        {
            stream.Position = 0;
            return new PdbReaderProvider().GetSymbolReader(module, stream);
        }

        /*
         * Try from disk first, some symbols might already be in the assembly directory
         */

        try
        {
            return new PdbReaderProvider().GetSymbolReader(module, fileName);
        }
        catch
        {
            // ignored
        }

        using DataTarget dt = DataTarget.CreateSnapshotAndAttach(Environment.ProcessId);

        ClrRuntime runtime = dt.ClrVersions.First().CreateRuntime();
        ClrModule? clrModule =
            runtime.EnumerateModules()
                .SingleOrDefault(m =>
                    !string.IsNullOrEmpty(m.Name) &&
                    m.Name.Equals(module.FileName, StringComparison.OrdinalIgnoreCase));

        if (clrModule is null)
        {
            if (_throwIfNoSymbol)
            {
                throw new InvalidOperationException($"Couldn't find CLR module for file: {fileName}");
            }

            return null;
        }

        PdbInfo? pdb = clrModule.Pdb;

        if (pdb is null)
        {
            if (_throwIfNoSymbol)
            {
                throw new SymbolsNotFoundException($"No symbol found for file: {fileName}");
            }

            return null;
        }

        string pdbFileName = Path.GetFileName(pdb.Path);
        string requestPath = $"{pdbFileName}/{pdb.Guid:N}{pdb.Revision:X}/{pdbFileName}";

        string request = $"/download/symbols/{requestPath}";

        if (_httpClientFactory is null && _httpClient is null)
        {
            throw new InvalidOperationException("Neither http client factory nor http client instance was provided");
        }

        HttpClient? client = _httpClient ?? _httpClientFactory?.CreateClient(_clientName!);

        ArgumentNullException.ThrowIfNull(client);

        HttpResponseMessage response = client.Send(new HttpRequestMessage(HttpMethod.Get, request));

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                try
                {
                    return new PdbReaderProvider().GetSymbolReader(module, fileName);
                }
                catch
                {
                    if (_throwIfNoSymbol)
                    {
                        throw new HttpRequestException($"Couldn't find remote nor local symbol for file: {fileName}");
                    }

                    return null;
                }
            }

            if (_throwIfNoSymbol)
            {
                throw new HttpRequestException($"Couldn't find remote symbol for file: {fileName}");
            }

            return null;
        }

        Stream webStream = response.Content.ReadAsStream();

        _streamsCache[fileName] = webStream;

        return new PdbReaderProvider().GetSymbolReader(module, webStream);
    }

    /// <inheritdoc />
    public ISymbolReader GetSymbolReader(ModuleDefinition module, Stream symbolStream)
    {
        throw new NotImplementedException();
    }
}