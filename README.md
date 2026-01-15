# PDFs Manager

Automatic PDF Organization Tool for Windows

PDFs Manager is a Windows desktop application that automatically organizes PDF files by their creation date. It monitors a workspace folder and moves files into structured year/month directories with standardized filenames.

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/platform-Windows-0078D4)](https://www.microsoft.com/windows)

## Features

✨ **Automatic File Organization**

- Renames PDFs to `YYYY-MM-DD_HH-mm-ss.pdf` based on creation date metadata
- Organizes into `YYYY/MM/` folder structure
- Handles duplicates with sequential suffixes (`_001`, `_002`, etc.)

🔍 **Smart Monitoring**

- Real-time file detection with FileSystemWatcher
- Fallback 5-minute periodic scan for reliability
- File lock retry mechanism (3 attempts, 2s delays)

🎨 **Modern UI**

- Clean WinForms interface with real-time log display
- JSON-based localization with fallback support
- JSON-based theming system (light theme included)
- Color-coded status indicators

⚙️ **Configuration Management**

- Persistent workspace configuration
- Optional auto-startup with Windows
- Dirty config tracking with explicit save/cancel

📝 **Comprehensive Logging**

- Timestamped action logs with detailed operation tracking
- Thread-safe logging from background operations
- Real-time GUI updates via callback mechanism

## Screenshots

```plaintext
┌─────────────────────────────────────────────┐
│ PDFs Manager                       [−][□][×]│
├─────────────────────────────────────────────┤
│ ┌─ Activity Log ───────────────────────────┐│
│ │ [2026-01-15 16:30:05] STRT: Starting...  ││
│ │ [2026-01-15 16:30:06] FILE: Processing   ││
│ │ [2026-01-15 16:30:07] DONE: Success      ││
│ │                                          ││
│ └──────────────────────────────────────────┘│
│ [Refresh] [Clear]                           │
│                                             │
│ ┌─ Configuration ──────────────────────────┐│
│ │ Workspace: D:\Documents\PDFs             ││
│ │ [Browse...]                              ││
│ │ [●] Auto Startup with Windows            ││
│ └──────────────────────────────────────────┘│
│                                             │
│ [Start] [Help]               [Exit] [Cancel]│
│ Status: ● Running                           │
└─────────────────────────────────────────────┘
```

## Quick Start

### Prerequisites

- Windows 10/11
- .NET 8.0 Runtime ([Download](https://dotnet.microsoft.com/download/dotnet/8.0))

### Installation

1. **Download** the latest release from [Releases](https://github.com/1172005thinh/PDFsManager/releases)

2. **Extract** to a folder (e.g., `C:\Program Files\PDFsManager\`)

3. **Run** `PDFsManager.exe`

### Usage

1. **Set Workspace**: Click `Browse` and select a folder containing PDF files
2. **Start Monitoring**: Click `Start` to begin automatic processing
3. **Monitor Logs**: Watch the activity log for real-time processing updates
4. **Stop Monitoring**: Click `Stop` when done

### Example

**Before**:

```plaintext
Workspace/
├─ invoice.pdf
├─ receipt_scan.pdf
└─ document.pdf
```

**After Processing**:

```plaintext
Workspace/
├─ 2026/
│  ├─ 01/
│  │  ├─ 2026-01-15_14-30-05.pdf  (was invoice.pdf)
│  │  └─ 2026-01-15_14-30-06.pdf  (was receipt_scan.pdf)
│  └─ 02/
│     └─ 2026-02-10_09-15-22.pdf  (was document.pdf)
```

## Building from Source

### Requirements

- Visual Studio 2022 or VS Code with C# extension
- .NET 8.0 SDK ([Download](https://dotnet.microsoft.com/download/dotnet/8.0))

### Steps

```powershell
# Clone repository
git clone https://github.com/1172005thinh/PDFsManager.git
cd PDFsManager

# Restore dependencies
dotnet restore

# Build project
dotnet build

# Run application
dotnet run

# Or build Release
dotnet build -c Release
```

### VS Code Tasks

- `Ctrl+Shift+B` → Build
- `F5` → Debug
- Task `build-release` → Create optimized Release build

## Project Structure

```plaintext
PDFsManager/
├─ src/
│  ├─ Core/              # Business logic layer
│  │  ├─ Logger.cs           Thread-safe logging
│  │  ├─ ConfigManager.cs    JSON config persistence
│  │  ├─ FileProcessor.cs    PDF processing pipeline
│  │  └─ FileMonitor.cs      FileSystemWatcher + periodic scan
│  ├─ Models/            # Data structures
│  │  ├─ AppConfig.cs        Configuration POCO
│  │  ├─ AppState.cs         Application state enum
│  │  └─ ProcessResult.cs    Operation result object
│  ├─ Utils/             # Helper utilities
│  │  ├─ Constants.cs        Application-wide constants
│  │  ├─ FileHelper.cs       File operations + lock handling
│  │  ├─ PdfHelper.cs        PDF metadata extraction (iText7)
│  │  ├─ LocalizationHelper.cs   Language string loader
│  │  └─ ThemeHelper.cs      Theme value loader
│  ├─ UI/                # WinForms GUI layer
│  │  ├─ MainForm.cs         Main application window
│  │  └─ HelpDialog.cs       Help/about modal dialog
│  └─ Program.cs         # Entry point
├─ res/
│  ├─ lang/
│  │  └─ en-us.json      # English localization strings
│  └─ theme/
│     └─ light.json      # Light theme colors/fonts
├─ PDFsManager.csproj
└─ PDFsManager.sln
```

## Architecture

### Design Principles

- **Separation of Concerns**: Core/Models/Utils/UI layered architecture
- **Dependency Injection**: Constructor injection for testability
- **Fail-Safe Design**: Fallback mechanisms for localization, theming, file access
- **Thread Safety**: Background processing with thread-safe GUI updates

### State Machine

```plaintext
IDLE → STOPPED → RUNNING → STOPPED → ...
  ↓       ↓         ↓
  └───────┴─────> ERROR
```

### Processing Pipeline

```plaintext
1. File Detected (FileSystemWatcher or periodic scan)
2. Check File Lock (3 retry attempts, 2s delays)
3. Extract PDF Creation Date (iText7)
4. Generate New Filename (YYYY-MM-DD_HH-mm-ss.pdf)
5. Create Target Directories (YYYY/MM/)
6. Check for Duplicates (append _001, _002, etc.)
7. Move File to Target Location
8. Log Result (success or error)
```

## Configuration

**Location**: `config.json` (same directory as executable)

**Structure**:

```json
{
  "Workspace": "D:\\Documents\\PDFs",
  "AutoStartup": false
}
```

**Fields**:

- `Workspace`: Target directory for PDF monitoring (must exist)
- `AutoStartup`: Launch with Windows (feature pending)

## Logging

**Location**: `PDFsManager.log` (same directory as executable)

**Format**: `[YYYY-MM-DD HH:mm:ss] ACTION: Message`

**Action Codes**:

- `STRT`: Application start
- `STOP`: Application stop
- `SCAN`: Periodic scan triggered
- `FILE`: File processing initiated
- `DONE`: Processing completed successfully
- `ERRO`: Error occurred

**Example**:

```plaintext
[2026-01-15 16:30:05] STRT: PDFsManager started.
[2026-01-15 16:30:06] WKSP: Workspace set to 'D:\Documents\PDFs'
[2026-01-15 16:30:07] FILE: Processing 'invoice.pdf'
[2026-01-15 16:30:08] META: Extracted creation date: 2026-01-15 14:30:05
[2026-01-15 16:30:09] MOVE: Moved to '2026/01/2026-01-15_14-30-05.pdf'
[2026-01-15 16:30:10] DONE: Successfully processed 'invoice.pdf'
```

## Dependencies

| Package                                             | Version | Purpose                                            |
|-----------------------------------------------------|---------|---------------------------------------------------_|
| [Newtonsoft.Json](https://www.newtonsoft.com/json)  | 13.0.3  | JSON serialization for config/localization/theming |
| [iText7](https://itextpdf.com/)                     | 8.0.3   | PDF metadata extraction (CreationDate)             |

## Localization

**Default Language**: English (US)

**Adding Languages**:

1. Create `res/lang/{language-code}.json` (e.g., `vi-vn.json`)
2. Copy structure from `en-us.json`
3. Translate all string values
4. Update `LocalizationHelper.Initialize()` to load new language

**Fallback Mechanism**:

- If language file missing → Use hardcoded English strings
- If specific key missing → Use fallback parameter provided in code
- No runtime crashes due to missing translations

## Theming

**Default Theme**: Light

**Theme Structure** (`light.json`):

```json
{
  "Colors": {
    "Background": "#FFFFFF",
    "Primary": "#0078D4",
    "StatusRunning": "#28A745",
    "StatusStopped": "#FFC107"
  },
  "Fonts": {
    "FontFamily": "Segoe UI",
    "FontSizeNormal": 9
  }
}
```

**Adding Themes**:

1. Create `res/theme/{theme-name}.json` (e.g., `dark.json`)
2. Copy structure from `light.json`
3. Modify color hex codes and font values
4. Update `ThemeHelper.Initialize()` to load new theme

## Troubleshooting

### Application Won't Start

- **Solution**: Install [.NET 8.0 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Check**: Run `dotnet --version` in terminal

### Files Not Processing

- **PDF Invalid**: File must have valid PDF structure and creation date metadata
- **File Locked**: Wait for downloads to complete, close other programs using files
- **Permissions**: Ensure read/write access to workspace folder

### Workspace Invalid

- **Solution**: Folder must exist and be accessible
- **Check**: Browse to folder in Windows Explorer to verify

### Monitoring Stops

- **Check Logs**: Look for `ERRO` entries in log
- **Restart**: Stop and restart monitoring
- **Workspace**: Verify folder still exists and has permissions

## Development

### Code Style

- **C# Conventions**: Follow [Microsoft C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- **Naming**: PascalCase for public members, _camelCase for private fields
- **Documentation**: XML comments for all public APIs

### Testing

```powershell
# Unit tests (pending)
dotnet test

# Manual testing
dotnet run
# 1. Set workspace to test folder
# 2. Copy test PDFs to workspace
# 3. Start monitoring
# 4. Verify files moved and renamed correctly
```

### Contributing

1. Fork the repository
2. Create feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Open Pull Request

## Roadmap

### Version 1.0 (Current)

- ✅ Core file processing pipeline
- ✅ WinForms GUI with localization/theming
- ✅ Real-time monitoring with FileSystemWatcher
- ✅ Configuration persistence
- ✅ Comprehensive logging
- ✅ Windows auto-startup functionality

### Version 1.1 (Planned)

- [ ] Clean GUI improvements

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Author

- **1172005thinh (QuickComp.)**

- Facebook: [@quickcomp.hungthinhnguyen](https://www.facebook.com/quickcomp.hungthinhnguyen/)
- GitHub: [@1172005thinh](https://github.com/1172005thinh)

## Acknowledgments

- [iText7](https://itextpdf.com/) for PDF processing
- [Newtonsoft.Json](https://www.newtonsoft.com/json) for JSON handling
- [.NET Team](https://github.com/dotnet) for excellent framework and tools

---

- **Built with ❤️ using .NET 8.0 and Windows Forms**
