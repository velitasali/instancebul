# Instancebul

> A wordplay on "instance" + "Istanbul" + "bulmak" (Turkish: to find)

A lightweight Windows utility that lets you instantly switch to any application with a custom keyboard shortcut. If the app isn't running, it launches it automatically.

## Features

- 🚀 **Instant App Switching** - Bring any app to focus with a single hotkey
- 🎯 **Smart Launch** - Automatically launches apps that aren't running
- 🪟 **Window Management** - Restores minimized windows and brings them to focus
- 🔄 **Live Reload** - Update config without restarting the app
- 🌟 **Start on Boot** - Optional auto-start with Windows
- 📦 **Self-Contained** - Single executable, no installation required
- 💡 **System Tray** - Runs quietly in the background
- 🔧 **UWP Support** - Works with both traditional Win32 apps and modern UWP apps

## Quick Start

1. Download `Instancebul.exe` from the releases
2. Run it - a notification will appear confirming it's running
3. Right-click the system tray icon and select "Open Config"
4. Edit your shortcuts and save
5. Right-click the tray icon and select "Reload Config"
6. Use your hotkeys!

## Default Configuration

The app comes pre-configured with these shortcuts:

| Hotkey | App | Description |
|--------|-----|-------------|
| `Alt+F9` | VS Code | Microsoft Visual Studio Code |
| `Alt+F10` | Chrome | Google Chrome browser |
| `Alt+F11` | Apple Music | Apple Music app |
| `Ctrl+Alt+T` | Windows Terminal | Modern Windows Terminal |

## Configuration

Edit `config.json` to customize your shortcuts:

```json
{
  "Shortcuts": [
    {
      "Hotkey": "Alt+F9",
      "ProcessName": "Code",
      "LaunchPath": "C:\\Program Files\\Microsoft VS Code\\Code.exe"
    },
    {
      "Hotkey": "Ctrl+Alt+T",
      "ProcessName": "WindowsTerminal",
      "LaunchPath": "finduwp:WindowsTerminal"
    },
    {
      "Hotkey": "Alt+N",
      "ProcessName": "notepad",
      "LaunchPath": "path:notepad"
    }
  ]
}
```

### Configuration Fields

- **Hotkey**: The keyboard shortcut (see supported keys below)
- **ProcessName**: Process name without `.exe` (check Task Manager > Details)
- **LaunchPath**: How to launch the app. Supports multiple formats:
  - **Auto-find UWP apps**: `finduwp:AppName` - automatically finds UWP apps
  - **Find in PATH**: `path:appname` - finds executables in PATH environment variable
  - **File paths**: `C:\\Program Files\\App\\app.exe`
  - **UWP apps manual**: `shell:AppsFolder\\PackageFamilyName!AppId`
  - **Environment variables**: `%PROGRAMFILES%\\App\\app.exe`
  - Use double backslashes `\\` in paths

### Supported Hotkey Format

**Modifiers** (at least one required):
- `Ctrl` or `Control`
- `Alt`
- `Shift`
- `Win` or `Windows`

**Keys**:
- Letters: `A`-`Z`
- Numbers: `0`-`9`
- Function keys: `F1`-`F24`
- Navigation: `Home`, `End`, `PageUp`, `PageDown`, `Insert`, `Delete`
- Arrows: `Up`, `Down`, `Left`, `Right`
- Special: `Space`, `Enter`, `Tab`, `Escape`, `Backspace`
- Media: `PrintScreen`, `ScrollLock`, `Pause`, `NumLock`, `CapsLock`
- Numpad: `Numpad0`-`Numpad9`, `Multiply`, `Add`, `Subtract`, `Decimal`, `Divide`
- Punctuation: `;`, `=`, `,`, `-`, `.`, `/`, `` ` ``, `[`, `\`, `]`, `'`

**Examples**:
- `Ctrl+Alt+V`
- `Win+Shift+T`
- `Alt+F9`
- `Ctrl+Shift+Home`

