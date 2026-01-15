using System;
using System.Windows.Forms;
using PDFsManager.Core;
using PDFsManager.Models;
using PDFsManager.Utils;

namespace PDFsManager
{
    /// <summary>
    /// Application entry point (INIT component).
    /// Handles initialization, configuration loading, and GUI launch.
    /// </summary>
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Initialize logger
            Logger logger = new Logger();
            logger.Write(Constants.LOG_ACTION_START, "PDFsManager started.");

            // Load configuration
            ConfigManager configManager = new ConfigManager(logger);
            AppConfig config = configManager.Load();

            // Validate workspace
            bool isWorkspaceValid = configManager.ValidateWorkspace(config.Workspace);
            AppState initialState = isWorkspaceValid ? AppState.STOPPED : AppState.IDLE;

            logger.Write(Constants.LOG_ACTION_WORKSPACE, $"Workspace set to '{config.Workspace}'");
            
            if (initialState == AppState.IDLE)
            {
                logger.Write(Constants.LOG_ACTION_WORKSPACE, "Workspace is empty or invalid.");
            }
            else
            {
                logger.Write(Constants.LOG_ACTION_WORKSPACE, "Workspace is valid. Monitoring can be started.");
            }

            // TODO: Launch GUI
            // For now, just log that we're ready
            logger.Write(Constants.LOG_ACTION_INFO, "Application initialized successfully. GUI not yet implemented.");
            logger.Write(Constants.LOG_ACTION_STOP, "PDFsManager stopped.");

            // Keep console open for testing
            Console.WriteLine("PDFsManager Core components initialized successfully.");
            Console.WriteLine($"Log file: {logger.LogFilePath}");
            Console.WriteLine($"Config file: {configManager.ConfigFilePath}");
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
