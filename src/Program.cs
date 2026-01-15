using System;
using System.Windows.Forms;
using PDFsManager.Core;
using PDFsManager.Models;
using PDFsManager.Utils;
using PDFsManager.UI;

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

            // Initialize localization and theming
            LocalizationHelper.Initialize();
            ThemeHelper.Initialize();

            // Initialize Core components
            FileProcessor fileProcessor = new FileProcessor(logger);
            FileMonitor fileMonitor = new FileMonitor(logger, fileProcessor);

            // Launch GUI
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            using (MainForm mainForm = new MainForm(logger, configManager, fileProcessor, fileMonitor, config, initialState))
            {
                Application.Run(mainForm);
            }

            // Cleanup on exit
            logger.Write(Constants.LOG_ACTION_STOP, "PDFsManager stopped.");
            Console.WriteLine($"Log file: {logger.LogFilePath}");
            Console.WriteLine($"Config file: {configManager.ConfigFilePath}");
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