## System Tray Menu

Right-click the Instancebul icon in the system tray:

- **Open Config** - Opens `config.json` in your default editor
- **Reload Config** - Reloads shortcuts without restarting the app
- **Start on Boot** ☐ - Toggle auto-start with Windows
- **Exit** - Closes the app

Double-click the tray icon to quickly open the config file.

## Start on Boot

Enable auto-start from the system tray:
1. Right-click the Instancebul tray icon
2. Click "Start on Boot"
3. A notification will confirm it's enabled

This adds Instancebul to Windows startup registry. You can disable it anytime from the same menu.

## Finding Process Names

To find the correct process name for an app:

1. Open Task Manager (`Ctrl+Shift+Esc`)
2. Go to the **Details** tab
3. Find your application
4. Look at the **Name** column (without `.exe`)

### Common Process Names

| Application | ProcessName |
|------------|-------------|
| VS Code | `Code` |
| Visual Studio | `devenv` |
| Chrome | `chrome` |
| Firefox | `firefox` |
| Edge | `msedge` |
| Windows Terminal | `WindowsTerminal` |
| Notepad++ | `notepad++` |
| Spotify | `Spotify` |
| Discord | `Discord` |
| Slack | `slack` |
| Apple Music | `AppleMusic` |
| Notion | `Notion` |
| Obsidian | `Obsidian` |

## UWP/Store Apps

Instancebul has **automatic UWP app discovery** - just use `finduwp:AppName` and it finds the app for you!

### Method 1: Auto-Discovery (Recommended) ✨

Simply use `finduwp:` followed by the app name:

```json
{
  "Shortcuts": [
    {
      "Hotkey": "Ctrl+Alt+T",
      "ProcessName": "WindowsTerminal",
      "LaunchPath": "finduwp:WindowsTerminal"
    },
    {
      "Hotkey": "Alt+F11",
      "ProcessName": "AppleMusic",
      "LaunchPath": "finduwp:AppleMusic"
    },
    {
      "Hotkey": "Alt+S",
      "ProcessName": "Spotify",
      "LaunchPath": "finduwp:Spotify"
    }
  ]
}
```

**How it works:**
- Automatically finds the UWP app's executable path
- First launch: Searches installed apps and caches the result
- Subsequent launches: Uses cached path (instant)
- Works with both new and updated app versions

**Tips:**
- Use the package name or part of it (e.g., `finduwp:Terminal`, `finduwp:Apple`)
- Case insensitive
- Searches Microsoft Store apps only

### Common UWP Apps with finduwp:

Just use these simple formats - no need to find complex paths!

| App | LaunchPath |
|-----|-----------|
| Windows Terminal | `finduwp:WindowsTerminal` |
| Apple Music | `finduwp:AppleMusic` |
| Spotify | `finduwp:Spotify` |
| Windows Photos | `finduwp:Photos` |
| Calculator | `finduwp:Calculator` |
| Microsoft To Do | `finduwp:Todo` |
| WhatsApp | `finduwp:WhatsApp` |

**Troubleshooting:**
- If `finduwp:AppName` doesn't work, try a more specific name (e.g., `finduwp:MicrosoftTodo`)
- Check the app is installed from Microsoft Store
- Restart Instancebul after installing new UWP apps

### Note on UWP App Updates

When UWP apps update, their version numbers change in the installation path. The `finduwp:` feature handles this automatically by always finding the current executable location, so you never need to update your config when apps are updated.

## Apps in PATH Environment Variable

For command-line tools and apps in your PATH, use the `path:` syntax:

```json
{
  "Shortcuts": [
    {
      "Hotkey": "Alt+N",
      "ProcessName": "notepad",
      "LaunchPath": "path:notepad"
    },
    {
      "Hotkey": "Alt+P",
      "ProcessName": "python",
      "LaunchPath": "path:python"
    },
    {
      "Hotkey": "Ctrl+Alt+G",
      "ProcessName": "git",
      "LaunchPath": "path:git"
    }
  ]
}
```

