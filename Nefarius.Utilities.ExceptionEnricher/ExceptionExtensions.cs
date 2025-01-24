using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

namespace Nefarius.Utilities.ExceptionEnricher;

/// <summary>
///     Extension methods for <see cref="Exception" /> objects.
/// </summary>
public static class ExceptionExtensions
{
    /// <summary>
    ///     Rebuilds the provided exception with debug information fetched from an online symbol server.
    /// </summary>
    /// <param name="exception">The <see cref="Exception" /> object to enrich/rebuild.</param>
    /// <param name="httpClient">A <see cref="HttpClient" /> that specified the remote symbol server to contact.</param>
    /// <returns>A new <see cref="EnrichedException" /> object with debug information added.</returns>
    public static EnrichedException ToRemotelyEnrichedException(this Exception exception, HttpClient httpClient)
    {
        StackTrace stackTrace = new(exception, true);

        StringBuilder enrichedStack = new();

        foreach (StackFrame frame in stackTrace.GetFrames())
        {
            MethodBase? method = frame.GetMethod();

            if (method is null)
            {
                enrichedStack.Append("   at <Method not found.>");
                continue;
            }

            enrichedStack.Append($"   at {method.DeclaringType!.FullName}.{method.Name}");

            // Try to resolve file and line numbers
            Module module = method.Module;
            string moduleName = module.FullyQualifiedName;

            AssemblyDefinition? assembly = AssemblyDefinition.ReadAssembly(
                moduleName,
                new ReaderParameters
                {
                    ReadSymbols = true, SymbolReaderProvider = new OnlineServerSymbolsResolver(httpClient)
                });

            // Find the method in the assembly
            TypeDefinition? type = assembly.MainModule.GetType(method.DeclaringType.FullName);
            MethodDefinition? methodDef = type?.Methods.FirstOrDefault(m => m.Name == method.Name);

            if (methodDef is not null)
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

        return new EnrichedException(exception, enrichedStack.ToString());
    }
}