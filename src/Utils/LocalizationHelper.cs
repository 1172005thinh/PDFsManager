using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace PDFsManager.Utils
{
    /// <summary>
    /// Localization helper for loading language strings from JSON files.
    /// Supports fallback to hardcoded English strings if file is missing or corrupted.
    /// </summary>
    public class LocalizationHelper
    {
        private static JObject? _languageData;
        private static readonly object _lockObject = new object();
        private static bool _isInitialized = false;

        /// <summary>
        /// Initializes localization by loading language file.
        /// </summary>
        /// <param name="languageFile">Path to language JSON file (default: res/lang/en-us.json)</param>
        public static void Initialize(string? languageFile = null)
        {
            lock (_lockObject)
            {
                if (_isInitialized)
                    return;

                try
                {
                    string langPath = languageFile ?? Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "res", "lang", "en-us.json"
                    );

                    if (File.Exists(langPath))
                    {
                        string json = File.ReadAllText(langPath);
                        _languageData = JObject.Parse(json);
                    }
                }
                catch
                {
                    // Fallback to hardcoded strings
                    _languageData = null;
                }

                _isInitialized = true;
            }
        }

        /// <summary>
        /// Gets a localized string by path (e.g., "MainWindow.LogSection.Title").
        /// Returns fallback string if path not found.
        /// </summary>
        public static string Get(string path, string fallback = "")
        {
            if (!_isInitialized)
                Initialize();

            try
            {
                if (_languageData != null)
                {
                    JToken? token = _languageData.SelectToken(path);
                    if (token != null)
                        return token.ToString();
                }
            }
            catch
            {
                // Return fallback on error
            }

            return fallback;
        }

        /// <summary>
        /// Hardcoded English fallback strings for critical UI elements.
        /// </summary>
        public static class Fallback
        {
            public const string AppTitle = "PDFs Manager v1.0.0";
            public const string Start = "Start";
            public const string Stop = "Stop";
            public const string Help = "Help";
            public const string Exit = "Exit";
            public const string Cancel = "Cancel";
            public const string Browse = "Browse";
            public const string Refresh = "Refresh";
            public const string Clear = "Clear";
            public const string Close = "Close";
            public const string Workspace = "Workspace";
            public const string AutoStartup = "Enable Auto Startup";
            public const string StatusIdle = "Status: No workspace configured";
            public const string StatusStopped = "Status: Stopped";
            public const string StatusRunning = "Status: Running";
            public const string StatusError = "Status: Error";
        }
    }
}
