# Symbols Harvesting Agent

Windows service that watches configured folders for new symbol files and uploads them to your caching proxy. Configuration is done through a **local web UI** (MudBlazor). The project targets **net9.0** so you can **build and run on Linux** for development; publish with the **win-x64** profile for Windows deployment.

## Web UI

- Bound to **127.0.0.1 only** (no authentication). Default port **5088** unless you change it in Settings.
- Open `http://127.0.0.1:5088/` after starting the agent.
- **Dashboard** — start/stop harvesting, upload counters, last error, watcher list.
- **Configuration** — upload servers (URL, basic auth), watcher paths (each with an **Include subdirectories** checkbox), upload file filters (`*.exe`, `*.dll`, …), delete-after-upload and deletion glob rules.
- **Settings** — listen port (requires **service restart** to take effect).

## Settings file

- **Windows (installed service):** `%ProgramData%\WinDbgSymbolsCachingProxy\HarvestingAgent\agent-settings.json` — machine-wide, writable without using `Program Files`.
- **Linux / macOS (local dev):** under your user data folder, e.g. `~/.local/share/WinDbgSymbolsCachingProxy/HarvestingAgent/agent-settings.json` on Linux (same `LocalApplicationData` layout as .NET on that OS).

On first run the file is created with defaults if it does not exist. The JSON contains credentials; keep machine access restricted as for any local admin tool.

## Workflow

The agent watches directories you configure. Supported upload patterns default to `.exe`, `.dll`, `.sys`, and `.pdb` unless you override them in the UI. Multiple servers and multiple watcher paths per server are supported.

Enable **Include subdirectories** on a watcher row when you want the entire directory tree monitored. That pairs well with **Delete after successful upload** turned off so builds are not disturbed. Older settings files that used a trailing `*` on the path are still loaded and converted automatically.

Logging is still configured via `appsettings.json` / `appsettings.Development.json` (Serilog); harvester options are **not** read from those files.

## Sources & 3rd-party credits

- [MinVer](https://github.com/adamralph/minver)
- [MimeTypeMap](https://github.com/samuelneff/MimeTypeMap)
- [MudBlazor](https://mudblazor.com/)
- [Serilog](https://github.com/serilog/serilog)
