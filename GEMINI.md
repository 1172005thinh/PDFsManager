# PDFs Manager - AI Assistant Context

## Project Overview
**PDFs Manager** is a C# Windows Desktop application built with .NET 8.0 and Windows Forms. Its primary function is to automatically organize PDF files by monitoring a workspace folder, extracting the creation date from PDF metadata, and moving the files into structured `YYYY/MM` directories with standardized filenames (`YYYY-MM-DD_HH-mm-ss.pdf`). 

**Current Architecture:**
The application follows a clean layered architecture:
*   **Core:** Business logic, including file processing pipeline, file system monitoring, thread-safe logging, and configuration management.
*   **Models:** Data structures and plain objects (e.g., `AppConfig`, state enums).
*   **Utils:** Helper classes for file operations, PDF metadata extraction (via iText7), localization, theming, and startup integration.
*   **UI:** WinForms layer (`MainForm.cs`, `HelpDialog.cs`) providing a modern interface with real-time log display.

**Current State & Upcoming Changes:**
Currently, the application monitors a single workspace directory defined by `AppConfig.Workspace` (string). The user intends to upgrade the application to support monitoring multiple workspaces concurrently (minimum: 1, maximum: 8 workspaces). This will require updates to the UI, Configuration models, File Monitors, and underlying processing logic.

## Building and Running
The project utilizes the .NET CLI.

*   **Restore dependencies:**
    ```powershell
    dotnet restore
    ```
*   **Build the project:**
    ```powershell
    dotnet build
    ```
*   **Run the application:**
    ```powershell
    dotnet run
    ```
*   **Build for Release:**
    ```powershell
    dotnet build -c Release
    ```

Visual Studio Code tasks are also configured (`Ctrl+Shift+B` for build, `F5` for debug).

## Development Conventions
*   **Coding Style:** Adheres to Microsoft C# Coding Conventions. 
    *   PascalCase for public members (Methods, Properties, Classes).
    *   _camelCase for private fields.
*   **Documentation:** XML comments are expected for all public APIs.
*   **Thread Safety:** Background processing (monitoring and moving files) must safely update the GUI using proper cross-thread invocation mechanisms.
*   **Dependencies:** Uses `Newtonsoft.Json` for configuration/localization/theming and `iText7` for PDF metadata extraction.
*   **Resilience:** Operations should include fallback mechanisms (e.g., file lock retries during processing, fallback localization strings).

## Key Files
*   `README.md`: Contains the comprehensive project documentation, features, state machine diagrams, and pipeline steps.
*   `src/Models/AppConfig.cs`: Configuration data model. (Note: Currently holds a single `Workspace` string; needs to be updated to support an array/list of workspaces).
*   `src/Core/FileMonitor.cs`: Contains the logic for the `FileSystemWatcher` and periodic scans.
*   `src/UI/MainForm.cs`: The main user interface that will need to be updated to handle adding/removing multiple workspace paths.