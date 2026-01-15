using System;
using System.IO;

namespace PDFsManager.Utils
{
    /// <summary>
    /// Helper class for managing Windows startup functionality.
    /// Creates/removes shortcuts in the Startup folder.
    /// </summary>
    public static class StartupHelper
    {
        private const string STARTUP_ARG = "--autostart";
        
        /// <summary>
        /// Gets the path to the Windows Startup folder for the current user.
        /// </summary>
        private static string StartupFolderPath
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Startup),
                    "PDFsManager.lnk"
                );
            }
        }

        /// <summary>
        /// Gets the command line argument used to indicate auto-startup.
        /// </summary>
        public static string StartupArgument => STARTUP_ARG;

        /// <summary>
        /// Creates a startup shortcut for the application.
        /// </summary>
        /// <returns>True if successful, false otherwise.</returns>
        public static bool CreateStartupShortcut()
        {
            try
            {
                if (!OperatingSystem.IsWindows())
                    return false;

                string? exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
                if (string.IsNullOrEmpty(exePath))
                    return false;

                // Use IWshRuntimeLibrary to create shortcut
                Type? shellType = Type.GetTypeFromProgID("WScript.Shell");
                if (shellType == null)
                    return false;

                dynamic? shell = Activator.CreateInstance(shellType);
                if (shell == null)
                    return false;

                dynamic? shortcut = shell.CreateShortcut(StartupFolderPath);
                if (shortcut == null)
                    return false;
                
                shortcut.TargetPath = exePath;
                shortcut.Arguments = STARTUP_ARG;
                shortcut.WorkingDirectory = Path.GetDirectoryName(exePath) ?? "";
                shortcut.Description = "PDFs Manager - Automatic PDF Organizer";
                shortcut.Save();

                System.Runtime.InteropServices.Marshal.ReleaseComObject(shortcut);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(shell);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Removes the startup shortcut if it exists.
        /// </summary>
        /// <returns>True if successful or shortcut doesn't exist, false on error.</returns>
        public static bool RemoveStartupShortcut()
        {
            try
            {
                if (File.Exists(StartupFolderPath))
                {
                    File.Delete(StartupFolderPath);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if the startup shortcut exists.
        /// </summary>
        /// <returns>True if shortcut exists, false otherwise.</returns>
        public static bool StartupShortcutExists()
        {
            return File.Exists(StartupFolderPath);
        }
    }
}
