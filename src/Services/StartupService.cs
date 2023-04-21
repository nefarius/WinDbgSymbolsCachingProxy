using System.Diagnostics;

using Microsoft.SymbolStore.KeyGenerators;

using MongoDB.Entities;

using Smx.PDBSharp;

using WinDbgSymbolsCachingProxy.Core;
using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Services;

public sealed class StartupService : BackgroundService
{
    private readonly ILogger<StartupService> _logger;
    private readonly RecheckNotFoundService _recheckNotFoundService;

    public StartupService(RecheckNotFoundService recheckNotFoundService, ILogger<StartupService> logger)
    {
        _recheckNotFoundService = recheckNotFoundService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
#if DEBUG

        _logger.LogInformation("Parsing all symbols ind database");

        List<SymbolsEntity>? symbols = await DB.Find<SymbolsEntity>()
            .ManyAsync(sym => sym.NotFoundAt == null, stoppingToken);

        // https://stackoverflow.com/a/9290531
        ParallelOptions opts = new()
        {
            CancellationToken = stoppingToken,
            MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling(Environment.ProcessorCount * 0.75 * 1.0))
        };

        await Parallel.ForEachAsync(symbols, opts, async (symbol, token) =>
        {
            _logger.LogInformation("Processing {IndexPrefix}", symbol.IndexPrefix);

            using MemoryStream ms = new();
            try
            {
                await symbol.Data.DownloadAsync(ms, cancellation: token);
            }
            catch (InvalidOperationException)
            {
                _logger.LogWarning("No data blob available for {IndexPrefix}", symbol.IndexPrefix);
                return;
            }

            ms.Position = 0;

            try
            {
                using PDBFile pdb = PDBFile.Open(ms);

                switch (pdb.Type)
                {
                    case PDBType.Old:
                        _logger.LogWarning("The PDB {IndexPrefix} version is not supported", symbol.IndexPrefix);
                        return;
                    case PDBType.Small:
                        {
                            await using DBIReader dbi = pdb.Services.GetService<DBIReader>();

                            if (dbi.Header is not DBIHeaderNew hdr)
                            {
                                _logger.LogError("Failed to parse PDB header of {IndexPrefix}", symbol.IndexPrefix);
                                return;
                            }

                            PdbStreamReader pdbStream = pdb.Services.GetService<PdbStreamReader>();

                            symbol.Signature = pdbStream.Signature;
                            symbol.Age = hdr.Age;

                            _logger.LogInformation("Got {Signature} and {Age} for {IndexPrefix}",
                                symbol.Signature, symbol.Age, symbol.IndexPrefix);

                            await symbol.SaveAsync(cancellation: token);
                            return;
                        }
                    case PDBType.Big:
                        {
                            List<SymbolStoreKeyWrapper> keys;
                            const KeyTypeFlags flags = KeyTypeFlags.IdentityKey | KeyTypeFlags.SymbolKey |
                                                       KeyTypeFlags.ClrKeys;

                            await using DBIReader dbi = pdb.Services.GetService<DBIReader>();

                            if (dbi.Header is not DBIHeaderNew hdr)
                            {
                                _logger.LogWarning("Couldn't get DBIHeaderNew for {IndexPrefix}", symbol.IndexPrefix);
                                return;
                            }

                            await using PdbStreamReader? pdbStream = pdb.Services.GetService<PdbStreamReader>();

                            if (pdbStream.NewSignature is null)
                            {
                                _logger.LogWarning("Couldn't get NewSignature for {IndexPrefix}", symbol.IndexPrefix);
                                return;
                            }

                            symbol.Signature = pdbStream.Signature;
                            symbol.NewSignature = pdbStream.NewSignature;
                            symbol.Age = hdr.Age;

                            _logger.LogInformation("Got {NewSignature} and {Age} for {IndexPrefix}",
                                symbol.NewSignature, symbol.Age, symbol.IndexPrefix);

                            await symbol.SaveAsync(cancellation: token);
                            return;
                        }
                    default:
                        _logger.LogWarning("Couldn't find the signature of PDB {IndexPrefix}", symbol.IndexPrefix);
                        return;
                }
            }
            catch (InvalidDataException ex)
            {
                _logger.LogWarning(ex, "Failed to open {IndexPrefix}", symbol.IndexPrefix);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                _logger.LogWarning(ex, "Failed to open {IndexPrefix}", symbol.IndexPrefix);
            }
            catch (NullReferenceException ex)
            {
                _logger.LogWarning(ex, "Failed to open {IndexPrefix}", symbol.IndexPrefix);
            }
        });

        _logger.LogInformation("Parsing finished");
        
        return;
#endif

        Stopwatch sw = Stopwatch.StartNew();

        _logger.LogInformation("Running 404 re-check");

        await _recheckNotFoundService.Run(stoppingToken);

        sw.Stop();

        _logger.LogInformation("Re-check finished after {Timespan}", sw.Elapsed);
    }
}