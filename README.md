<img src="assets/NSS-128x128.png" align="right" />

# WinDbgSymbolsCachingProxy

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

## TODOs

- Add support to provide a custom directory where the server can pick up custom/private symbols from and cache them with the same methods.
- Make retry and cache invalidation parameters configurable e.g. via `appsettings.json`
- Maybe add custom "admin" endpoints to upload custom symbols, delete existing entries and invalidate cache on command?

## 3rd party credits

- [FastEndpoints](https://fast-endpoints.com/)
- [MongoDB Entities](https://mongodb-entities.com/)
- [Nefarius.Utilities.AspNetCore](https://github.com/nefarius/Nefarius.Utilities.AspNetCore)
