# Symbols Harvesting Agent

Windows service that watches configured folders for new symbol files and uploads them to your caching proxy. Configuration is done through a **local web UI** (MudBlazor). The project targets **net10.0** so you can **build and run on Linux** for development; publish with the **win-x64** profile for Windows deployment.

## Web UI

- Bound to **127.0.0.1 only** (no authentication). Default port **5088** unless you change it in Settings.
- Open `http://127.0.0.1:5088/` after starting the agent.
- **Dashboard** — start/stop harvesting, upload counters, last error with a **Clear error** action, watcher list, and recent in-memory file activity history with **Clear history**. Activity includes detected, uploaded, failed, deleted, delete-skipped, and delete-failed outcomes to help spot unhealthy uploads quickly.
- **Configuration** — named upload servers (display name, URL, basic auth) and watcher paths. Each watched path has its own **Include subdirectories** checkbox, directory browser assisted selection, upload file filters (`*.exe`, `.dll`, …), delete-after-upload toggle, and deletion inclusion/exclusion glob rules.
- **Settings** — listen port (requires **service restart** to take effect).
- **Logs** — inspect the latest Serilog events captured since startup. Clearing this page only resets the in-memory view, not log files.

## Settings file

- **Windows (installed service):** `%ProgramData%\WinDbgSymbolsCachingProxy\HarvestingAgent\agent-settings.json` — machine-wide, writable without using `Program Files`.
- **Linux / macOS (local dev):** under your user data folder, e.g. `~/.local/share/WinDbgSymbolsCachingProxy/HarvestingAgent/agent-settings.json` on Linux (same `LocalApplicationData` layout as .NET on that OS).

On first run the file is created with defaults if it does not exist. The JSON contains credentials; keep machine access restricted as for any local admin tool.

## Workflow

The agent watches directories you configure. Supported upload patterns default to `.exe`, `.dll`, `.sys`, and `.pdb` unless you override them in the UI. Multiple named servers and multiple watcher paths per server are supported.

Enable **Include subdirectories** on a watcher row when you want the entire directory tree monitored. The built-in directory browser helps select watcher roots without manually typing full paths. Configure upload filters, **Delete after successful upload**, and deletion glob rules per watched path so build output and archival folders can use different behavior on the same server. Keeping deletion turned off for build trees is still recommended. Older settings files that used a trailing `*` on the path are still loaded and converted automatically.

When watching **live** build output (e.g. MSBuild writing to the same folder), the agent opens files with **shared** read access, waits for a **stable file size**, then **copies each symbol to a temp file** and uploads from that snapshot so the original path is not held open for the duration of the HTTP request. Files that are still being written are retried instead of being uploaded prematurely. That reduces contention with the toolchain.

Logging is still configured via `appsettings.json` / `appsettings.Development.json` (Serilog); harvester options are **not** read from those files.
Dashboard file activity history is in-memory only and resets when the process/service restarts.

## Running in Docker on Linux

Release tags publish the agent as `containinger/wdscp-harvesting-agent` on Docker Hub. The example below watches a host directory such as a build server's symbol output folder.

```bash
docker run -d --name wdscp-harvesting-agent --restart unless-stopped \
  -p 127.0.0.1:5088:5088 \
  -v wdscp-agent-data:/data \
  -v /srv/build/symbols:/symbols \
  containinger/wdscp-harvesting-agent:latest
```

1. Open `http://localhost:5088/` on the Docker host (use an SSH tunnel such as `ssh -L 5088:127.0.0.1:5088 host` for remote access; the UI has no authentication on Linux, so keep the `127.0.0.1:` prefix on the published port).
2. Under **Configuration**, add a server (the upload URL of your caching proxy plus credentials or API key) and a watcher path of `/symbols`. Enable **Include subdirectories** for nested build output; the default filters cover `*.exe`, `*.dll`, `*.sys` and `*.pdb`.
3. The configuration is stored in the `/data` volume and survives container upgrades. Container logs are available through `docker logs`.

Notes:

- `/symbols` must be readable by the container user (non-root, uid assigned by the image). Mount it read-only (`-v /srv/build/symbols:/symbols:ro`) when **Delete after upload** stays off; deleting uploaded files requires a writable mount and write permission for that user.
- The container must be able to reach the proxy URL; use a host name or IP address reachable from inside the container (not `localhost`).
- Watching uses inotify. Very large trees may need a higher `fs.inotify.max_user_watches` on the host, and network file systems (NFS/SMB) do not reliably deliver change events.
- Build the image yourself from the repository root with `docker build -f agent/Dockerfile -t wdscp-harvesting-agent .`.

Without Docker, run `dotnet run --project agent/HarvestingAgent.csproj` (UI on `127.0.0.1:5088`) and add the watch path in the UI, or edit `agent-settings.json` directly.

## Sources & 3rd-party credits

- [MinVer](https://github.com/adamralph/minver)
- [MimeTypeMapOfficial](https://github.com/samuelneff/MimeTypeMap)
- [MudBlazor](https://mudblazor.com/)
- [Serilog](https://github.com/serilog/serilog)
