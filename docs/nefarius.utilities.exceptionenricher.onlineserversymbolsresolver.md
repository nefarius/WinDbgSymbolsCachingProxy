# OnlineServerSymbolsResolver

Namespace: Nefarius.Utilities.ExceptionEnricher

An implementation of  that contacts an online symbol server to resolve debug
 symbols.

```csharp
public sealed class OnlineServerSymbolsResolver : Mono.Cecil.Cil.ISymbolReaderProvider, System.IDisposable
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [OnlineServerSymbolsResolver](./nefarius.utilities.exceptionenricher.onlineserversymbolsresolver.md)<br>
Implements ISymbolReaderProvider, [IDisposable](https://docs.microsoft.com/en-us/dotnet/api/system.idisposable)

## Constructors

### <a id="constructors-.ctor"/>**OnlineServerSymbolsResolver(HttpClient, Boolean)**

A new instance of [OnlineServerSymbolsResolver](./nefarius.utilities.exceptionenricher.onlineserversymbolsresolver.md).

```csharp
public OnlineServerSymbolsResolver(HttpClient httpClient, bool throwIfNoSymbol)
```

#### Parameters

`httpClient` HttpClient<br>
The  to use to contact the symbol server.

`throwIfNoSymbol` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
If set, throws an exception if symbol resolving failed. If false, each missing symbol
 will be resolved as null.

## Methods

### <a id="methods-dispose"/>**Dispose()**

```csharp
public void Dispose()
```

### <a id="methods-getsymbolreader"/>**GetSymbolReader(ModuleDefinition, String)**

```csharp
public ISymbolReader GetSymbolReader(ModuleDefinition module, string fileName)
```

#### Parameters

`module` ModuleDefinition<br>

`fileName` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

#### Returns

ISymbolReader

### <a id="methods-getsymbolreader"/>**GetSymbolReader(ModuleDefinition, Stream)**

```csharp
public ISymbolReader GetSymbolReader(ModuleDefinition module, Stream symbolStream)
```

#### Parameters

`module` ModuleDefinition<br>

`symbolStream` [Stream](https://docs.microsoft.com/en-us/dotnet/api/system.io.stream)<br>

#### Returns

ISymbolReader
