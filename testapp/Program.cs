// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Reflection;

using FieldReleasedDemoApp;

using Microsoft.Diagnostics.Runtime;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

try
{
    throw new Exception("Test exception");
}
catch (Exception ex)
{
    using DataTarget dt = DataTarget.CreateSnapshotAndAttach(Process.GetCurrentProcess().Id);

    dt.SetSymbolPath("srv*http://192.168.2.12:5000/download/symbols");

    
    
    // Analyze the stack trace
    StackTrace stackTrace = new(ex, true);

    foreach (StackFrame frame in stackTrace.GetFrames())
    {
        MethodBase? method = frame.GetMethod();
        if (method != null)
        {
            Console.WriteLine($"Method: {method.Name}");

            // Try to resolve file and line numbers
            Module module = method.Module;
            string moduleName = module?.FullyQualifiedName;

            if (!string.IsNullOrEmpty(moduleName))
            {
                ClrRuntime runtime = dt.ClrVersions[0].CreateRuntime();
                ClrModule? clrModule =
                    runtime.EnumerateModules()
                        .SingleOrDefault(m => m.Name.Equals(moduleName, StringComparison.OrdinalIgnoreCase));

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

                    AssemblyDefinition? assembly = AssemblyDefinition.ReadAssembly(
                            moduleName, 
                            new ReaderParameters
                            {
                                ReadSymbols = true,
                                SymbolReaderProvider = new MySymResolver()
                            });

                    // Find the method in the assembly
                    TypeDefinition? type = assembly.MainModule.GetType(method.DeclaringType.FullName);
                    MethodDefinition? methodDef = type?.Methods.FirstOrDefault(m => m.Name == method.Name);

                    if (methodDef != null)
                    {
                        // Resolve line number information from the PDB
                        Collection<SequencePoint>? sequencePoints = methodDef.DebugInformation.SequencePoints;
                        foreach (SequencePoint? sequencePoint in sequencePoints)
                        {
                            Console.WriteLine($"File: {sequencePoint.Document.Url}, Line: {sequencePoint.StartLine}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Method not found in the PDB.");
                    }
                }
            }
        }
    }

    Console.WriteLine(ex.StackTrace);
}