# UI Layer Documentation

## Overview

The UI layer provides a Windows Forms-based graphical user interface for PDFs Manager. It implements a clean, modern interface with support for localization and theming.

## Components

### MainForm.cs

**Purpose**: Main application window providing user control over file monitoring and configuration.

**Key Features**:

- Real-time log display with auto-scroll
- Workspace selection via folder browser dialog
- Start/Stop monitoring controls
- Color-coded status indicators
- Configuration management with dirty tracking
- Thread-safe GUI updates via BeginInvoke

**Layout** (600×400 fixed, non-resizable):

```plaintext
┌─────────────────────────────────────────────┐
│ PDFs Manager                                │
├─────────────────────────────────────────────┤
│ ┌─ Log Section ────────────────────────┐   │
│ │ [Log TextBox - 560×150]              │   │
│ │                                       │   │
│ │ Consolas monospace, scrollable       │   │
│ └───────────────────────────────────────┘   │
│ [Refresh] [Clear]                            │
│                                              │
│ ┌─ Configuration ──────────────────────┐   │
│ │ Workspace: [TextBox - read-only]     │   │
│ │ [Browse...]                           │   │
│ │ [✓] Auto Startup with Windows        │   │
│ └───────────────────────────────────────┘   │
│                                              │
│ [Start] [Help] [Exit] [Cancel]              │
│ Status: ● Idle                               │
└─────────────────────────────────────────────┘
```

**State Management**:

- `IDLE`: No valid workspace (gray status)
- `STOPPED`: Valid workspace, monitoring off (yellow status)
- `RUNNING`: Active monitoring (green status)
- `ERROR`: Critical failure (red status)

**Event Handlers**:

1. **Refresh**: Reloads log content from file
2. **Clear**: Clears log display only
3. **Browse**: Opens folder dialog, validates workspace, stops monitor if running
4. **Start/Stop**:
   - Validates workspace
   - Saves config if dirty
   - Starts/stops FileMonitor
   - Updates state and UI
5. **Help**: Opens HelpDialog (modal)
6. **Exit**: Stops monitor, saves config if valid, closes application
7. **Cancel**: Discards unsaved changes, reloads from file

**Configuration Tracking**:

- `_configDirty` flag tracks unsaved changes
- Changes detected on workspace browse or auto-startup toggle
- Save prompted only if workspace is valid
- Explicit Cancel button to discard changes

### HelpDialog.cs

**Purpose**: Modal dialog displaying usage instructions, author info, and external links.

**Key Features**:

- Scrollable help text from localization file
- Clickable links to Facebook, GitHub, and license
- Fallback help content if localization fails
- Fixed 600×500 size, centered on parent

**Layout**:

```plaintext
┌──────────────────────────────────────────┐
│ PDFs Manager Help                        │
├──────────────────────────────────────────┤
│ ┌─ Help Content ─────────────────────┐  │
│ │ WHAT DOES THIS TOOL DO?           │  │
│ │ ...                                │  │
│ │ ...                                │  │
│ │ (Scrollable TextBox - 560×380)    │  │
│ └────────────────────────────────────┘  │
├──────────────────────────────────────────┤
│ Author: 1172005thinh (QuickComp.)        │
│ Contact: [Facebook] [GitHub] [License]   │
│ v1.0.0                        [Close]     │
└──────────────────────────────────────────┘
```

**External Links**:

- Facebook: `https://www.facebook.com/quickcomp.hungthinhnguyen/`
- GitHub: `https://github.com/1172005thinh/PDFsManager`
- License: `https://github.com/1172005thinh/PDFsManager/blob/main/LICENSE`

## Localization System

### LocalizationHelper.cs

**Purpose**: Provides language string retrieval with fallback mechanism.

**Usage**:

```csharp
// Initialize once at startup
LocalizationHelper.Initialize();

// Get localized strings
string title = LocalizationHelper.Get("MainWindow.Title", "PDFs Manager");
string btnStart = LocalizationHelper.Get("MainWindow.ControlButtons.StartButton", "Start");
```

**Fallback Strategy**:

1. Try loading `res/lang/en-us.json`
2. If file missing or parsing fails, use hardcoded English strings
3. If specific key missing, use provided fallback parameter
4. Log any localization failures

**Language File Structure** (`en-us.json`):

```json
{
  "AppName": "PDFs Manager",
  "AppVersion": "v1.0.0",
  "MainWindow": {
    "LogSection": {
      "Title": "Activity Log",
      "RefreshButton": "Refresh"
    },
    "ConfigSection": {
      "WorkspaceLabel": "Workspace:"
    }
  },
  "HelpDialog": {
    "HelpContent": "Full help text..."
  },
  "Messages": {
    "WorkspaceInvalid": "Please select a valid workspace folder."
  }
}
```

