using System.Diagnostics;

using Microsoft.Diagnostics.Runtime;

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace FieldReleasedDemoApp;

public class MySymResolver : ISymbolReaderProvider
{
    public ISymbolReader GetSymbolReader(ModuleDefinition module, string fileName)
    {
        using DataTarget dt = DataTarget.CreateSnapshotAndAttach(Process.GetCurrentProcess().Id);

        dt.SetSymbolPath("srv*http://192.168.2.12:5000/download/symbols");

        ClrRuntime runtime = dt.ClrVersions[0].CreateRuntime();
        ClrModule? clrModule =
            runtime.EnumerateModules()
                .SingleOrDefault(m => m.Name.Equals(module.FileName, StringComparison.OrdinalIgnoreCase));

        if (clrModule != null)
        {
            Console.WriteLine($"Module: {clrModule.Name}");
            PdbInfo? pdb = clrModule.Pdb;
            if (pdb != null)
            {
                Console.WriteLine($"PDB Found: {pdb.Path}");
                //Console.WriteLine($"Downloaded from symbol server: {symbolPath}");
            }
            else
            {
                Console.WriteLine($"No PDB found for {clrModule.Name}");
            }
        }

        return new PortablePdbReaderProvider().GetSymbolReader(module, fileName);

        /*
        if (module.Image.HasDebugTables ())
            return null;

        if (module.HasDebugHeader) {
            var header = module.GetDebugHeader ();
            var entry = header.GetEmbeddedPortablePdbEntry ();
            if (entry != null)
                return new EmbeddedPortablePdbReaderProvider ().GetSymbolReader (module, fileName);
        }

        var pdb_file_name = Mixin.GetPdbFileName (fileName);

        if (File.Exists (pdb_file_name)) {
            if (Mixin.IsPortablePdb (Mixin.GetPdbFileName (fileName)))
                return new PortablePdbReaderProvider ().GetSymbolReader (module, fileName);

            try {
                return SymbolProvider.GetReaderProvider (SymbolKind.NativePdb).GetSymbolReader (module, fileName);
            } catch (Exception) {
                // We might not include support for native pdbs.
            }
        }

        var mdb_file_name = Mixin.GetMdbFileName (fileName);
        if (File.Exists (mdb_file_name)) {
            try {
                return SymbolProvider.GetReaderProvider (SymbolKind.Mdb).GetSymbolReader (module, fileName);
            } catch (Exception) {
                // We might not include support for mdbs.
            }
        }

        if (throw_if_no_symbol)
            throw new SymbolsNotFoundException (string.Format ("No symbol found for file: {0}", fileName));

        return null;
        */

        throw new NotImplementedException();
    }

    public ISymbolReader GetSymbolReader(ModuleDefinition module, Stream symbolStream)
    {
        throw new NotImplementedException();
    }
}