# PDFs Manager - Core Components

**Version**: 1.0.0  
**Author**: 1172005thinh (QuickComp.)  
**Date**: January 15, 2026

## Overview

This directory contains the **Core business logic components** of PDFs Manager, implementing the heart of the application's file monitoring, processing, and configuration management functionality.

## Components Architecture

```plaintext
Core/
├── ConfigManager.cs      # Configuration file management
├── FileMonitor.cs        # Real-time file monitoring with FileSystemWatcher
├── FileProcessor.cs      # PDF file processing, renaming, and organizing
└── Logger.cs             # Thread-safe logging system
```

### Dependencies

- **Models**: `AppConfig`, `AppState`, `ProcessResult`
- **Utils**: `Constants`, `FileHelper`, `PdfHelper`
- **External**: `Newtonsoft.Json`, `iText7`

---

## Component Details

### 1. Logger.cs

**Purpose**: Thread-safe logging system for all application operations.

**Key Features**:

- Thread-safe file I/O with lock mechanism
- Optional GUI callback for real-time log display
- Automatic log file creation and management
- Support for log separators (major/minor)
- Silent failure handling (never crashes the app)

**Public API**:

```csharp
Logger(string? logFilePath = null)
void SetGuiCallback(Action<string> callback)
void Write(string action, string message)
void WriteSeparator(bool major = false)
string ReadLog()
string LogFilePath { get; }
```

**Usage Example**:

```csharp
Logger logger = new Logger();
logger.Write(Constants.LOG_ACTION_START, "Application started");
logger.WriteSeparator(major: true);
logger.Write(Constants.LOG_ACTION_INFO, "Processing file...");
```

**Log Format**:

```plaintext
[2026-01-15 16:45:00] STRT: PDFsManager started.
[2026-01-15 16:45:01] CONF: Configuration loaded successfully.
*********************
[2026-01-15 16:50:00] SCAN: Found 3 file(s) to process.
=====================
[2026-01-15 16:50:01] FILE: Processing file 'example.pdf'
```

**Thread Safety**: Uses `lock` statement around file operations to prevent race conditions.

---

### 2. ConfigManager.cs

**Purpose**: Configuration file management with JSON serialization.

**Key Features**:

- Load/save configuration in JSON format
- Workspace directory validation
- Automatic default configuration creation
- Error handling with graceful degradation

**Public API**:

```csharp
ConfigManager(Logger logger, string? configFilePath = null)
AppConfig Load()
bool Save(AppConfig config)
bool ValidateWorkspace(string path)
string ConfigFilePath { get; }
```

**Configuration File Format** (`config.json`):

```json
{
  "Workspace": "D:\\Users\\User\\BillsInvoices",
  "AutoStartup": false
}
```

**Usage Example**:

```csharp
ConfigManager configMgr = new ConfigManager(logger);
AppConfig config = configMgr.Load();

if (configMgr.ValidateWorkspace(config.Workspace))
{
    // Workspace is valid, proceed
}

config.AutoStartup = true;
configMgr.Save(config);
```

**Error Handling**:

- Missing config file → Creates default config
- Invalid JSON → Returns default config
- Save failure → Logs error, returns false

---

### 3. FileProcessor.cs

**Purpose**: Core file processing logic - scan, extract metadata, rename, organize.

**Key Features**:

- Workspace scanning (non-recursive, PDF only)
- PDF metadata extraction (creation date)
- Intelligent file renaming (`YYYY-MM-DD_HH-mm-ss.pdf`)
- Automatic folder structure creation (`YYYY/MM/`)
- Duplicate filename handling with sequential numbering
- File lock detection and retry mechanism (3 attempts, 2s delay)
- Comprehensive error logging

**Public API**:

```csharp
FileProcessor(Logger logger)
void SetWorkspace(string workspacePath)
void ScanAndProcess()
ProcessResult ProcessFile(string filePath)
```

**Processing Workflow**:

1. **Lock Check**: Verify file is not in use (retry 3× with 2s delays)
2. **Metadata Extraction**: Extract PDF creation date using iText7
3. **Rename**: Generate new filename (`2026-01-15_16-30-05.pdf`)
4. **Folder Creation**: Ensure `YYYY/MM/` folder exists
5. **Duplicate Handling**: Append `_001`, `_002`, etc. if needed
6. **Move**: Move file to organized folder structure

