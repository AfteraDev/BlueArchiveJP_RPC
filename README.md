# BlueArchiveJP_RPC

A lightweight, native C# rewrite of the original [Banana-Niku87729/BlueArchive-RPC](https://github.com/Banana-Niku87729/BlueArchive-RPC) (which was written in Python).

Compiles into a single `.exe` file (under 1 MB) and requires no Python installation or .NET runtime downloads — it targets the built-in .NET Framework 4.8, which ships with every Windows 10 / 11 installation out of the box.

## Features

- **Game detection**
  - First checks if `BlueArchive.exe` is currently running and reads the install path directly from the process.
  - Then scans all fixed drives for `YostarGames\BlueArchive_JP`.
- **File verification** — ensures game files are present before enabling RPC.
- **Tray-only interface** — no pop-up windows. Right-click the tray icon to:
  - Toggle Discord RPC.
  - Toggle autostart (writes to `HKCU\...\Run`).
  - Exit.

## Requirements

- Windows 10 or 11
- Discord Desktop Client running in the background
- .NET Framework 4.8 (pre-installed on Windows 10/11)

## Build

1. Clone the repository.
2. Make sure the [.NET SDK](https://dotnet.microsoft.com/download) is installed.
3. From the project folder run:

   ```bash
   dotnet build -c Release