## Theming System

### ThemeHelper.cs

**Purpose**: Provides color, font, and styling values with fallback mechanism.

**Usage**:

```csharp
// Initialize once at startup
ThemeHelper.Initialize();

// Get theme values
Color bg = ThemeHelper.GetColor("Colors.Background", ThemeHelper.Fallback.Background);
int fontSize = ThemeHelper.GetInt("Fonts.FontSizeNormal", ThemeHelper.Fallback.FontSizeNormal);
string fontFamily = ThemeHelper.GetString("Fonts.FontFamily", ThemeHelper.Fallback.FontFamily);
```

**Fallback Strategy**:

1. Try loading `res/theme/light.json`
2. If file missing or parsing fails, use hardcoded fallback values
3. If specific key missing, use provided fallback parameter
4. Support hex color parsing (#RRGGBB format)

**Theme File Structure** (`light.json`):

```json
{
  "Colors": {
    "Background": "#FFFFFF",
    "Surface": "#F5F5F5",
    "Primary": "#0078D4",
    "Text": "#212529",
    "StatusRunning": "#28A745",
    "StatusStopped": "#FFC107"
  },
  "Fonts": {
    "FontFamily": "Segoe UI",
    "FontFamilyMono": "Consolas",
    "FontSizeNormal": 9,
    "FontSizeLarge": 10
  },
  "Spacing": {
    "PaddingSmall": 4,
    "PaddingNormal": 8
  }
}
```

**Fallback Class** (ThemeHelper.Fallback):

- Provides hardcoded Color objects for all theme colors
- Default font families: Segoe UI, Consolas
- Default sizes: 9pt normal, 10pt large, 8pt mono

## Threading and Synchronization

### GUI Thread Safety

All UI updates from background threads use `Control.BeginInvoke`:

```csharp
private void AppendLogToGui(string message)
{
    if (_logTextBox.InvokeRequired)
    {
        _logTextBox.BeginInvoke(new Action(() => AppendLogToGui(message)));
        return;
    }
    
    _logTextBox.AppendText(message + Environment.NewLine);
    _logTextBox.SelectionStart = _logTextBox.Text.Length;
    _logTextBox.ScrollToCaret();
}
```

### Logger GUI Callback

MainForm registers a callback with Logger to receive real-time log updates:

```csharp
private void LoadConfiguration()
{
    _logger.SetGuiCallback(AppendLogToGui);
    // ...
}
```

This enables:

- Real-time log display as files are processed
- Thread-safe updates from FileMonitor background thread
- No polling or timer-based updates needed

## State Transitions

### Application State Machine

```plaintext
[IDLE] ──Browse (valid)──> [STOPPED]
  ↑                             ↓
  └─────Browse (invalid)────────┘
  
[STOPPED] ──Start──> [RUNNING] ──Stop──> [STOPPED]
    ↓                    ↓
    └──────Error─────> [ERROR]
    
[ERROR] ──Browse/Retry──> [IDLE/STOPPED]
```

### State-Dependent UI Behavior

| State   | Start Button | Browse Button | Status Color | Log Display |
|---------|--------------|---------------|--------------|-------------|
| IDLE    | Disabled     | Enabled       | Gray         | Enabled     |
| STOPPED | "Start"      | Enabled       | Yellow       | Enabled     |
| RUNNING | "Stop"       | Disabled      | Green        | Enabled     |
| ERROR   | Disabled     | Enabled       | Red          | Enabled     |

## Configuration Persistence

### Config Dirty Tracking

Changes to workspace or auto-startup are tracked:

```csharp
private void BrowseButton_Click(object? sender, EventArgs e)
{
    // ... folder dialog logic ...
    _currentConfig.Workspace = folderPath;
    _configDirty = true;  // Mark as dirty
    UpdateStatus();
}
```

### Save Strategies

1. **Explicit Start**: Save before starting monitor (if valid)
2. **Exit**: Prompt to save if dirty and valid
3. **Cancel**: Discard changes, reload from file

### Validation Flow

```plaintext
User Changes Config
    ↓
_configDirty = true
    ↓
User Clicks Start/Exit
    ↓
Validate Workspace
    ↓
If Valid: Save → Proceed
If Invalid: Show Error → No Save
```

## Error Handling

### User-Facing Errors

- **Invalid Workspace**: MessageBox with localized error message
- **File Access Issues**: Logged, not shown as MessageBox (non-blocking)
- **Monitor Failures**: State → ERROR, log details

### Non-Blocking Design

- File processing errors logged but don't stop monitor
- Missing localization/theme files use fallbacks
- Invalid config creates default

## Integration Points

### From Program.cs

```csharp
// Initialize helpers
LocalizationHelper.Initialize();
ThemeHelper.Initialize();

// Create Core components
Logger logger = new Logger();
ConfigManager configManager = new ConfigManager(logger);
FileProcessor fileProcessor = new FileProcessor(logger);
FileMonitor fileMonitor = new FileMonitor(logger, fileProcessor);

// Load config and determine state
AppConfig config = configManager.Load();
bool isValid = configManager.ValidateWorkspace(config.Workspace);
AppState initialState = isValid ? AppState.STOPPED : AppState.IDLE;

// Launch GUI
Application.EnableVisualStyles();
Application.SetCompatibleTextRenderingDefault(false);

using (MainForm mainForm = new MainForm(logger, configManager, 
                                       fileProcessor, fileMonitor, 
                                       config, initialState))
{
    Application.Run(mainForm);
}
```

### To Core Layer

- **Logger**: Receives GUI callback, sends updates to MainForm
- **ConfigManager**: Load/save operations called from MainForm
- **FileProcessor**: Workspace configured via SetWorkspace()
- **FileMonitor**: Start/Stop controlled via MainForm buttons

## Testing Considerations

### Manual Test Cases

- **First Launch (No Config)**
  - ✓ Window opens with IDLE state
  - ✓ Workspace field empty
  - ✓ Start button disabled
  - ✓ Log shows initialization message

- **Workspace Configuration**
  - ✓ Browse button opens folder dialog
  - ✓ Selecting folder updates workspace field
  - ✓ Valid folder → state changes to STOPPED
  - ✓ Invalid folder → error message shown

- **Monitoring Lifecycle**
  - ✓ Start button enables when workspace valid
  - ✓ Clicking Start → state changes to RUNNING
  - ✓ Button label changes to "Stop"
  - ✓ Browse button disables
  - ✓ Clicking Stop → state changes to STOPPED

- **Configuration Persistence**
  - ✓ Change workspace → _configDirty = true
  - ✓ Start → config saved automatically
  - ✓ Exit → prompt to save if dirty
  - ✓ Cancel → changes discarded

- **Log Display**
  - ✓ Refresh reloads from file
  - ✓ Clear empties display
  - ✓ Real-time updates appear during monitoring
  - ✓ Auto-scroll to bottom on new entries

- **Help Dialog**
  - ✓ Help button opens modal dialog
  - ✓ Main window disabled while open
  - ✓ Links clickable (Facebook, GitHub, License)
  - ✓ Close button returns to main window
- **Localization Fallback**
  - ✓ Delete en-us.json → app uses hardcoded strings
  - ✓ Invalid JSON → app uses fallback strings
  - ✓ Missing key → uses provided fallback parameter

- **Theme Fallback**
  - ✓ Delete light.json → app uses default colors
  - ✓ Invalid JSON → app uses fallback theme
  - ✓ Missing color → uses fallback Color object

## Future Enhancements

### Planned Features

- [ ] Dark theme support (dark.json)
- [ ] Additional language files (vi-vn.json, etc.)
- [ ] Settings dialog for advanced options
- [ ] Drag-and-drop workspace selection
- [ ] System tray icon with minimize-to-tray
- [ ] Log filtering and search
- [ ] Export log to file option

### Architecture Improvements

- [ ] MVVM pattern with data binding
- [ ] Custom theme engine with live switching
- [ ] Localization hot-reload for development
- [ ] Accessibility support (screen readers)
- [ ] High DPI awareness and scaling

## Dependencies

**Internal**:

- `PDFsManager.Core`: Logger, ConfigManager, FileProcessor, FileMonitor
- `PDFsManager.Models`: AppConfig, AppState
- `PDFsManager.Utils`: Constants, LocalizationHelper, ThemeHelper

**External**:

- `System.Windows.Forms`: GUI framework
- `System.Drawing`: Colors, fonts, graphics
- `Newtonsoft.Json`: JSON parsing for localization/theming

## Resource Files

**Location**: `res/` directory (copied to output)

**Language Files**:

- `res/lang/en-us.json`: English (US) strings

**Theme Files**:

- `res/theme/light.json`: Light theme colors/fonts

**Build Configuration**:

```xml
<ItemGroup>
  <Content Include="res\**\*">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```

This ensures resource files are copied to `bin/Debug/net8.0-windows/res/` during build.