**How it works:**
- Searches all directories in your PATH environment variable
- Automatically adds `.exe` extension if not present
- Shows an error if the app isn't found in PATH

**Common use cases:**
- Command-line tools (`git`, `python`, `node`, `npm`)
- System utilities (`notepad`, `calc`, `mspaint`)
- Development tools in PATH

**Example:**
Instead of:
```json
"LaunchPath": "C:\\Windows\\System32\\notepad.exe"
```

Use:
```json
"LaunchPath": "path:notepad"
```

## Building from Source

### Requirements
- Windows 10/11
- .NET 8 SDK

### Quick Build

Use the included build script to create both versions at once:

```bash
build.bat
```

This creates both the small (169KB) and self-contained (147MB) versions.

### Manual Build Options

There are two build types available:

#### Option 1: Small Build (Recommended - 169KB)
**Framework-dependent** - Requires .NET 8 runtime on target machine

```bash
dotnet publish -c Release --no-self-contained -o bin/Release/net8.0-windows/publish-small
```

Output: `bin/Release/net8.0-windows/publish-small/Instancebul.exe` (~169KB)

**Pros**: Tiny executable size
**Cons**: Requires [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) installed

#### Option 2: Self-Contained Build (147MB)
**Self-contained** - No runtime needed, works on any Windows machine

```bash
dotnet publish -c Release
```

Output: `bin/Release/net8.0-windows/win-x64/publish/Instancebul.exe` (~147MB)

**Pros**: Works without any dependencies
**Cons**: Large file size (includes entire .NET runtime)

### Which Build Should You Use?

- **Use the small build (169KB)** if you have .NET 8 runtime installed or don't mind installing it
- **Use the self-contained build (147MB)** if you want a portable version that works anywhere

Most modern Windows machines either have .NET runtime or can easily install it, so the small build is recommended.

## Troubleshooting

### "Failed to register hotkey"
The keyboard shortcut is already in use by another application or Windows. Try a different combination. Avoid common Windows shortcuts like:
- `Win+L` (Lock screen)
- `Ctrl+Alt+Delete` (Security screen)
- `Win+D` (Show desktop)

### App doesn't come to focus
- Verify the process name is correct (check Task Manager → Details)
- Some apps may use multiple processes - you may need to try different process names
- Try launching the app manually first, then use the hotkey

### Hotkey not working at all
- Make sure Instancebul is running (check system tray)
- Right-click tray icon → "Reload Config" to refresh
- Check for error messages in the notification area
- Ensure the hotkey format is correct (e.g., `Ctrl+Alt+V`, not `Ctrl-Alt-V`)

### UWP app path stops working
If you're using full file paths for UWP apps, they include version numbers that change with updates.

**Solution**: Use `finduwp:` instead, which never breaks:
1. Update your config.json: `"LaunchPath": "finduwp:AppName"`
2. Right-click tray icon → "Reload Config"

Example: Change from `C:\Program Files\WindowsApps\...` to `finduwp:Terminal`

See the [UWP/Store Apps](#uwpstore-apps) section for details.

### Application launches but doesn't come to focus
This can happen with some apps that handle their windows specially. The app works best with standard Win32 applications.

## How It Works

1. **Hotkey Registration**: Uses Windows `RegisterHotKey` API to register global keyboard shortcuts
2. **Window Detection**: Finds windows by process name using the Win32 API
3. **Focus Management**: Brings windows to focus using thread input attachment
4. **Smart Launch**: If no window is found, launches the app using the configured path
5. **Background Operation**: Runs as a system tray application with minimal resource usage

## Requirements

- Windows 10 or Windows 11
- No .NET runtime needed (self-contained build)
- ~60KB memory usage when idle

## License

MIT License - Feel free to use, modify, and distribute.
