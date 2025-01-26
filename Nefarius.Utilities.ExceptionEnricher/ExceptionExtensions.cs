using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class ExceptionExtensions
{
    /// <summary>
    ///     Rebuilds the provided exception with debug information fetched from an online symbol server.
    /// </summary>
    /// <param name="exception">The <see cref="Exception" /> object to enrich/rebuild.</param>
    /// <param name="provider">The <see cref="OnlineServerSymbolsResolver" /> to be used for symbol lookup.</param>
    /// <returns>A new <see cref="EnrichedException" /> object with debug information added.</returns>
    public static EnrichedException ToRemotelyEnrichedException(this Exception exception,
        OnlineServerSymbolsResolver provider)
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

            enrichedStack.Append($"   at {GetParameterString(method)}");

            // Try to resolve file and line numbers
            Module module = method.Module;
            string moduleName = module.FullyQualifiedName;

            AssemblyDefinition? assembly = AssemblyDefinition.ReadAssembly(
                moduleName,
                new ReaderParameters { ReadSymbols = true, SymbolReaderProvider = provider });

            if (assembly is null)
            {
                enrichedStack.AppendLine(" in <Failed to resolve assembly definition.>");
                continue;
            }

            if (method.DeclaringType is null)
            {
                enrichedStack.AppendLine(" in <Declaring type not found.>");
                continue;
            }

            // Find the method in the assembly
            TypeDefinition? type = assembly.MainModule.GetType(method.DeclaringType.FullName);
            MethodDefinition? methodDef = type?.Methods.FirstOrDefault(m => m.Name == method.Name);

            if (methodDef is not null)
            {
                // Resolve line number information from the PDB
                Collection<SequencePoint>? sequencePoints = methodDef.DebugInformation.SequencePoints;
                SequencePoint? firstSequencePoint = sequencePoints.FirstOrDefault();

                if (firstSequencePoint is not null)
                {
                    enrichedStack.AppendLine(
                        $" in {firstSequencePoint.Document.Url} (line {firstSequencePoint.StartLine + 1})");
                }
                else
                {
                    enrichedStack.AppendLine(" in <No sequence point found.>");
                }
            }
            else
            {
                enrichedStack.AppendLine(" in <Method not found in the PDB.>");
            }
        }

        return new EnrichedException(exception, enrichedStack.ToString());
    }

    /// <summary>
    ///     Rebuilds the provided exception with debug information fetched from an online symbol server.
    /// </summary>
    /// <param name="exception">The <see cref="Exception" /> object to enrich/rebuild.</param>
    /// <param name="httpClient">A <see cref="HttpClient" /> that specified the remote symbol server to contact.</param>
    /// <returns>A new <see cref="EnrichedException" /> object with debug information added.</returns>
    public static EnrichedException ToRemotelyEnrichedException(this Exception exception, HttpClient httpClient)
    {
        using OnlineServerSymbolsResolver provider = new(httpClient);

        return ToRemotelyEnrichedException(exception, provider);
    }

    private static string GetParameterString(MethodBase method)
    {
        ArgumentNullException.ThrowIfNull(method);

        ParameterInfo[] parameters = method.GetParameters();
        List<string> parameterStrings = new();

        foreach (ParameterInfo param in parameters)
        {
            string typeName = GetTypeDisplayName(param.ParameterType);
            if (param.GetCustomAttribute(typeof(ParamArrayAttribute)) != null)
            {
                typeName = "params " + typeName;
            }
            else if (param.IsOut)
            {
                typeName = "out " + typeName;
            }
            else if (param.ParameterType.IsByRef)
            {
                typeName = "ref " + typeName;
            }

            parameterStrings.Add($"{typeName} {param.Name}");
        }

        return $"{method.DeclaringType}.{method.Name}({string.Join(", ", parameterStrings)})";
    }

    private static string GetTypeDisplayName(Type type)
    {
        if (!type.IsGenericType)
        {
            return type.Name;
        }

        Type genericTypeDefinition = type.GetGenericTypeDefinition();
        string genericArguments = string.Join(", ", type.GetGenericArguments().Select(GetTypeDisplayName));
        return $"{genericTypeDefinition.Name.Split('`')[0]}<{genericArguments}>";
    }
}