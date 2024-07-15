using System.Diagnostics;

using MongoDB.Entities;

using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Services;

public sealed class StartupService : BackgroundService
{
    private const string RunParser = "RunParser";
    private const string RunRecheck = "RunRecheck";
    private readonly IConfiguration _config;
    private readonly ILogger<StartupService> _logger;
    private readonly SymbolParsingService _parsingService;
    private readonly RecheckNotFoundService _recheckNotFoundService;

    public StartupService(RecheckNotFoundService recheckNotFoundService, ILogger<StartupService> logger,
        IConfiguration config, SymbolParsingService parsingService)
    {
        _recheckNotFoundService = recheckNotFoundService;
        _logger = logger;
        _config = config;
        _parsingService = parsingService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // run PDBSharp parsing for all DB entries, if enabled
        if (bool.TryParse(_config.GetSection(nameof(RunParser)).Value, out bool runParser) && runParser)
        {
            _logger.LogWarning(
                "{Setting} is enabled, this can severely impact startup performance on huge databases",
                nameof(RunParser));

            await ParseAllEntries(stoppingToken);
        }

        // run 404 re-check, if enabled
        if (bool.TryParse(_config.GetSection(nameof(RunRecheck)).Value, out bool runRecheck) && runRecheck)
        {
            _logger.LogWarning(
                "{Setting} is enabled, this can severely impact startup performance on huge databases",
                nameof(RunRecheck));

            Stopwatch sw = Stopwatch.StartNew();

            _logger.LogInformation("Running 404 re-check");

            await _recheckNotFoundService.Run(stoppingToken);

            sw.Stop();

            _logger.LogInformation("Re-check finished after {Timespan}", sw.Elapsed);
        }
    }

    /// <summary>
    ///     Fetches all found symbols from the database and enriches them with all missing PDB properties.
    /// </summary>
    private async Task ParseAllEntries(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Parsing all symbols ind database");

        List<SymbolsEntity> symbols = await DB.Find<SymbolsEntity>()
            .ManyAsync(sym => sym.NotFoundAt == null, stoppingToken);

        // https://stackoverflow.com/a/9290531
        ParallelOptions opts = new()
        {
            CancellationToken = stoppingToken,
            MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling(Environment.ProcessorCount * 0.75 * 1.0))
        };

        // boost performance by issuing requests in parallel
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
                SymbolParsingResult result = await _parsingService.ParseSymbol(symbol.FileName, ms, stoppingToken);

                symbol.Signature = result.Signature;
                symbol.NewSignature = result.NewSignature;
                symbol.Age = result.Age;

                _logger.LogInformation("Got {NewSignature} and {Age} for {IndexPrefix}",
                    symbol.NewSignature, symbol.Age, symbol.IndexPrefix);

                await symbol.SaveAsync(cancellation: token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse {File}", symbol.FileName);
            }
        });

        _logger.LogInformation("Parsing finished");
    }
}