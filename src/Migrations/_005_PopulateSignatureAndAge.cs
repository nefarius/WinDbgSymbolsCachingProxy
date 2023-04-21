using MongoDB.Driver;
using MongoDB.Entities;

using Serilog;

using Smx.PDBSharp;

using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Migrations;

public class _005_PopulateSignatureAndAge : IMigration
{
    public async Task UpgradeAsync()
    {
        await DB.Fluent<SymbolsEntity>()
            .Project(symbol => ExtractSignatureAndAge(symbol))
            .ForEachAsync(async symbol =>
            {
                await DB.Update<SymbolsEntity>()
                    .Match(_ => true)
                    .Modify(entity => entity.Signature, symbol.Signature)
                    .Modify(entity => entity.NewSignature, symbol.NewSignature)
                    .Modify(entity => entity.Age, symbol.Age)
                    .ExecuteAsync();
            });
    }

    private PdbMeta ExtractSignatureAndAge(SymbolsEntity symbol)
    {
        // TODO: implement me!

        var meta = new PdbMeta();
        
        using MemoryStream ms = new();
        symbol.Data.DownloadAsync(ms).RunSynchronously();
        ms.Position = 0;
        
        using PDBFile? pdb = PDBFile.Open(ms);
        
        if (pdb is null)
        {
            Log.Logger.Warning("Couldn't parse {IndexPrefix} as PDB file", symbol.IndexPrefix);
            return meta;
        }

        return meta;
    }
    
    private class PdbMeta
    {
        public uint Signature { get; set; } = 0;
        public Guid? NewSignature { get; set; } = null;
        public uint Age { get; set; } = 0;
    }
}