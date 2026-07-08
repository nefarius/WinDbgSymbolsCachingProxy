# WinDbgSymbolsCachingProxy (server)

ASP.NET Core service that fronts the Microsoft symbol server (or another upstream), caches results in MongoDB, and
exposes a REST API plus a small [MudBlazor](https://mudblazor.com/) web UI. For motivation and WinDbg setup, see the
[repository README](../README.md).

## Stack

- [FastEndpoints](https://fast-endpoints.com/) with Swagger for HTTP APIs
- [Blazor Server](https://learn.microsoft.com/en-us/aspnet/core/blazor/) + MudBlazor for interactive pages
- [MongoDB.Entities](https://mongodb-entities.com/) for persistence
- **Optional OpenID Connect** (`Microsoft.AspNetCore.Authentication.OpenIdConnect`) for browser-based login — when
  configured, replaces Basic auth and secures all routes by default
- **In-app Roles & Permissions** stored in MongoDB — granular access control for symbols download/upload/delete, log
  viewing, and administration
- **API Keys** for non-interactive clients (harvesting agents, WinDbg URL parameters) when OIDC is active
- [Basic authentication](https://github.com/blowdart/idunno.Authentication) for protected APIs and Blazor pages when
  OIDC is **not** configured, credentials from `ServiceConfig.BasicAuthCredentials` in configuration
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
| `NotFoundCleanup.Enabled` | `false` by default; set to `true` to enable automatic pruning of stale 404 placeholders |
| `NotFoundCleanup.InactiveAfter` | Placeholder that was requested at least once is deleted when `LastAccessedAt` is older than this threshold (default `180.00:00:00` — 180 days) |
| `NotFoundCleanup.UnusedNotFoundAfter` | Placeholder that was **never** requested by any client is deleted when `NotFoundAt` is older than this threshold (default `180.00:00:00` — 180 days) |

## Run locally

Requires a reachable MongoDB instance matching your connection string.

```bash
dotnet run --project WinDbgSymbolsCachingProxy.csproj
```

From the repository root:

```bash
dotnet run --project server/WinDbgSymbolsCachingProxy.csproj
```

## Authentication modes

### Basic auth mode (default)

When no OIDC configuration exists in MongoDB (or `Enabled` is `false`), the server behaves exactly as before:

- Symbol download is **anonymous**.
- Upload and admin Blazor pages require HTTP Basic auth via `ServiceConfig.BasicAuthCredentials`.

### OIDC mode

Set `Enabled = true` on the `OidcConfigEntity` document in the `oidc-config` MongoDB collection (easiest via the
**Administration → OIDC Settings** page while still in Basic-auth mode), then **restart the service**.

When active:

- **Cookie + OpenID Connect** handles interactive browser login at `/account/login`.
- **API keys** (`X-Api-Key` header or `?apiKey=` query parameter) handle non-interactive clients.
- All routes require the appropriate permission unless listed in `AnonymousPathAllowlist`.

#### Permissions and Roles

| Permission | Required for |
|------------|-------------|
| `symbols.download` | `GET /download/symbols/…` |
| `symbols.upload` | `POST /api/uploads/symbol`, `/upload` page |
| `symbols.delete` | `/search` page (delete action) |
| `logs.view` | `/logs` page |
| `admin` | Administration pages |

Default seeded roles: **Admin** (all), **Uploader** (download + upload), **Reader** (download only).

#### First-admin bootstrap

1. While the server runs in Basic-auth mode, go to **Administration → OIDC Settings**.
2. Fill in Authority, Client ID/Secret, and add your OIDC subject or e-mail to **Bootstrap admin identifiers**.
3. Save, then restart the service.
4. Log in with your identity provider — the bootstrap rule grants your account the Admin role automatically.
5. Use **Administration → Users** to assign roles to other users, then clear the bootstrap list.

#### API keys for non-interactive clients

Create keys in **Administration → API Keys**. Use them:

- In the harvesting agent: set `ApiKey` in the `ServerConfig` — the agent prefers it over Basic credentials.
- In WinDbg symbol path: `srv*c:\symbols*http://host/download/symbols?apiKey=<key>` (URL query parameter).
- In HTTP requests: `X-Api-Key: <key>` header.

## HTTP routes (summary)

| Area | Path | Notes |
|------|------|--------|
| Symbol download | `/download/symbols/{Symbol}/{SymbolKey}/{FileName}` | `symbols.download` permission; anonymous in Basic-auth mode |
| Badges | `/api/badges/{name}` | SVG metrics; anonymous |
| Upload API | `/api/uploads/symbol` | `symbols.upload` permission; Cookie or API key |
| Overview JSON | `/info` | Version + counts; anonymous; one-hour cache |
| OG image | `/og/status.png` | PNG for link previews; anonymous |
| UI | `/` | Redirects to `/status` |
| UI | `/status` | Public dashboard; anonymous |
| UI | `/search` | `symbols.delete` permission |
| UI | `/upload` | `symbols.upload` permission |
| UI | `/logs` | `logs.view` permission |
| UI | `/admin/oidc`, `/admin/users`, `/admin/roles`, `/admin/api-keys` | `admin` permission |
| Account | `/account/login`, `/account/logout` | OIDC challenge / sign-out (no-op in Basic-auth mode) |

Swagger / OpenAPI is enabled for the FastEndpoints routes (some internal endpoints are excluded from the document).

## Database migrations

Schema changes and one-off maintenance live under [`Migrations/`](Migrations/). See
[Migrations/README.md](Migrations/README.md) for how those scripts are organized and applied.
