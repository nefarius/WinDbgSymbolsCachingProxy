<img src="assets/NSS-128x128.png" align="right" />

# WinDbgSymbolsCachingProxy

TBD

## Real-world example

Let's see if my efforts paid off, shall we! I've opened a kernel memory dump which requested **1090 files at a total size of 186 Megabytes** with three different strategies outlined below. Let's go!

### No proxy, no local cache

**12 minutes, 17 seconds**. Abysmal and shameful! 😒

### With proxy, no local cache

**1 minute, 20 seconds**.  Woha, we're getting somewhere! 😃

### With proxy and local cache

**48 seconds**. Heck yeah! 🥳