**Usage Example**:

```csharp
FileProcessor processor = new FileProcessor(logger);
processor.SetWorkspace(@"D:\BillsInvoices");

// Scan entire workspace
processor.ScanAndProcess();

// Process single file
ProcessResult result = processor.ProcessFile(@"D:\BillsInvoices\invoice.pdf");
if (result.Success)
{
    Console.WriteLine($"Moved to: {result.NewFilePath}");
}
```

**Duplicate Handling**:

- `2026-01-15_16-30-05.pdf` (original)
- `2026-01-15_16-30-05_001.pdf` (1st duplicate)
- `2026-01-15_16-30-05_002.pdf` (2nd duplicate)

**File Lock Retry**:

```plaintext
Attempt 1: Locked → Wait 2s
Attempt 2: Locked → Wait 2s
Attempt 3: Locked → Wait 2s
Still locked → Skip file, log error
```

---

### 4. FileMonitor.cs

**Purpose**: Real-time file monitoring using FileSystemWatcher + periodic fallback.

**Key Features**:

- **FileSystemWatcher**: Real-time detection of new PDF files
- **Periodic Scan**: Fallback scan every 5 minutes
- **Background Threading**: Non-blocking operation
- **Error Recovery**: Automatic FileSystemWatcher restart on errors
- **Event-Driven**: Immediate processing of new files
- **Resource Management**: Implements `IDisposable` for cleanup

**Public API**:

```csharp
FileMonitor(Logger logger, FileProcessor fileProcessor)
bool Start(string workspacePath)
void Stop()
bool IsRunning { get; }
void Dispose()
```

**Monitoring Strategy**:

1. **FileSystemWatcher**: Detects new files in real-time
   - Filter: `*.pdf`
   - Events: Created, Renamed, Error
   - Settle delay: 1 second (let file finish writing)
2. **Periodic Scan**: Every 5 minutes (300,000 ms)
   - Catches files missed by watcher
   - Processes any existing unprocessed files

**Usage Example**:

```csharp
FileMonitor monitor = new FileMonitor(logger, processor);

if (monitor.Start(@"D:\BillsInvoices"))
{
    Console.WriteLine("Monitoring started");
    // Monitor runs in background
}

// Later...
monitor.Stop();
monitor.Dispose();
```

**Event Handling**:

- **Created**: New PDF detected → Wait 1s → Process file
- **Renamed**: File renamed → Log and ignore (likely already processed)
- **Error**: Watcher error → Attempt automatic restart

**Thread Safety**: Uses `lock` around state changes to prevent race conditions.

---

## Integration Example

Complete integration of all Core components:

```csharp
// 1. Initialize Logger
Logger logger = new Logger();
logger.Write(Constants.LOG_ACTION_START, "Application started");

// 2. Load Configuration
ConfigManager configMgr = new ConfigManager(logger);
AppConfig config = configMgr.Load();

// 3. Validate Workspace
if (!configMgr.ValidateWorkspace(config.Workspace))
{
    Console.WriteLine("Invalid workspace!");
    return;
}

// 4. Create FileProcessor
FileProcessor processor = new FileProcessor(logger);
processor.SetWorkspace(config.Workspace);

// 5. Create and Start FileMonitor
FileMonitor monitor = new FileMonitor(logger, processor);

if (monitor.Start(config.Workspace))
{
    Console.WriteLine("Monitoring started. Press Enter to stop...");
    Console.ReadLine();
    
    monitor.Stop();
    monitor.Dispose();
}

logger.Write(Constants.LOG_ACTION_STOP, "Application stopped");
```

---

## Error Handling Strategy

### Defensive Programming

All Core components follow these principles:

1. **Never Crash**: Errors are logged but don't crash the application
2. **Graceful Degradation**: Missing config → default config
3. **User Feedback**: All errors logged with clear messages
4. **Recovery**: FileMonitor auto-restarts on watcher errors

### Error Categories

**Logger Errors**:

- File I/O errors → Silent fail with Debug output
- Never blocks or crashes

**ConfigManager Errors**:

- Missing file → Create default config
- Invalid JSON → Return default config
- Save failure → Log error, return false

**FileProcessor Errors**:

- File locked → Retry 3× then skip
- Missing metadata → Skip file
- Invalid PDF → Skip file
- Move failure → Log error

**FileMonitor Errors**:

