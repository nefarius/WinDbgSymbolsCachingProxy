# EnrichedException

Namespace: Nefarius.Utilities.ExceptionEnricher

A custom-built exception with debug information resolved from other sources than local files.

```csharp
public class EnrichedException : System.Exception, System.Runtime.Serialization.ISerializable
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Exception](https://docs.microsoft.com/en-us/dotnet/api/system.exception) → [EnrichedException](./nefarius.utilities.exceptionenricher.enrichedexception.md)<br>
Implements [ISerializable](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.serialization.iserializable)

## Properties

### <a id="properties-baseexception"/>**BaseException**

The [Exception](https://docs.microsoft.com/en-us/dotnet/api/system.exception) object this [EnrichedException](./nefarius.utilities.exceptionenricher.enrichedexception.md) is based on.

```csharp
public Exception BaseException { get; }
```

#### Property Value

[Exception](https://docs.microsoft.com/en-us/dotnet/api/system.exception)<br>

### <a id="properties-data"/>**Data**

```csharp
public IDictionary Data { get; }
```

#### Property Value

[IDictionary](https://docs.microsoft.com/en-us/dotnet/api/system.collections.idictionary)<br>

### <a id="properties-helplink"/>**HelpLink**

```csharp
public string HelpLink { get; set; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### <a id="properties-hresult"/>**HResult**

```csharp
public int HResult { get; set; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### <a id="properties-innerexception"/>**InnerException**

```csharp
public Exception InnerException { get; }
```

#### Property Value

[Exception](https://docs.microsoft.com/en-us/dotnet/api/system.exception)<br>

### <a id="properties-message"/>**Message**

```csharp
public string Message { get; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### <a id="properties-source"/>**Source**

```csharp
public string Source { get; set; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### <a id="properties-stacktrace"/>**StackTrace**

```csharp
public string StackTrace { get; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### <a id="properties-targetsite"/>**TargetSite**

```csharp
public MethodBase TargetSite { get; }
```

#### Property Value

[MethodBase](https://docs.microsoft.com/en-us/dotnet/api/system.reflection.methodbase)<br>

## Methods

### <a id="methods-getbaseexception"/>**GetBaseException()**

```csharp
public Exception GetBaseException()
```

#### Returns

[Exception](https://docs.microsoft.com/en-us/dotnet/api/system.exception)

### <a id="methods-tostring"/>**ToString()**

```csharp
public string ToString()
```

#### Returns

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)
