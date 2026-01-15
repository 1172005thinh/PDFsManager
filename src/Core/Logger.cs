using System;
using System.IO;
using PDFsManager.Utils;

namespace PDFsManager.Core
{
    /// <summary>
    /// Thread-safe logging system for PDFs Manager.
    /// Writes log entries to file and optionally notifies GUI for real-time updates.
    /// </summary>
    public class Logger
    {
        private readonly string _logFilePath;
        private readonly object _lockObject = new object();
        private Action<string>? _guiUpdateCallback;

        /// <summary>
        /// Creates a new logger instance with the specified log file path.
        /// </summary>
        /// <param name="logFilePath">Path to the log file. If null, uses default location.</param>
        public Logger(string? logFilePath = null)
        {
            _logFilePath = logFilePath ?? Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                Constants.LOG_FILENAME
            );

            // Ensure log file exists
            EnsureLogFileExists();
        }

        /// <summary>
        /// Sets the callback function for GUI updates.
        /// This allows the logger to notify the UI when new log entries are written.
        /// </summary>
        /// <param name="callback">Action to invoke with log message for GUI display.</param>
        public void SetGuiCallback(Action<string> callback)
        {
            _guiUpdateCallback = callback;
        }

        /// <summary>
        /// Writes a log entry with the specified action code and message.
        /// Thread-safe and supports GUI updates via callback.
        /// </summary>
        /// <param name="action">Log action code (e.g., STRT, ERRO, INFO).</param>
        /// <param name="message">Log message details.</param>
        public void Write(string action, string message)
        {
            try
            {
                string timestamp = DateTime.Now.ToString(Constants.TIMESTAMP_FORMAT);
                string logEntry = $"[{timestamp}] {action}: {message}";

                // Thread-safe file write
                lock (_lockObject)
                {
                    File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
                }

                // Notify GUI if callback is set (use BeginInvoke pattern in actual GUI)
                _guiUpdateCallback?.Invoke(logEntry);
            }
            catch (Exception ex)
            {
                // Silent fail - logging should never crash the application
                System.Diagnostics.Debug.WriteLine($"Logger failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Writes a separator line to the log for visual organization.
        /// </summary>
        /// <param name="major">If true, writes major separator; otherwise minor separator.</param>
        public void WriteSeparator(bool major = false)
        {
            string separator = major ? Constants.LOG_SEPARATOR_MAJOR : Constants.LOG_SEPARATOR_MINOR;
            
            try
            {
                lock (_lockObject)
                {
                    File.AppendAllText(_logFilePath, separator + Environment.NewLine);
                }

                _guiUpdateCallback?.Invoke(separator);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Logger failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Reads the entire log file content.
        /// </summary>
        /// <returns>Log file content as string.</returns>
        public string ReadLog()
        {
            try
            {
                lock (_lockObject)
                {
                    if (File.Exists(_logFilePath))
                    {
                        return File.ReadAllText(_logFilePath);
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Error reading log file: {ex.Message}";
            }

            return string.Empty;
        }

        /// <summary>
        /// Ensures the log file exists, creating it if necessary.
        /// </summary>
        private void EnsureLogFileExists()
        {
            try
            {
                if (!File.Exists(_logFilePath))
                {
                    string? directory = Path.GetDirectoryName(_logFilePath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    File.WriteAllText(_logFilePath, string.Empty);
                }
            }
            catch (Exception)
            {
                // Silent fail - will retry on first write
            }
        }

        /// <summary>
        /// Gets the log file path.
        /// </summary>
        public string LogFilePath => _logFilePath;
    }
}
