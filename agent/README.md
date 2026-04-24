# Symbols Harvesting Agent

Windows service that watches configured folders for new symbol files and uploads them to your caching proxy. Configuration is done through a **local web UI** (MudBlazor). The project targets **net9.0** so you can **build and run on Linux** for development; publish with the **win-x64** profile for Windows deployment.

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

## Sources & 3rd-party credits

- [MinVer](https://github.com/adamralph/minver)
- [MimeTypeMapOfficial](https://github.com/samuelneff/MimeTypeMap)
- [MudBlazor](https://mudblazor.com/)
- [Serilog](https://github.com/serilog/serilog)
