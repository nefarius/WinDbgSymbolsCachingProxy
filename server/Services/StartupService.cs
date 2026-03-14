using System.Diagnostics;

using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Entities;

using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Services;

/// <summary>
///     Hosted service that runs at startup: blocks until DB is ready (migrations, indexes), then optionally parses/rechecks.
/// </summary>
internal sealed class StartupService(
    DB db,
    RecheckNotFoundService recheckNotFoundService,
    ILogger<StartupService> logger,
    IConfiguration config,
    SymbolParsingService parsingService)
    : BackgroundService
{
    private const string RunParser = "RunParser";
    private const string RunRecheck = "RunRecheck";

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Running database migrations (if any)");
        await db.MigrateAsync<SymbolsEntity>();

        logger.LogInformation("Ensuring database indexes");
        await EnsureSymbolsIndexAsync(cancellationToken);

        await base.StartAsync(cancellationToken);
    }

    private async Task EnsureSymbolsIndexAsync(CancellationToken cancellationToken)
    {
        try
        {
            await db.Index<SymbolsEntity>()
                .Key(a => a.IndexPrefix, KeyType.Ascending)
                .Key(a => a.FileName, KeyType.Ascending)
                .Option(o => o.Unique = true)
                .CreateAsync(cancellationToken);
        }
        catch (MongoCommandException ex) when (ex.CodeName == "DuplicateKey")
        {
            logger.LogWarning(ex,
                "Unique index creation failed due to remaining duplicates; falling back to non-unique ascending index");

            await db.Index<SymbolsEntity>()
                .Key(a => a.IndexPrefix, KeyType.Ascending)
                .Key(a => a.FileName, KeyType.Ascending)
                .CreateAsync(cancellationToken);
        }
        catch (MongoCommandException ex) when (ex.CodeName == "IndexOptionsConflict" || ex.Code == 85)
        {
            logger.LogWarning(ex, "Index options conflict; dropping existing index and retrying unique index creation");

            IMongoCollection<SymbolsEntity> collection = db.Collection<SymbolsEntity>();
            using IAsyncCursor<BsonDocument> cursor = await collection.Indexes.ListAsync(cancellationToken);

            while (await cursor.MoveNextAsync(cancellationToken))
            {
                foreach (BsonDocument index in cursor.Current)
                {
                    if (!index.Contains("name") || index["name"].AsString == "_id_")
                    {
                        continue;
                    }

                    if (!index.TryGetValue("key", out BsonValue keyVal) || !keyVal.IsBsonDocument)
                    {
                        continue;
                    }

                    BsonDocument key = keyVal.AsBsonDocument;
                    if (key.ElementCount != 2)
                    {
                        continue;
                    }

                    if (key.Contains("IndexPrefix") && key.Contains("FileName") &&
                        key["IndexPrefix"].AsInt32 == 1 && key["FileName"].AsInt32 == 1)
                    {
                        await collection.Indexes.DropOneAsync(index["name"].AsString, cancellationToken);
                        await db.Index<SymbolsEntity>()
                            .Key(a => a.IndexPrefix, KeyType.Ascending)
                            .Key(a => a.FileName, KeyType.Ascending)
                            .Option(o => o.Unique = true)
                            .CreateAsync(cancellationToken);
                        return;
                    }
                }
            }

            await db.Index<SymbolsEntity>()
                .Key(a => a.IndexPrefix, KeyType.Ascending)
                .Key(a => a.FileName, KeyType.Ascending)
                .CreateAsync(cancellationToken);
        }
    }

    /// <summary>
    ///     Runs optional startup tasks (parser, 404 recheck) after DB is ready.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // run PDBSharp parsing for all DB entries, if enabled
        if (bool.TryParse(config.GetSection(nameof(RunParser)).Value, out bool runParser) && runParser)
        {
            logger.LogWarning(
                "{Setting} is enabled, this can severely impact startup performance on huge databases",
                nameof(RunParser));

            await ParseAllEntries(stoppingToken);
        }

        // run 404 re-check, if enabled
        if (bool.TryParse(config.GetSection(nameof(RunRecheck)).Value, out bool runRecheck) && runRecheck)
        {
            logger.LogWarning(
                "{Setting} is enabled, this can severely impact startup performance on huge databases",
                nameof(RunRecheck));

            Stopwatch sw = Stopwatch.StartNew();

            logger.LogInformation("Running 404 re-check");

            await recheckNotFoundService.Run(stoppingToken);

            sw.Stop();

            logger.LogInformation("Re-check finished after {Timespan}", sw.Elapsed);
        }
    }

    /// <summary>
    ///     Fetches all found symbols from the database and enriches them with all missing PDB properties.
    ///     Parses stored symbol blobs for all non-custom symbols that are not marked as not found, updates each symbol's signature and age, and saves the changes.
    /// </summary>
    /// <param name="stoppingToken">Cancellation token to stop the parsing operation.</param>
    private async Task ParseAllEntries(CancellationToken stoppingToken)
    {
        logger.LogInformation("Parsing all symbols ind database");

        List<SymbolsEntity> symbols = await db.Find<SymbolsEntity>()
            .ManyAsync(sym => sym.NotFoundAt == null && !sym.IsCustom, stoppingToken);

        // https://stackoverflow.com/a/9290531
        ParallelOptions opts = new()
        {
            CancellationToken = stoppingToken,
            MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling(Environment.ProcessorCount * 0.75 * 1.0))
        };

        // boost performance by issuing requests in parallel
        await Parallel.ForEachAsync(symbols, opts, async (symbol, innerToken) =>
        {
            logger.LogInformation("Processing {IndexPrefix}", symbol.IndexPrefix);

            using MemoryStream ms = new();
            try
            {
                await symbol.Data(db).DownloadAsync(ms, cancellation: innerToken);
            }
            catch (InvalidOperationException)
            {
                logger.LogWarning("No data blob available for {IndexPrefix}", symbol.IndexPrefix);
                return;
            }

            ms.Position = 0;

            try
            {
                SymbolParsingResult result = await parsingService.ParseSymbol(symbol.FileName, ms, innerToken);

                symbol.Signature = result.Signature;
                symbol.NewSignature = result.NewSignature;
                symbol.Age = result.Age;

                logger.LogInformation("Got {NewSignature} and {Age} for {IndexPrefix}",
                    symbol.NewSignature, symbol.Age, symbol.IndexPrefix);

                await db.SaveAsync(symbol, cancellation: innerToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to parse {File}", symbol.FileName);
            }
        });

        logger.LogInformation("Parsing finished");
    }
}