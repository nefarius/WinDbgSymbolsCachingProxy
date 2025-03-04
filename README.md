# <img src="assets/NSS-128x128.png" align="left" />WinDbgSymbolsCachingProxy

[![.NET](https://github.com/nefarius/WinDbgSymbolsCachingProxy/actions/workflows/build.yml/badge.svg)](https://github.com/nefarius/WinDbgSymbolsCachingProxy/actions/workflows/build.yml)
[![Docker Image CI](https://github.com/nefarius/WinDbgSymbolsCachingProxy/actions/workflows/docker-image.yml/badge.svg)](https://github.com/nefarius/WinDbgSymbolsCachingProxy/actions/workflows/docker-image.yml)
[![Nuget](https://img.shields.io/nuget/v/Nefarius.Utilities.ExceptionEnricher)](https://www.nuget.org/packages/Nefarius.Utilities.ExceptionEnricher/)
[![Nuget](https://img.shields.io/nuget/dt/Nefarius.Utilities.ExceptionEnricher)](https://www.nuget.org/packages/Nefarius.Utilities.ExceptionEnricher/)

Microsoft debugging symbols caching proxy using [FastEndpoints](https://fast-endpoints.com/)
and [MongoDB](https://mongodb-entities.com/).

## About

When working frequently
with [WinDbg](https://learn.microsoft.com/en-us/windows-hardware/drivers/debugger/debugging-using-windbg-preview) to
analyze kernel memory dumps, symbol (down)loading is a widespread occurrence. What's also quite common is the abysmal
performance of the official Microsoft Symbol Servers, taking easily **between two to three seconds** for every request!
Being fed up with wasting precious lifetime I made this little caching proxy you can put in between your WinDbg
instances and use it in conjunction with an offline caching directory. The proxy server will store a copy of the
requested symbol in a MongoDB database, including information if the symbol even exists upstream.
This information gets cached for a week, so you won't waste any more time twiddling thumbs while the Microsoft servers
take their sweet time to return a 404.

## Real-world example

Let's see if my efforts paid off, shall we! I've opened a kernel memory dump which requested **1090 files at a total
size of 186 Megabytes** with three different strategies outlined below. Let's go!

### No proxy, no local cache

**12 minutes, 17 seconds**. Abysmal and shameful! 😒

### With proxy, no local cache

**1 minute, 20 seconds**. Whoa, we're getting somewhere! 😃

### With proxy and local cache

**48 seconds**. Heck yeah! 🥳

## Public instance

![Cached Symbols Count](https://symbols.nefarius.at/api/badges/cachedSymbolsTotal) ![Cached Symbols 404](https://symbols.nefarius.at/api/badges/cachedSymbolsNotFound) ![Cached Symbols Found](https://symbols.nefarius.at/api/badges/cachedSymbolsFound)

I'm hosting an instance of this project at [`https://symbols.nefarius.at/`](https://symbols.nefarius.at/) which you can
consume and help grow by adjusting your symbol source settings like so:

```text
.sympath cache*D:\symbols;srv*https://symbols.nefarius.at/download/symbols
```

This means my local symbol cache resides at `D:\symbols`, adjust to your own preferences.

Like any other of my public web services, it only logs an absolute minimal amount required for debugging purposes, and
I'll never sell your data out to the big bois 😉

If you like this idea and want to keep my public instance happy, up and
running [consider making a donation](https://docs.nefarius.at/Community-Support/) 💸

## Features

- Caching! Pretty much the main purpose 😁
    - `/download/symbols/{Symbol}/{SignaturePlusAge}/{File}`
        - Serves the typical download request which the debugger issues.
- Badges! Embed server statistics anywhere via generated SVGs 🖼️
    - `/api/badges/cachedSymbolsTotal`
        - Returns the total number of cached records.
    - `/api/badges/cachedSymbolsFound`
        - Returns the number of records found upstream (these actually contain the data).
    - `/api/badges/cachedSymbolsNotFound`
        - Returns the number of records flagged as "not found" upstream.
- Add your own symbols!
    - `/api/uploads/symbol`
        - `POST` one or more supported symbol (currently `.exe`, `.dll`, `.sys` and `.pdb`) files using your own
          REST client or the provided [harvesting agent](./agent).

## Projects

- [`WinDbgSymbolsCachingProxy`](./server)  
  The symbols caching/hosting server backend that provides the REST API to the database.
- [`HarvestingAgent`](./agent)  
  A worker service that watches over filesystem directories for new symbols and uploads them to your servers.
- [`Nefarius.Utilities.ExceptionEnricher`](./lib)  
  A class library that makes use of the symbol server infrastructure to on-demand download debug symbols whenever an
  exception happens.

## Limitations

- Regarding non-native PDBs like those produced by .NET applications, currently only `full` (a.k.a. Windows-native)
  debug symbols are supported, since the `portable` can not be parsed server-side.

## How to publish

```PowerShell
dotnet publish -p:PublishProfile=Properties\PublishProfiles\release-win-x64.pubxml -c:Release .\WinDbgSymbolsCachingProxy.sln
```

<details><summary>Docker build</summary>

Use your own registry, of course 😉

```PowerShell
docker build -t nefarius.azurecr.io/wdscp:latest . ; docker push nefarius.azurecr.io/wdscp:latest
```

</details>

## Sources & 3rd party credits

This application benefits from these awesome projects ❤ (appearance in no special order):

### Web, Database

- [FastEndpoints](https://fast-endpoints.com/)
- [MongoDB Entities](https://mongodb-entities.com/)

### PE, PDB, etc. parsing

- [PDBSharp](https://github.com/smx-smx/PDBSharp)
- [PeNet](https://github.com/secana/PeNet)
- [Microsoft.SymbolStore](https://github.com/dotnet/symstore)
- [kaitai-pdb](https://github.com/smx-smx/kaitai-pdb)

### Badge generation

- [badger](https://github.com/8/badger)
- [SkiaSharp](https://github.com/mono/SkiaSharp)

### Misc.

- [Coravel](https://docs.coravel.net/)
- [Resourcer.Fody](https://github.com/Fody/Resourcer)
- [Nefarius.Utilities.AspNetCore](https://github.com/nefarius/Nefarius.Utilities.AspNetCore)
- [idunno.Authentication](https://github.com/blowdart/idunno.Authentication)
- [Fix Symbol loading for Kernel Driver minidump analysis](https://docs.nefarius.at/research/PDB-in-WinDbg-Session/)
- [The C5 Generic Collection Library for C# and CLI](https://www.itu.dk/research/c5/)
- [MinVer](https://github.com/adamralph/minver)
