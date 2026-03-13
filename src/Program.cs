using System;
using System.Threading;
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
        static void Main(string[] args)
        {
            // Single instance check
            bool createdNew;
            using (Mutex mutex = new Mutex(true, "PDFsManager_SingleInstance_Mutex", out createdNew))
            {
                if (!createdNew)
                {
                    MessageBox.Show("PDFs Manager is already running.", "PDFs Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Check if launched at startup
                bool isAutoStart = args.Length > 0 && args[0] == StartupHelper.StartupArgument;

            // Initialize logger
            Logger logger = new Logger();
            logger.Write(Constants.LOG_ACTION_START, "PDFsManager started.");

            // Load configuration
            ConfigManager configManager = new ConfigManager(logger);
            AppConfig config = configManager.Load();

            // Validate workspace
            bool isWorkspaceValid = configManager.ValidateWorkspaces(config.Workspaces);
            AppState initialState = isWorkspaceValid ? AppState.STOPPED : AppState.IDLE;

            logger.Write(Constants.LOG_ACTION_WORKSPACE, $"Workspaces set to '{string.Join(", ", config.Workspaces)}'");

            if (initialState == AppState.IDLE)
            {
                logger.Write(Constants.LOG_ACTION_WORKSPACE, "Workspaces are empty or invalid.");
            }
            else
            {
                logger.Write(Constants.LOG_ACTION_WORKSPACE, "Workspaces are valid. Monitoring can be started.");
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
                // If auto-started, start monitoring and minimize to tray
                if (isAutoStart && config.AutoStartup && isWorkspaceValid)
                {
                    mainForm.Load += (s, e) =>
                    {
                        mainForm.AutoStartMonitoring();
                    };
                }
                
                Application.Run(mainForm);
            }

                logger.Write(Constants.LOG_ACTION_STOP, "PDFsManager terminated.");
                GC.KeepAlive(mutex);
            }
        }
    }
}
