using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Nefarius.Utilities.ExceptionEnricher;

/// <summary>
///     Extension methods for <see cref="Exception" /> objects.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class ExceptionExtensions
{
    /// <summary>
    ///     Hidden sequence points (compiler generated code) are marked with this line number.
    /// </summary>
    private const int HiddenLine = 0xFEEFEE;

    /// <summary>
    ///     Rebuilds the provided exception with debug information fetched from an online symbol server.
    ///     This method never throws because of a failed symbol lookup; frames that cannot be resolved are annotated
    ///     instead, so it is safe to call from exception handlers.
    /// </summary>
    /// <param name="exception">The <see cref="Exception" /> object to enrich/rebuild.</param>
    /// <param name="provider">The <see cref="OnlineServerSymbolsResolver" /> to be used for symbol lookup.</param>
    /// <returns>A new <see cref="EnrichedException" /> object with debug information added.</returns>
    public static EnrichedException ToRemotelyEnrichedException(this Exception exception,
        OnlineServerSymbolsResolver provider)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(provider);

        StringBuilder enrichedStack = new();
        // One read assembly per module for the whole stack trace; they are released (file handles!) at the end.
        Dictionary<string, AssemblyDefinition?> assemblies = new(StringComparer.OrdinalIgnoreCase);

        try
        {
            StackFrame[] frames = new StackTrace(exception, true).GetFrames() ?? [];

            foreach (StackFrame frame in frames)
            {
                try
                {
                    AppendFrame(enrichedStack, frame, provider, assemblies);
                }
                catch (Exception ex) when (ex is not OutOfMemoryException)
                {
                    // never let a symbol lookup problem mask the original exception
                    enrichedStack.AppendLine($" in <Symbol lookup failed: {ex.GetType().Name}: {ex.Message}>");
                }
            }
        }
        finally
        {
            foreach (AssemblyDefinition? assembly in assemblies.Values)
            {
                assembly?.Dispose();
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
        // Missing symbols must not turn into a new exception while another one is being reported.
        using OnlineServerSymbolsResolver provider = new(httpClient, false);

        return ToRemotelyEnrichedException(exception, provider);
    }

    private static void AppendFrame(StringBuilder sb, StackFrame frame, OnlineServerSymbolsResolver provider,
        Dictionary<string, AssemblyDefinition?> assemblies)
    {
        MethodBase? actualMethod = frame.GetMethod();

        if (actualMethod is null)
        {
            sb.AppendLine("   at <Method not found.>");
            return;
        }

        // Show the name the developer wrote (not the state machine's MoveNext) ...
        MethodBase displayMethod = GetOriginalMethod(actualMethod);
        sb.Append($"   at {GetParameterString(displayMethod)}");

        // ... but sequence points live in the method that actually executed.
        string? localFile = frame.GetFileName();
        int localLine = frame.GetFileLineNumber();
        if (!string.IsNullOrEmpty(localFile) && localLine > 0)
        {
            sb.AppendLine($" in {localFile} (line {localLine})");
            return;
        }

        string modulePath = actualMethod.Module.FullyQualifiedName;
        if (!File.Exists(modulePath))
        {
            sb.AppendLine(" in <Module file not found.>");
            return;
        }

        if (!assemblies.TryGetValue(modulePath, out AssemblyDefinition? assembly))
        {
            try
            {
                assembly = AssemblyDefinition.ReadAssembly(
                    modulePath,
                    new ReaderParameters { ReadSymbols = true, SymbolReaderProvider = provider });
            }
            catch (Exception ex) when (ex is not OutOfMemoryException)
            {
                assembly = null;
                sb.Append($" <Failed to load symbols: {ex.Message}>");
            }

            assemblies[modulePath] = assembly;
        }

        if (assembly is null)
        {
            sb.AppendLine(" in <Failed to resolve assembly definition.>");
            return;
        }

        MethodDefinition? methodDef = FindMethodDefinition(assembly.MainModule, actualMethod);
        if (methodDef is null)
        {
            sb.AppendLine(" in <Method not found in the PDB.>");
            return;
        }

        SequencePoint? sequencePoint = FindSequencePoint(methodDef, frame.GetILOffset());
        if (sequencePoint is null)
        {
            sb.AppendLine(" in <No sequence point found.>");
            return;
        }

        sb.AppendLine($" in {sequencePoint.Document.Url} (line {sequencePoint.StartLine})");
    }

    private static MethodDefinition? FindMethodDefinition(ModuleDefinition module, MethodBase method)
    {
        // The metadata token identifies the exact method, including overloads, generics and compiler generated members.
        if (method.MetadataToken != 0 &&
            module.LookupToken(method.MetadataToken) is MethodDefinition byToken)
        {
            return byToken;
        }

        return null;
    }

    /// <summary>
    ///     Finds the sequence point covering the given IL offset (the last one starting at or before it).
    /// </summary>
    private static SequencePoint? FindSequencePoint(MethodDefinition method, int ilOffset)
    {
        List<SequencePoint> points = method.DebugInformation.SequencePoints
            .Where(p => p.StartLine != HiddenLine)
            .OrderBy(p => p.Offset)
            .ToList();

        if (points.Count == 0)
        {
            return null;
        }

        if (ilOffset < 0)
        {
            return points[0];
        }

        SequencePoint? match = null;
        foreach (SequencePoint point in points)
        {
            if (point.Offset > ilOffset)
            {
                break;
            }

            match = point;
        }

        return match ?? points[0];
    }

    private static MethodBase GetOriginalMethod(MethodBase method)
    {
        // Only compiler generated state machines need mapping back to the method that was written in source.
        if (method is not { Name: "MoveNext", DeclaringType: { DeclaringType: { } containingType } stateMachineType })
        {
            return method;
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                                   BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        try
        {
            foreach (MethodInfo candidate in containingType.GetMethods(flags))
            {
                if (candidate.GetCustomAttribute<AsyncStateMachineAttribute>()?.StateMachineType == stateMachineType ||
                    candidate.GetCustomAttribute<IteratorStateMachineAttribute>()?.StateMachineType == stateMachineType)
                {
                    return candidate;
                }
            }
        }
        catch (Exception ex) when (ex is TypeLoadException or NotSupportedException or FileNotFoundException)
        {
            // fall through to the state machine method itself
        }

        return method;
    }

    private static string GetParameterString(MethodBase method)
    {
        ParameterInfo[] parameters = method.GetParameters();
        List<string> parameterStrings = new(parameters.Length);

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
