namespace PDFsManager.Models
{
    /// <summary>
    /// Configuration data model for PDFs Manager application.
    /// Stores workspace directory and auto-startup preference.
    /// </summary>
    public class AppConfig
    {
        /// <summary>
        /// Workspace directory path where PDF files are monitored and processed.
        /// </summary>
        public string Workspace { get; set; } = string.Empty;

        /// <summary>
        /// Whether the application should start automatically when Windows starts.
        /// </summary>
        public bool AutoStartup { get; set; } = false;

        /// <summary>
        /// Creates a default configuration with empty workspace and auto-startup disabled.
        /// </summary>
        public AppConfig()
        {
        }

        /// <summary>
        /// Creates a configuration with specified values.
        /// </summary>
        public AppConfig(string workspace, bool autoStartup)
        {
            Workspace = workspace;
            AutoStartup = autoStartup;
        }
    }
}
