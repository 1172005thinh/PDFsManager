using System.Collections.Generic;
using Newtonsoft.Json;

namespace PDFsManager.Models
{
    /// <summary>
    /// Configuration data model for PDFs Manager application.
    /// Stores workspace directory and auto-startup preference.
    /// </summary>
    public class AppConfig
    {
        /// <summary>
        /// Workspace directories path where PDF files are monitored and processed.
        /// </summary>
        public List<string> Workspaces { get; set; } = new List<string>();

        [JsonProperty("Workspace")]
        private string LegacyWorkspace
        {
            set
            {
                if (!string.IsNullOrEmpty(value) && !Workspaces.Contains(value))
                {
                    Workspaces.Add(value);
                }
            }
        }

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
        public AppConfig(List<string> workspaces, bool autoStartup)
        {
            Workspaces = workspaces ?? new List<string>();
            AutoStartup = autoStartup;
        }
    }
}