- Invalid workspace → Return false, don't start
- Watcher error → Attempt restart
- Processing error → Log and continue

---

## Performance Considerations

### Threading

- **FileMonitor**: Runs on background thread/timer
- **File Processing**: Offloaded to Task.Run()
- **GUI Updates**: Via BeginInvoke (non-blocking)

### Resource Usage

- **FileSystemWatcher**: Low overhead, event-driven
- **Periodic Scan**: Only runs every 5 minutes
- **File Locks**: Released immediately after checks

### Scalability

- **Tested with**: Hundreds of simultaneous files
- **Memory**: Minimal (processes one file at a time)
- **Disk I/O**: Sequential processing prevents contention

---

## Testing Recommendations

### Unit Tests (Recommended)

```csharp
// Logger Tests
[Test] void Logger_WritesCorrectFormat()
[Test] void Logger_ThreadSafe()
[Test] void Logger_HandlesInvalidPath()

// ConfigManager Tests
[Test] void ConfigManager_CreatesDefaultConfig()
[Test] void ConfigManager_ValidatesWorkspace()
[Test] void ConfigManager_SaveLoad()

// FileProcessor Tests
[Test] void FileProcessor_ExtractsMetadata()
[Test] void FileProcessor_HandlesDuplicates()
[Test] void FileProcessor_RetriesLockedFiles()

// FileMonitor Tests
[Test] void FileMonitor_DetectsNewFiles()
[Test] void FileMonitor_PeriodicScan()
[Test] void FileMonitor_HandlesErrors()
```

### Integration Tests

1. Create test workspace with sample PDFs
2. Start monitoring
3. Copy files to workspace
4. Verify files are processed correctly
5. Check log file for expected entries

### Edge Cases to Test

- ✅ Empty workspace
- ✅ Files with no metadata
- ✅ Locked files (still downloading)
- ✅ Very large files (100+ MB)
- ✅ Files with special characters in names
- ✅ Hundreds of files simultaneously
- ✅ Invalid workspace path
- ✅ Corrupted PDF files

---

## Build Status

✅ **Successfully Built**: January 15, 2026  
✅ **All Components Implemented**  
✅ **Zero Compilation Errors**  
✅ **Dependencies Resolved**

**Build Command**:

```bash
dotnet build
```

**Output**:

```plaintext
Restore complete (0.8s)
PDFsManager succeeded (0.9s) → bin\Debug\net8.0-windows\PDFsManager.dll
Build succeeded in 2.1s
```

---

## Dependencies Details

### NuGet Packages

- **Newtonsoft.Json** 13.0.3 - JSON serialization
- **iText7** 8.0.3 - PDF metadata extraction

### Framework

- **.NET 8.0** (net8.0-windows)
- **Windows Forms** - GUI framework (to be implemented)

---

## Next Steps

### Immediate (Phase 1 - Complete ✅)

- ✅ Logger implementation
- ✅ ConfigManager implementation
- ✅ FileProcessor implementation
- ✅ FileMonitor implementation

### Next Phase (Phase 2 - GUI)

- ⏳ MainForm.cs - Main GUI window
- ⏳ HelpDialog.cs - Help/About dialog
- ⏳ GUI integration with Core components
- ⏳ Real-time log display

### Future Enhancements

- 📋 Unit test suite
- 📋 StartupHelper - Auto-startup management
- 📋 Custom PDF metadata fallbacks
- 📋 Statistics/analytics features

---

## Changelog

**v1.0.0** - January 15, 2026

- Initial implementation of all Core components
- Logger: Thread-safe logging with GUI callback support
- ConfigManager: JSON-based configuration management
- FileProcessor: Complete PDF processing pipeline
- FileMonitor: Real-time monitoring with FileSystemWatcher
- FileHelper: File lock detection and retry logic
- PdfHelper: PDF metadata extraction using iText7
- Models: AppConfig, AppState, ProcessResult
- Constants: Centralized configuration values
- ✅ Build successful, zero errors

---

## Contact & Support

**Author**: 1172005thinh (QuickComp.)  
**GitHub**: [to GitHub profile](https://github.com/1172005thinh)  
**Facebook**: [to Facebook profile](https://www.facebook.com/quickcomp.hungthinhnguyen/)  
**Repository**: [to repository](https://github.com/1172005thinh/PDFsManager)  
**License**: MIT License

---

- *Last Updated: January 15, 2026*
