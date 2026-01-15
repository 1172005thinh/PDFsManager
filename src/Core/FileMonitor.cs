using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using PDFsManager.Utils;

namespace PDFsManager.Core
{
    /// <summary>
    /// Background file monitoring system using FileSystemWatcher and periodic scanning.
    /// Runs on separate thread to avoid blocking GUI operations.
    /// </summary>
    public class FileMonitor : IDisposable
    {
        private readonly Logger _logger;
        private readonly FileProcessor _fileProcessor;
        private FileSystemWatcher? _watcher;
        private Timer? _periodicScanTimer;
        private string _workspacePath = string.Empty;
        private bool _isRunning = false;
        private readonly object _lockObject = new object();

        /// <summary>
        /// Creates a new FileMonitor instance.
        /// </summary>
        /// <param name="logger">Logger instance for logging operations.</param>
        /// <param name="fileProcessor">FileProcessor instance for processing detected files.</param>
        public FileMonitor(Logger logger, FileProcessor fileProcessor)
        {
            _logger = logger;
            _fileProcessor = fileProcessor;
        }

        /// <summary>
        /// Starts monitoring the specified workspace directory.
        /// </summary>
        /// <param name="workspacePath">Path to workspace directory to monitor.</param>
        /// <returns>True if started successfully, false otherwise.</returns>
        public bool Start(string workspacePath)
        {
            lock (_lockObject)
            {
                if (_isRunning)
                {
                    _logger.Write(Constants.LOG_ACTION_WARN, "Monitor is already running.");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(workspacePath) || !Directory.Exists(workspacePath))
                {
                    _logger.Write(Constants.LOG_ACTION_ERROR, "Invalid workspace path. Cannot start monitoring.");
                    return false;
                }

                try
                {
                    _workspacePath = workspacePath;
                    _fileProcessor.SetWorkspace(workspacePath);

                    // Initialize FileSystemWatcher
                    _watcher = new FileSystemWatcher(_workspacePath)
                    {
                        Filter = Constants.PDF_FILTER,
                        NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime,
                        IncludeSubdirectories = false,
                        EnableRaisingEvents = true
                    };

                    // Subscribe to events
                    _watcher.Created += OnFileCreated;
                    _watcher.Renamed += OnFileRenamed;
                    _watcher.Error += OnWatcherError;

                    // Start periodic scan timer (every 5 minutes)
                    _periodicScanTimer = new Timer(
                        callback: _ => PeriodicScan(),
                        state: null,
                        dueTime: TimeSpan.Zero, // Start immediately
                        period: TimeSpan.FromMilliseconds(Constants.SCAN_INTERVAL_MS)
                    );

                    _isRunning = true;
                    _logger.Write(Constants.LOG_ACTION_START, "Monitoring started.");
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.Write(Constants.LOG_ACTION_ERROR, $"Failed to start monitoring: {ex.Message}");
                    CleanupResources();
                    return false;
                }
            }
        }

        /// <summary>
        /// Stops monitoring and cleanup resources.
        /// </summary>
        public void Stop()
        {
            lock (_lockObject)
            {
                if (!_isRunning)
                {
                    return;
                }

                try
                {
                    CleanupResources();
                    _isRunning = false;
                    _logger.Write(Constants.LOG_ACTION_STOP, "Monitoring stopped.");
                }
                catch (Exception ex)
                {
                    _logger.Write(Constants.LOG_ACTION_ERROR, $"Error stopping monitoring: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Event handler for new PDF files detected by FileSystemWatcher.
        /// </summary>
        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            try
            {
                string fileName = Path.GetFileName(e.FullPath);
                _logger.Write(Constants.LOG_ACTION_DETECT, $"New file detected '{fileName}'");

                // Wait for file to finish writing
                Task.Delay(Constants.WATCHER_SETTLE_DELAY_MS).Wait();

                // Process the file on background thread
                Task.Run(() =>
                {
                    _logger.WriteSeparator(major: false);
                    _fileProcessor.ProcessFile(e.FullPath);
                    _logger.WriteSeparator(major: false);
                });
            }
            catch (Exception ex)
            {
                _logger.Write(Constants.LOG_ACTION_ERROR, $"Error processing created file: {ex.Message}");
            }
        }

        /// <summary>
        /// Event handler for renamed files in workspace.
        /// Renamed files are likely already processed or user-managed, so we ignore them.
        /// </summary>
        private void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            _logger.Write(Constants.LOG_ACTION_INFO, $"File renamed in workspace: '{e.OldName}' -> '{e.Name}'. Ignoring.");
        }

        /// <summary>
        /// Event handler for FileSystemWatcher errors.
        /// </summary>
        private void OnWatcherError(object sender, ErrorEventArgs e)
        {
            Exception? ex = e.GetException();
            _logger.Write(Constants.LOG_ACTION_ERROR, $"FileSystemWatcher error: {ex?.Message ?? "Unknown error"}");
            
            // Try to recover by restarting the watcher
            if (_watcher != null)
            {
                try
                {
                    _watcher.EnableRaisingEvents = false;
                    Thread.Sleep(1000);
                    _watcher.EnableRaisingEvents = true;
                    _logger.Write(Constants.LOG_ACTION_INFO, "FileSystemWatcher restarted after error.");
                }
                catch
                {
                    _logger.Write(Constants.LOG_ACTION_ERROR, "Failed to restart FileSystemWatcher.");
                }
            }
        }

        /// <summary>
        /// Periodic scan callback (every 5 minutes).
        /// Fallback mechanism to catch any files missed by FileSystemWatcher.
        /// </summary>
        private void PeriodicScan()
        {
            if (!_isRunning)
                return;

            try
            {
                _logger.Write(Constants.LOG_ACTION_SCAN, "Periodic scan started.");
                _fileProcessor.ScanAndProcess();
            }
            catch (Exception ex)
            {
                _logger.Write(Constants.LOG_ACTION_ERROR, $"Periodic scan failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Cleanup FileSystemWatcher and timer resources.
        /// </summary>
        private void CleanupResources()
        {
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Created -= OnFileCreated;
                _watcher.Renamed -= OnFileRenamed;
                _watcher.Error -= OnWatcherError;
                _watcher.Dispose();
                _watcher = null;
            }

            if (_periodicScanTimer != null)
            {
                _periodicScanTimer.Dispose();
                _periodicScanTimer = null;
            }
        }

        /// <summary>
        /// Gets whether the monitor is currently running.
        /// </summary>
        public bool IsRunning => _isRunning;

        /// <summary>
        /// Disposes of resources.
        /// </summary>
        public void Dispose()
        {
            Stop();
            GC.SuppressFinalize(this);
        }
    }
}
