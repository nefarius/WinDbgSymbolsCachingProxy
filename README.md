<img src="assets/NSS-128x128.png" align="right" />

# WinDbgSymbolsCachingProxy

![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/nefarius/WinDbgSymbolsCachingProxy/docker-image.yml)

Microsoft debugging symbols caching proxy using [FastEndpoints](https://fast-endpoints.com/) and [MongoDB](https://mongodb-entities.com/).

## About

When working frequently with [WinDbg](https://learn.microsoft.com/en-us/windows-hardware/drivers/debugger/debugging-using-windbg-preview) to analyze kernel memory dumps, symbol (down)loading is a very common occurrence. What's also quite common is the abysmal performance of the official Microsoft Symbol Servers, taking easily between **two to three seconds** for every request! Being fed up with wasting precious lifetime I made this little caching proxy you can put in between your WinDbg instances and use it in conjunction with an offline caching directory. The proxy server will store a copy of the requested symbol in a MongoDB database, including information if the symbol even exists upstream. This information gets cached for a day, so you won't waste any more time twiddling thumbs while the Microsoft servers take their sweet time to return a 404.

## Real-world example

Let's see if my efforts paid off, shall we! I've opened a kernel memory dump which requested **1090 files at a total size of 186 Megabytes** with three different strategies outlined below. Let's go!

### No proxy, no local cache

**12 minutes, 17 seconds**. Abysmal and shameful! 😒

### With proxy, no local cache

**1 minute, 20 seconds**.  Whoa, we're getting somewhere! 😃

### With proxy and local cache

**48 seconds**. Heck yeah! 🥳

## Public instance

I'm hosting an instance of this project at [`https://symbols.nefarius.at/`](https://symbols.nefarius.at/) which you can consume and help growing by adjusting your symbol source settings like so:

```text
.sympath cache*D:\symbols;srv*https://symbols.nefarius.at/download/symbols
```

This means my local symbol cache resides at `D:\symbols`, adjust to your own preferences.

Like any other of my public web services it only logs an absolute minimal amount required for debugging purposes and I'll never sell your data out to the big bois 😉

## TODOs

- Add support to provide a custom directory where the server can pick up custom/private symbols from and cache them with the same methods.
- Make retry and cache invalidation parameters configurable e.g. via `appsettings.json`
- Maybe add custom "admin" endpoints to upload custom symbols, delete existing entries and invalidate cache on command?
- Add timer to periodically check if 404ed symbols are now available
- Add timer to automate some tasks, maybe log last symbol access and scrub some that never got requested after a few months or so

## 3rd party credits

- [FastEndpoints](https://fast-endpoints.com/)
- [MongoDB Entities](https://mongodb-entities.com/)
- [Nefarius.Utilities.AspNetCore](https://github.com/nefarius/Nefarius.Utilities.AspNetCore)
- [PeNet](https://github.com/secana/PeNet)
- [badger](https://github.com/8/badger)
- [SkiaSharp](https://github.com/mono/SkiaSharp)
