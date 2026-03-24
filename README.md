# <img src="assets/NSS-128x128.png" align="left" />WinDbgSymbolsCachingProxy

[![.NET](https://github.com/nefarius/WinDbgSymbolsCachingProxy/actions/workflows/build.yml/badge.svg)](https://github.com/nefarius/WinDbgSymbolsCachingProxy/actions/workflows/build.yml)
[![Docker Image CI](https://github.com/nefarius/WinDbgSymbolsCachingProxy/actions/workflows/docker-image.yml/badge.svg)](https://github.com/nefarius/WinDbgSymbolsCachingProxy/actions/workflows/docker-image.yml)
[![Nuget](https://img.shields.io/nuget/v/Nefarius.Utilities.ExceptionEnricher)](https://www.nuget.org/packages/Nefarius.Utilities.ExceptionEnricher/)
[![Nuget](https://img.shields.io/nuget/dt/Nefarius.Utilities.ExceptionEnricher)](https://www.nuget.org/packages/Nefarius.Utilities.ExceptionEnricher/)
[![Assisted by Cursor AI](https://img.shields.io/badge/Assisted%20by-Cursor%20AI-8B5CF6?style=flat)](https://cursor.com/)

Microsoft debugging symbols caching proxy using [FastEndpoints](https://fast-endpoints.com/)
and [MongoDB](https://mongodb-entities.com/).

## About

When working frequently
with [WinDbg](https://learn.microsoft.com/en-us/windows-hardware/drivers/debugger/debugging-using-windbg-preview) to
analyze kernel memory dumps, symbol (down)loading is a widespread occurrence. What's also quite common is the abysmal
performance of the official Microsoft Symbol Servers, taking easily **between two and three seconds** for every request!
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
use and help grow. Opening that URL in a browser sends you straight to the **status** dashboard; for WinDbg, point your
symbol source at the download path below:

```text
.sympath cache*D:\symbols;srv*https://symbols.nefarius.at/download/symbols
```

This means my local symbol cache resides at `D:\symbols`, adjust to your own preferences.

Like any other of my public web services, it only logs the absolute minimum required for debugging purposes, and
I'll never sell your data out to the big bois 😉

If you like this idea and want to keep my public instance happy and running,
[consider making a donation](https://docs.nefarius.at/Community-Support/) 💸

## Features

- Caching! Pretty much the main purpose 😁
    - `/download/symbols/{Symbol}/{SymbolKey}/{FileName}`
        - Serves the typical download request which the debugger issues.
- Web UI ([MudBlazor](https://mudblazor.com/)) 🖥️
    - `/` redirects to `/status`.
    - `/status` — public overview: server version and cached symbol counts (total, found upstream, not-found upstream).
    - `/search` — browse stored symbols in a data grid (Basic authentication, same credentials as the upload API).
    - `/upload` — upload symbols in the browser (multi-file, drag-and-drop; same extensions as the API; Basic auth).
    - App shell with a navigation drawer and light/dark theme. Search and Upload use a full page load so the browser
      can attach Basic credentials when you open those routes.
- Status and metrics (for dashboards, bots, and link previews) 📊
    - `/info` — JSON with `serverVersion`, `cachedSymbolsTotal`, `cachedSymbolsFound`, `cachedSymbols404`, and
      `projectUrl` (same figures as the status page, with a short in-memory cache).
    - `/og/status.png` — PNG preview image for Open Graph / Twitter cards when sharing `/status` (Discord, Slack, etc.).
- Badges! Embed server statistics anywhere via generated SVGs 🖼️
    - `/api/badges/cachedSymbolsTotal`
        - Returns the total number of cached records.
    - `/api/badges/cachedSymbolsFound`
        - Returns the number of records found upstream (these actually contain the data).
    - `/api/badges/cachedSymbolsNotFound`
        - Returns the number of records flagged as "not found" upstream.
- Add your own symbols 🗂️
    - `/api/uploads/symbol`
        - `POST` one or more supported symbol (currently `.exe`, `.dll`, `.sys` and `.pdb`) files using your own
          REST client or the provided [harvesting agent](./agent).

## Projects

- [`WinDbgSymbolsCachingProxy`](./server)  
  The symbol caching/hosting server backend that provides the REST API, Blazor UI, and MongoDB integration. See
  [server/README.md](./server/README.md) for a concise endpoint and setup summary.
- [`HarvestingAgent`](./agent)  
  A worker service that watches over filesystem directories for new symbols and uploads them to your servers.
- [`Nefarius.Utilities.ExceptionEnricher`](./lib)  
  A class library that makes use of the symbol server infrastructure to on-demand download debug symbols whenever an
  exception happens.

## Limitations

- Regarding non-native PDBs like those produced by .NET applications, currently only `full` (a.k.a. Windows-native)
  debug symbols are supported, since `portable` symbols cannot be parsed server-side.

## How to publish

```PowerShell
dotnet publish -p:PublishProfile=Properties\PublishProfiles\release-win-x64.pubxml -c:Release .\WinDbgSymbolsCachingProxy.sln
```

<details><summary>Docker build</summary>

Use your own registry, of course 😉

```PowerShell
docker build --push -t nefarius.azurecr.io/wdscp:latest .
```

The GitHub Actions Docker workflow uses [Buildx](https://docs.docker.com/build/buildx/) with a GitHub Actions cache
backend so repeated CI builds stay faster.

</details>

## Sources & 3rd party credits

This application benefits from these awesome projects ❤ (appearance in no special order):

### Web, Database

- [FastEndpoints](https://fast-endpoints.com/)
- [MongoDB Entities](https://mongodb-entities.com/)
- [MudBlazor](https://mudblazor.com/)

### PE, PDB, etc. parsing

- [PDBSharp](https://github.com/smx-smx/PDBSharp)
- [PeNet](https://github.com/secana/PeNet)
- [Microsoft.SymbolStore](https://github.com/dotnet/symstore)
- [kaitai-pdb](https://github.com/smx-smx/kaitai-pdb)
- [Nefarius.Shared.PdbUtils](https://github.com/nefarius/PdbUtils)

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
