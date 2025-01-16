# Symbols Harvesting Agent

Service agent that picks up symbols from a given directory and uploads them to the configured server.

Check `appsettings.Development.json` for potential settings you need to customize before usage.

## Workflow

This project is meant to be run as a (Windows) service and watch over a given filesystem directory where the developer
can put copies of supported symbol files (currently `.exe`, `.dll`, `.sys` and `.pdb`) in, which then get uploaded to
the caching server.
Multiple upload servers watching over multiple upload directories are supported.
