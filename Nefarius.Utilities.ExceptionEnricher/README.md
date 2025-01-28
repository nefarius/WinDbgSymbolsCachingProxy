# Nefarius.Utilities.ExceptionEnricher

![Requirements](https://img.shields.io/badge/Requires-.NET%20%3E%3D8.0-blue.svg)
[![Nuget](https://img.shields.io/nuget/v/Nefarius.Utilities.ExceptionEnricher)](https://www.nuget.org/packages/Nefarius.Utilities.ExceptionEnricher/)
[![Nuget](https://img.shields.io/nuget/dt/Nefarius.Utilities.ExceptionEnricher)](https://www.nuget.org/packages/Nefarius.Utilities.ExceptionEnricher/)

## Motivation

Having stack traces on exceptions including line numbers and the source file name of the offending code is a luxury
we're accustomed to in local debug builds, not so much in the field though.

Getting readable exceptions requires debug symbols being present at the time the exception/stack trace is
thrown/generated, so you'd typically need to include your `.pdb` files with your published app, at the cost of increased
size of the deliverables.

This library tries to tackle this conundrum by on-demand downloading the required symbols from a symbol server (which
you can host yourself) whenever an "enriched" exception is thrown.
You can then send this enriched exception to your logging or tracing subsystem and enjoy the comfort of readable stack
traces!
Symbols are held in memory until application termination to reduce the required http calls to a minimum.

## Limitations

### `<No sequence point found, async method?>`

Currently, file names and line numbers cannot be looked up correctly for asynchronous methods
through [the use of Mono.Cecil](https://github.com/jbevain/cecil/issues/805) alone, so you will get this "error"
instead.

## Example

Here we have a classic `ToString()` on an exception object in a production release with no symbols:

```text
System.InvalidOperationException: Test exception
   at Program.<<Main>$>g__Baz|0_2()
   at Program.<<Main>$>g__Bar|0_1(List`1 someArgs)
   at Program.<<Main>$>g__Foo|0_0(Int32 firstArg, Int32 secondArg)
   at Program.<Main>$(String[] args)
```

If we enrich the same exception by on-demand downloading remote symbols, we get something way more helpful:

```text
System.InvalidOperationException: Test exception
   at Program.<<Main>$>g__Baz|0_2() in D:\Development\GitHub\WinDbgSymbolsCachingProxy\testapp\Program.cs (line 47)
   at Program.<<Main>$>g__Bar|0_1(List<Int32> someArgs) in D:\Development\GitHub\WinDbgSymbolsCachingProxy\testapp\Program.cs (line 42)
   at Program.<<Main>$>g__Foo|0_0(Int32 firstArg, Int32 secondArg) in D:\Development\GitHub\WinDbgSymbolsCachingProxy\testapp\Program.cs (line 36)
   at Program.<Main>$(String[] args) in D:\Development\GitHub\WinDbgSymbolsCachingProxy\testapp\Program.cs (line 10)
```

## Setup

- Wherever you catch and process your apps' fatal exceptions use `ex.ToRemotelyEnrichedException(client)` instead of
  `ex` directly
- Add or adjust the debug type for your release build in your `.csproj` like so:
    ```xml
        <PropertyGroup Condition=" '$(Configuration)' == 'Release' ">
          <DebugType>full</DebugType>
        </PropertyGroup>
    ```
- Use the [harvesting agent](../agent) (or whatever method you prefer) to collect the resulting `.pdb` files and upload
  them to your symbol server instance
- Package your app for your users **without** the `.pdb` files included
- Now whenever an exception is thrown (and the user has an active Internet connection, and you configure everything
  right), you will get proper useful stack traces when you send the exception details to your logging framework 💪

## Documentation

[Link to API docs](../docs/index.md).

## Sources & 3rd party credits

- [Microsoft.Diagnostics.Runtime](https://github.com/microsoft/clrmd)
- [Mono.Cecil](https://www.mono-project.com/docs/tools+libraries/libraries/Mono.Cecil/)
- [FastDeepCloner](https://github.com/AlenToma/FastDeepCloner)
