// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Reflection;
using System.Text;

using FieldReleasedDemoApp;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

try
{
    Foo();
}
catch (Exception ex)
{
    using HttpClient client = new();
    client.BaseAddress = new Uri("http://192.168.2.12:5000");

    // Analyze the stack trace
    StackTrace stackTrace = new(ex, true);

    StringBuilder enrichedStack = new();

    foreach (StackFrame frame in stackTrace.GetFrames())
    {
        MethodBase? method = frame.GetMethod();
        if (method is not null)
        {
            enrichedStack.Append($"   at {method.DeclaringType.FullName}.{method.Name}");

            // Try to resolve file and line numbers
            Module module = method.Module;
            string moduleName = module.FullyQualifiedName;

            AssemblyDefinition? assembly = AssemblyDefinition.ReadAssembly(
                moduleName,
                new ReaderParameters
                {
                    ReadSymbols = true, SymbolReaderProvider = new OnlineServerSymbolsResolver(client)
                });

            // Find the method in the assembly
            TypeDefinition? type = assembly.MainModule.GetType(method.DeclaringType.FullName);
            MethodDefinition? methodDef = type?.Methods.FirstOrDefault(m => m.Name == method.Name);

            if (methodDef != null)
            {
                // Resolve line number information from the PDB
                Collection<SequencePoint>? sequencePoints = methodDef.DebugInformation.SequencePoints;
                SequencePoint? firstSequencePoint = sequencePoints.FirstOrDefault();

                if (firstSequencePoint != null)
                {
                    enrichedStack.AppendLine(
                        $" in {firstSequencePoint.Document.Url} (line {firstSequencePoint.StartLine})");
                }
            }
            else
            {
                enrichedStack.AppendLine(" in <Method not found in the PDB.>");
            }
        }
    }

    // original
    Console.WriteLine("=== ORIGINAL ===");
    Console.WriteLine(ex.StackTrace);
    // enriched
    Console.WriteLine("=== ENRICHED ===");
    Console.WriteLine(enrichedStack.ToString());
}

return;

void Foo()
{
    Bar();
}

void Bar()
{
    Baz();
}

void Baz()
{
    throw new Exception("Test exception");
}