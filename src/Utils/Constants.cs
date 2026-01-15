namespace PDFsManager.Utils
{
    /// <summary>
    /// Application-wide constants for configuration, logging, and file operations.
    /// Centralizes all magic numbers and strings for easy maintenance.
    /// </summary>
    public static class Constants
    {
        // File Names
        public const string CONFIG_FILENAME = "config.json";
        public const string LOG_FILENAME = "PDFsManager.log";

        // Scanning Configuration
        public const int SCAN_INTERVAL_MINUTES = 5;
        public const int SCAN_INTERVAL_MS = SCAN_INTERVAL_MINUTES * 60 * 1000;

        // File Lock Retry Configuration
        public const int FILE_LOCK_RETRY_COUNT = 3;
        public const int FILE_LOCK_RETRY_DELAY_MS = 2000;

        // FileSystemWatcher Configuration
        public const string PDF_FILTER = "*.pdf";
        public const int WATCHER_SETTLE_DELAY_MS = 1000;

        // Log Action Codes
        public const string LOG_ACTION_START = "STRT";
        public const string LOG_ACTION_STOP = "STOP";
        public const string LOG_ACTION_WORKSPACE = "WKSP";
        public const string LOG_ACTION_CONFIG = "CONF";
        public const string LOG_ACTION_SCAN = "SCAN";
        public const string LOG_ACTION_DETECT = "DTCT";
        public const string LOG_ACTION_FILE = "FILE";
        public const string LOG_ACTION_READ = "READ";
        public const string LOG_ACTION_META = "META";
        public const string LOG_ACTION_RENAME = "RENM";
        public const string LOG_ACTION_DUPLICATE = "DUPL";
        public const string LOG_ACTION_DIRS = "DIRS";
        public const string LOG_ACTION_DIRC = "DIRC";
        public const string LOG_ACTION_MOVE = "MOVE";
        public const string LOG_ACTION_DONE = "DONE";
        public const string LOG_ACTION_INFO = "INFO";
        public const string LOG_ACTION_WARN = "WARN";
        public const string LOG_ACTION_ERROR = "ERRO";

        // Log Separators
        public const string LOG_SEPARATOR_MAJOR = "*********************";
        public const string LOG_SEPARATOR_MINOR = "=====================";

        // Date/Time Formats
        public const string TIMESTAMP_FORMAT = "yyyy-MM-dd HH:mm:ss";
        public const string FILENAME_DATE_FORMAT = "yyyy-MM-dd_HH-mm-ss";
        public const string FOLDER_YEAR_FORMAT = "yyyy";
        public const string FOLDER_MONTH_FORMAT = "MM";

        // Duplicate File Naming
        public const string DUPLICATE_SUFFIX_FORMAT = "_{0:D3}"; // _001, _002, etc.
        public const int DUPLICATE_MAX_ATTEMPTS = 999;

        // Application Info
        public const string APP_NAME = "PDFs Manager";
        public const string APP_VERSION = "1.0.0";
        public const string APP_AUTHOR = "1172005thinh(QuickComp.)";
    }
}
