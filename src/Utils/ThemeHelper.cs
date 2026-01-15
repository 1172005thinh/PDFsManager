using System;
using System.Drawing;
using System.IO;
using Newtonsoft.Json.Linq;

namespace PDFsManager.Utils
{
    /// <summary>
    /// Theme helper for loading colors and font styles from JSON files.
    /// Supports fallback to hardcoded Light theme if file is missing or corrupted.
    /// </summary>
    public class ThemeHelper
    {
        private static JObject? _themeData;
        private static readonly object _lockObject = new object();
        private static bool _isInitialized = false;

        /// <summary>
        /// Initializes theme by loading theme file.
        /// </summary>
        /// <param name="themeFile">Path to theme JSON file (default: res/theme/light.json)</param>
        public static void Initialize(string? themeFile = null)
        {
            lock (_lockObject)
            {
                if (_isInitialized)
                    return;

                try
                {
                    string themePath = themeFile ?? Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "res", "theme", "light.json"
                    );

                    if (File.Exists(themePath))
                    {
                        string json = File.ReadAllText(themePath);
                        _themeData = JObject.Parse(json);
                    }
                }
                catch
                {
                    // Fallback to hardcoded theme
                    _themeData = null;
                }

                _isInitialized = true;
            }
        }

        /// <summary>
        /// Gets a color by path (e.g., "Colors.Primary").
        /// Returns fallback color if path not found.
        /// </summary>
        public static Color GetColor(string path, Color fallback)
        {
            if (!_isInitialized)
                Initialize();

            try
            {
                if (_themeData != null)
                {
                    JToken? token = _themeData.SelectToken(path);
                    if (token != null)
                    {
                        string hexColor = token.ToString();
                        return ColorTranslator.FromHtml(hexColor);
                    }
                }
            }
            catch
            {
                // Return fallback on error
            }

            return fallback;
        }

        /// <summary>
        /// Gets an integer value by path (e.g., "Fonts.FontSizeNormal").
        /// </summary>
        public static int GetInt(string path, int fallback)
        {
            if (!_isInitialized)
                Initialize();

            try
            {
                if (_themeData != null)
                {
                    JToken? token = _themeData.SelectToken(path);
                    if (token != null && token.Type == JTokenType.Integer)
                        return token.Value<int>();
                }
            }
            catch
            {
                // Return fallback on error
            }

            return fallback;
        }

        /// <summary>
        /// Gets a string value by path (e.g., "Fonts.FontFamily").
        /// </summary>
        public static string GetString(string path, string fallback)
        {
            if (!_isInitialized)
                Initialize();

            try
            {
                if (_themeData != null)
                {
                    JToken? token = _themeData.SelectToken(path);
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
        /// Hardcoded Light theme fallback values.
        /// </summary>
        public static class Fallback
        {
            // Colors
            public static readonly Color Background = Color.White;
            public static readonly Color Surface = ColorTranslator.FromHtml("#F5F5F5");
            public static readonly Color Primary = ColorTranslator.FromHtml("#0078D4");
            public static readonly Color Text = ColorTranslator.FromHtml("#212529");
            public static readonly Color TextSecondary = ColorTranslator.FromHtml("#6C757D");
            public static readonly Color Border = ColorTranslator.FromHtml("#DEE2E6");
            public static readonly Color LogBackground = ColorTranslator.FromHtml("#FAFAFA");
            public static readonly Color StatusRunning = ColorTranslator.FromHtml("#28A745");
            public static readonly Color StatusStopped = ColorTranslator.FromHtml("#FFC107");
            public static readonly Color StatusError = ColorTranslator.FromHtml("#DC3545");
            public static readonly Color StatusIdle = ColorTranslator.FromHtml("#6C757D");

            // Fonts
            public const string FontFamily = "Segoe UI";
            public const string FontFamilyMono = "Consolas";
            public const int FontSizeNormal = 10;
            public const int FontSizeLarge = 11;
        }
    }
}
