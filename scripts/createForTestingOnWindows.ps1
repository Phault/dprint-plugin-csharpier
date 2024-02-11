# Script for quickly creating the plugin for testing purposes on Windows
# To run:
# 1. Run `./scripts/createForTestingOnWindows.ps1`
# 2. Update dprint.json to point at ./plugin.json then update checksum
#    as shown when initially run.

$ErrorActionPreference = "Stop"

dotnet build DprintPluginCsharpier -c Release --runtime win-x64
Compress-Archive -Force -Path DprintPluginCsharpier/bin/Release/net8.0/win-x64/* -DestinationPath dprint-plugin-csharpier-x86_64-pc-windows-msvc.zip
deno run --allow-read=. --allow-write=. ./scripts/create_plugin_file.ts --test
