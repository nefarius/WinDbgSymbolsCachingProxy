# ExceptionExtensions

Namespace: Nefarius.Utilities.ExceptionEnricher

Extension methods for [Exception](https://docs.microsoft.com/en-us/dotnet/api/system.exception) objects.

```csharp
public static class ExceptionExtensions
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ExceptionExtensions](./nefarius.utilities.exceptionenricher.exceptionextensions.md)

## Methods

### <a id="methods-toremotelyenrichedexception"/>**ToRemotelyEnrichedException(Exception, OnlineServerSymbolsResolver)**

Rebuilds the provided exception with debug information fetched from an online symbol server.

```csharp
public static EnrichedException ToRemotelyEnrichedException(Exception exception, OnlineServerSymbolsResolver provider)
```

#### Parameters

`exception` [Exception](https://docs.microsoft.com/en-us/dotnet/api/system.exception)<br>
The [Exception](https://docs.microsoft.com/en-us/dotnet/api/system.exception) object to enrich/rebuild.

`provider` [OnlineServerSymbolsResolver](./nefarius.utilities.exceptionenricher.onlineserversymbolsresolver.md)<br>
The [OnlineServerSymbolsResolver](./nefarius.utilities.exceptionenricher.onlineserversymbolsresolver.md) to be used for symbol lookup.

#### Returns

A new [EnrichedException](./nefarius.utilities.exceptionenricher.enrichedexception.md) object with debug information added.

### <a id="methods-toremotelyenrichedexception"/>**ToRemotelyEnrichedException(Exception, HttpClient)**

Rebuilds the provided exception with debug information fetched from an online symbol server.

```csharp
public static EnrichedException ToRemotelyEnrichedException(Exception exception, HttpClient httpClient)
```

#### Parameters

`exception` [Exception](https://docs.microsoft.com/en-us/dotnet/api/system.exception)<br>
The [Exception](https://docs.microsoft.com/en-us/dotnet/api/system.exception) object to enrich/rebuild.

`httpClient` HttpClient<br>
A  that specified the remote symbol server to contact.

#### Returns

A new [EnrichedException](./nefarius.utilities.exceptionenricher.enrichedexception.md) object with debug information added.
