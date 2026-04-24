# WinDbgSymbolsCachingProxy (server)

ASP.NET Core service that fronts the Microsoft symbol server (or another upstream), caches results in MongoDB, and
exposes a REST API plus a small [MudBlazor](https://mudblazor.com/) web UI. For motivation and WinDbg setup, see the
[repository README](../README.md).

## Stack

- [FastEndpoints](https://fast-endpoints.com/) with Swagger for HTTP APIs
- [Blazor Server](https://learn.microsoft.com/en-us/aspnet/core/blazor/) + MudBlazor for interactive pages
- [MongoDB.Entities](https://mongodb-entities.com/) for persistence
- [Basic authentication](https://github.com/blowdart/idunno.Authentication) for protected APIs and Blazor pages,
  credentials from `ServiceConfig.BasicAuthCredentials` in configuration
- [Coravel](https://docs.coravel.net/) for a scheduled job that rechecks upstream for previously not-found symbols

## Configuration

Edit [`appsettings.json`](appsettings.json) or environment-specific files such as
[`appsettings.Development.json`](appsettings.Development.json). The important `ServiceConfig` values are:

| Setting | Role |
|--------|------|
| `ConnectionString` / `DatabaseName` | MongoDB |
| `UpstreamUrl` | Symbol server to proxy (default Microsoft CDN) |
| `BadgeExpiresSeconds` | HTTP cache for SVG badge responses |
| `UpstreamRecheckPeriod` | How long 404 cache entries wait before a background recheck |
| `MemoryCacheSizeLimit` | In-memory cache cap (used for overview stats shared by `/info` and `/og/status.png`) |
| `BasicAuthCredentials` | Allowed username/password pairs for upload API and authorized Blazor routes |

## Run locally

Requires a reachable MongoDB instance matching your connection string.

```bash
dotnet run --project WinDbgSymbolsCachingProxy.csproj
```

From the repository root:

```bash
dotnet run --project server/WinDbgSymbolsCachingProxy.csproj
```

## HTTP routes (summary)

| Area | Path | Notes |
|------|------|--------|
| Symbol download | `/download/symbols/{Symbol}/{SymbolKey}/{FileName}` | WinDbg-compatible layout; anonymous |
| Badges | `/api/badges/{name}` | SVG metrics; `name` is `cachedSymbolsTotal`, `cachedSymbolsFound`, or `cachedSymbolsNotFound`; anonymous |
| Upload API | `/api/uploads/symbol` | Multipart upload; Basic auth |
| Overview JSON | `/info` | Version + counts; anonymous; one-hour cache |
| OG image | `/og/status.png` | PNG for link previews; anonymous; same cached counts as `/info` |
| UI | `/` | Redirects to `/status` |
| UI | `/status` | Public dashboard |
| UI | `/search`, `/upload`, `/logs` | Basic auth; `/logs` shows the in-memory Serilog event buffer |

Swagger / OpenAPI is enabled for the FastEndpoints routes (some internal endpoints are excluded from the document).

## Database migrations

Schema changes and one-off maintenance live under [`Migrations/`](Migrations/). See
[Migrations/README.md](Migrations/README.md) for how those scripts are organized and applied.
