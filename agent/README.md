# Symbols Harvesting Agent

Service agent that picks up symbols from a given directory and uploads them to the configured server.

Check `appsettings.Development.json` for potential settings you need to customize before usage.

## Workflow

This project is meant to be run as a (Windows) service and watch over a given filesystem directory where the developer
can put copies of supported symbol files (currently `.exe`, `.dll`, `.sys` and `.pdb`) in, which then get uploaded to
the caching server.
Multiple upload servers watching over multiple upload directories are supported.
By default, the symbol files in the watched folder get deleted after a successful upload, if this is undesired, adjust
it in the configuration file.

### Watching subdirectories recursively

By ending your `WatcherPaths` entries with an asterisk `*` (e.g. `"D:\\temp\\pdb_uploads\\*"`) the entire subtree under
that path will get monitored for new symbols.
Recommended to use with `"DeleteAfterUpload": false` to not disturb your build process.

## Sources & 3rd party credits

- [MinVer](https://github.com/adamralph/minver)
- [MimeTypeMap](https://github.com/samuelneff/MimeTypeMap)
- [Serilog](https://github.com/serilog/serilog)
