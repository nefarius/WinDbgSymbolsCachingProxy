using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;

using Microsoft.Diagnostics.Runtime;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Pdb;

namespace Nefarius.Utilities.ExceptionEnricher;

internal class OnlineServerSymbolsResolver : ISymbolReaderProvider
{
    private readonly HttpClient _httpClient;
    private readonly bool _throwIfNoSymbol;

    public OnlineServerSymbolsResolver(HttpClient httpClient, bool throwIfNoSymbol = true)
    {
        _httpClient = httpClient;
        _throwIfNoSymbol = throwIfNoSymbol;
    }

    public ISymbolReader? GetSymbolReader(ModuleDefinition module, string fileName)
    {
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

        using DataTarget dt = DataTarget.CreateSnapshotAndAttach(Process.GetCurrentProcess().Id);

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

        HttpResponseMessage response = _httpClient.Send(new HttpRequestMessage(HttpMethod.Get, request));

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
                    return null;
                }
            }

            if (_throwIfNoSymbol)
            {
                throw new HttpRequestException($"Couldn't find remote symbol for file: {fileName}");
            }

            return null;
        }

        using Stream ss = response.Content.ReadAsStream();

        return new PdbReaderProvider().GetSymbolReader(module, ss);
    }

    public ISymbolReader GetSymbolReader(ModuleDefinition module, Stream symbolStream)
    {
        throw new NotImplementedException();
    }
}