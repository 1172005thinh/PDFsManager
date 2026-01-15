using System;
using System.IO;
using Newtonsoft.Json;
using PDFsManager.Models;
using PDFsManager.Utils;

namespace PDFsManager.Core
{
    /// <summary>
    /// Configuration file management for PDFs Manager.
    /// Handles loading, saving, and validating application configuration.
    /// </summary>
    public class ConfigManager
    {
        private readonly string _configFilePath;
        private readonly Logger _logger;

        /// <summary>
        /// Creates a new ConfigManager instance.
        /// </summary>
        /// <param name="logger">Logger instance for logging operations.</param>
        /// <param name="configFilePath">Path to config file. If null, uses default location.</param>
        public ConfigManager(Logger logger, string? configFilePath = null)
        {
            _logger = logger;
            _configFilePath = configFilePath ?? Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                Constants.CONFIG_FILENAME
            );
        }

        /// <summary>
        /// Loads configuration from file.
        /// Creates default configuration if file doesn't exist or is invalid.
        /// </summary>
        /// <returns>AppConfig object with loaded or default configuration.</returns>
        public AppConfig Load()
        {
            try
            {
                if (File.Exists(_configFilePath))
                {
                    string json = File.ReadAllText(_configFilePath);
                    var config = JsonConvert.DeserializeObject<AppConfig>(json);

                    if (config != null)
                    {
                        _logger.Write(Constants.LOG_ACTION_CONFIG, "Configuration loaded successfully.");
                        return config;
                    }
                }

                // File doesn't exist or is invalid - create default
                _logger.Write(Constants.LOG_ACTION_CONFIG, "Configuration file not found or invalid. Creating default configuration.");
                var defaultConfig = new AppConfig();
                Save(defaultConfig);
                return defaultConfig;
            }
            catch (Exception ex)
            {
                _logger.Write(Constants.LOG_ACTION_ERROR, $"Failed to load configuration: {ex.Message}");
                return new AppConfig(); // Return default config on error
            }
        }

        /// <summary>
        /// Saves configuration to file.
        /// </summary>
        /// <param name="config">Configuration object to save.</param>
        /// <returns>True if save was successful, false otherwise.</returns>
        public bool Save(AppConfig config)
        {
            try
            {
                string json = JsonConvert.SerializeObject(config, Formatting.Indented);
                
                // Ensure directory exists
                string? directory = Path.GetDirectoryName(_configFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(_configFilePath, json);
                _logger.Write(Constants.LOG_ACTION_CONFIG, "Configuration saved successfully.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Write(Constants.LOG_ACTION_ERROR, $"Failed to save configuration: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Validates that a workspace directory path is valid.
        /// </summary>
        /// <param name="path">Directory path to validate.</param>
        /// <returns>True if valid, false otherwise.</returns>
        public bool ValidateWorkspace(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                _logger.Write(Constants.LOG_ACTION_WORKSPACE, "Workspace path is empty.");
                return false;
            }

            if (!Directory.Exists(path))
            {
                _logger.Write(Constants.LOG_ACTION_WORKSPACE, $"Workspace directory does not exist: '{path}'");
                return false;
            }

            _logger.Write(Constants.LOG_ACTION_WORKSPACE, $"Workspace is valid: '{path}'");
            return true;
        }

        /// <summary>
        /// Gets the full path to the configuration file.
        /// </summary>
        public string ConfigFilePath => _configFilePath;
    }
}
