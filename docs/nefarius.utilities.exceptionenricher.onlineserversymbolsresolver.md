# OnlineServerSymbolsResolver

Namespace: Nefarius.Utilities.ExceptionEnricher

An implementation of ISymbolReaderProvider that contacts an online symbol server to resolve debug
 symbols.

```csharp
public sealed class OnlineServerSymbolsResolver : Mono.Cecil.Cil.ISymbolReaderProvider, System.IDisposable
```

Inheritance [Object](https://learn.microsoft.com/dotnet/api/system.object) → [OnlineServerSymbolsResolver](./nefarius.utilities.exceptionenricher.onlineserversymbolsresolver.md)<br>
Implements ISymbolReaderProvider, [IDisposable](https://learn.microsoft.com/dotnet/api/system.idisposable)<br>
Attributes [NullableContextAttribute](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Constructors

### <a id="constructors-.ctor"/>**OnlineServerSymbolsResolver(HttpClient, Boolean)**

A new instance of [OnlineServerSymbolsResolver](./nefarius.utilities.exceptionenricher.onlineserversymbolsresolver.md).

```csharp
public OnlineServerSymbolsResolver(HttpClient httpClient, bool throwIfNoSymbol)
```

#### Parameters

`httpClient` [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient)<br>
The [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) to use to contact the symbol server.

`throwIfNoSymbol` [Boolean](https://learn.microsoft.com/dotnet/api/system.boolean)<br>
If set, throws an exception if symbol resolving failed. If false, each missing symbol
 will be resolved as null.

### <a id="constructors-.ctor"/>**OnlineServerSymbolsResolver(IHttpClientFactory, String, Boolean)**

A new instance of [OnlineServerSymbolsResolver](./nefarius.utilities.exceptionenricher.onlineserversymbolsresolver.md).

```csharp
public OnlineServerSymbolsResolver(IHttpClientFactory httpClientFactory, string clientName, bool throwIfNoSymbol)
```

#### Parameters

`httpClientFactory` [IHttpClientFactory](https://learn.microsoft.com/dotnet/api/system.net.http.ihttpclientfactory)<br>
An [IHttpClientFactory](https://learn.microsoft.com/dotnet/api/system.net.http.ihttpclientfactory) instance, when used with DI.

`clientName` [String](https://learn.microsoft.com/dotnet/api/system.string)<br>
The name of the http client to request via the `httpClientFactory`.

`throwIfNoSymbol` [Boolean](https://learn.microsoft.com/dotnet/api/system.boolean)<br>
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

`fileName` [String](https://learn.microsoft.com/dotnet/api/system.string)<br>

#### Returns

ISymbolReader

### <a id="methods-getsymbolreader"/>**GetSymbolReader(ModuleDefinition, Stream)**

```csharp
public ISymbolReader GetSymbolReader(ModuleDefinition module, Stream symbolStream)
```

#### Parameters

`module` ModuleDefinition<br>

`symbolStream` [Stream](https://learn.microsoft.com/dotnet/api/system.io.stream)<br>

#### Returns

ISymbolReader
