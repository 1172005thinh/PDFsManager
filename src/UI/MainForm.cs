using System;
using System.Drawing;
using System.Windows.Forms;
using PDFsManager.Core;
using PDFsManager.Models;
using PDFsManager.Utils;

namespace PDFsManager.UI
{
    /// <summary>
    /// Main application form for PDFs Manager.
    /// Implements the GUI workflow as specified in PROMPT.md.
    /// </summary>
    public partial class MainForm : Form
    {
        private readonly Logger _logger;
        private readonly ConfigManager _configManager;
        private readonly FileProcessor _fileProcessor;
        private readonly FileMonitor _fileMonitor;
        
        private AppConfig _currentConfig;
        private AppState _currentState;
        private bool _configDirty = false;

        // UI Controls
        private TextBox _logTextBox = null!;
        private Button _refreshButton = null!;
        private Button _clearButton = null!;
        private ListBox _workspacesListBox = null!;
        private Button _addButton = null!;
        private Button _removeButton = null!;
        private CheckBox _autoStartupCheckBox = null!;
        private Button _startStopButton = null!;
        private Button _helpButton = null!;
        private Button _exitButton = null!;
        private Button _cancelButton = null!;
        private Label _statusLabel = null!;
        private NotifyIcon _notifyIcon = null!;
        private ToolStripMenuItem _trayStartItem = null!;
        private ToolStripMenuItem _trayStopItem = null!;

        public MainForm(Logger logger, ConfigManager configManager, FileProcessor fileProcessor, 
                       FileMonitor fileMonitor, AppConfig config, AppState initialState)
        {
            _logger = logger;
            _configManager = configManager;
            _fileProcessor = fileProcessor;
            _fileMonitor = fileMonitor;
            _currentConfig = config;
            _currentState = initialState;

            // Initialize localization and theme
            LocalizationHelper.Initialize();
            ThemeHelper.Initialize();

            InitializeComponent();
            SetupGUI();
            LoadConfiguration();
            UpdateStatus();

            // Setup logger GUI callback
            _logger.SetGuiCallback(AppendLogToGui);
        }

        private void SetupGUI()
        {
            // Window properties
            this.Text = LocalizationHelper.Get("AppTitle", LocalizationHelper.Fallback.AppTitle);
            this.Size = new Size(600, 410);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = ThemeHelper.GetColor("Colors.Background", ThemeHelper.Fallback.Background);

            // Set app icon
            try
            {
                string iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "res", "icon", "app256.ico");
                if (System.IO.File.Exists(iconPath))
                {
                    this.Icon = new Icon(iconPath);
                }
            }
            catch { /* Ignore icon loading errors */ }

            // Setup system tray icon for background operation
            SetupSystemTray();

            // Log section
            Label logLabel = new Label
            {
                Text = LocalizationHelper.Get("MainWindow.LogSection.Title", "Log"),
                Location = new Point(10, 10),
                Size = new Size(100, 20),
                Font = new Font(ThemeHelper.GetString("Fonts.FontFamily", ThemeHelper.Fallback.FontFamily), 
                               ThemeHelper.GetInt("Fonts.FontSizeLarge", ThemeHelper.Fallback.FontSizeLarge), FontStyle.Bold)
            };
            this.Controls.Add(logLabel);

            _logTextBox = new TextBox
            {
                Location = new Point(10, 35),
                Size = new Size(560, 150),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                BackColor = ThemeHelper.GetColor("Colors.LogBackground", ThemeHelper.Fallback.LogBackground),
                ForeColor = ThemeHelper.GetColor("Colors.Text", ThemeHelper.Fallback.Text),
                Font = new Font(ThemeHelper.GetString("Fonts.FontFamilyMono", ThemeHelper.Fallback.FontFamilyMono), 9f)
            };
            this.Controls.Add(_logTextBox);

            _refreshButton = new Button
            {
                Text = LocalizationHelper.Get("MainWindow.LogSection.RefreshButton", LocalizationHelper.Fallback.Refresh),
                Location = new Point(10, 190),
                Size = new Size(80, 25)
            };
            _refreshButton.Click += RefreshButton_Click;
            this.Controls.Add(_refreshButton);

            _clearButton = new Button
            {
                Text = LocalizationHelper.Get("MainWindow.LogSection.ClearButton", LocalizationHelper.Fallback.Clear),
                Location = new Point(95, 190),
                Size = new Size(80, 25)
            };
            _clearButton.Click += ClearButton_Click;
            this.Controls.Add(_clearButton);

            // Separator line
            Panel separator1 = new Panel
            {
                Location = new Point(10, 225),
                Size = new Size(560, 1),
                BackColor = ThemeHelper.GetColor("Colors.Border", ThemeHelper.Fallback.Border)
            };
            this.Controls.Add(separator1);

            // Configuration section
            Label workspaceLabel = new Label
            {
                Text = LocalizationHelper.Get("MainWindow.ConfigSection.WorkspaceLabel", LocalizationHelper.Fallback.Workspace),
                Location = new Point(10, 235),
                Size = new Size(70, 20)
            };
            this.Controls.Add(workspaceLabel);

            _workspacesListBox = new ListBox
            {
                Location = new Point(85, 233),
                Size = new Size(400, 30),
                SelectionMode = SelectionMode.One
            };
            this.Controls.Add(_workspacesListBox);

            _addButton = new Button
            {
                Text = "+",
                Location = new Point(490, 232),
                Size = new Size(35, 25)
            };
            _addButton.Click += AddButton_Click;
            this.Controls.Add(_addButton);

            _removeButton = new Button
            {
                Text = "-",
                Location = new Point(530, 232),
                Size = new Size(35, 25)
            };
            _removeButton.Click += RemoveButton_Click;
            this.Controls.Add(_removeButton);

            _autoStartupCheckBox = new CheckBox
            {
                Text = LocalizationHelper.Get("MainWindow.ConfigSection.AutoStartupCheckbox", LocalizationHelper.Fallback.AutoStartup),
                Location = new Point(10, 265),
                Size = new Size(300, 20)
            };
            _autoStartupCheckBox.CheckedChanged += AutoStartupCheckBox_CheckedChanged;
            this.Controls.Add(_autoStartupCheckBox);

            // Separator line
            Panel separator2 = new Panel
            {
                Location = new Point(10, 295),
                Size = new Size(560, 1),
                BackColor = ThemeHelper.GetColor("Colors.Border", ThemeHelper.Fallback.Border)
            };
            this.Controls.Add(separator2);

            // Control buttons
            _startStopButton = new Button
            {
                Text = LocalizationHelper.Get("MainWindow.ControlButtons.StartButton", LocalizationHelper.Fallback.Start),
                Location = new Point(10, 305),
                Size = new Size(80, 25)
            };
            _startStopButton.Click += StartStopButton_Click;
            this.Controls.Add(_startStopButton);

            _helpButton = new Button
            {
                Text = LocalizationHelper.Get("MainWindow.ControlButtons.HelpButton", LocalizationHelper.Fallback.Help),
                Location = new Point(95, 305),
                Size = new Size(80, 25)
            };
            _helpButton.Click += HelpButton_Click;
            this.Controls.Add(_helpButton);

            _exitButton = new Button
            {
                Text = LocalizationHelper.Get("MainWindow.ControlButtons.ExitButton", LocalizationHelper.Fallback.Exit),
                Location = new Point(410, 305),
                Size = new Size(80, 25)
            };
            _exitButton.Click += ExitButton_Click;
            this.Controls.Add(_exitButton);

            _cancelButton = new Button
            {
                Text = LocalizationHelper.Get("MainWindow.ControlButtons.CancelButton", LocalizationHelper.Fallback.Cancel),
                Location = new Point(495, 305),
                Size = new Size(75, 25)
            };
            _cancelButton.Click += CancelButton_Click;
            this.Controls.Add(_cancelButton);

            // Separator line
            Panel separator3 = new Panel
            {
                Location = new Point(10, 340),
                Size = new Size(560, 1),
                BackColor = ThemeHelper.GetColor("Colors.Border", ThemeHelper.Fallback.Border)
            };
            this.Controls.Add(separator3);

            // Status label
            _statusLabel = new Label
            {
                Text = LocalizationHelper.Get("MainWindow.StatusLabel.Idle", LocalizationHelper.Fallback.StatusIdle),
                Location = new Point(10, 345),
                Size = new Size(560, 20),
                Font = new Font(ThemeHelper.GetString("Fonts.FontFamily", ThemeHelper.Fallback.FontFamily), 
                               ThemeHelper.GetInt("Fonts.FontSizeLarge", ThemeHelper.Fallback.FontSizeLarge), FontStyle.Bold)
            };
            this.Controls.Add(_statusLabel);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(600, 400);
            this.Name = "MainForm";
            this.ResumeLayout(false);
        }

        private void LoadConfiguration()
        {
            _workspacesListBox.Items.Clear();
            if (_currentConfig.Workspaces != null)
            {
                foreach(var ws in _currentConfig.Workspaces)
                {
                    _workspacesListBox.Items.Add(ws);
                }
            }
            _autoStartupCheckBox.Checked = _currentConfig.AutoStartup;
            _configDirty = false;

            // Load existing logs
            string existingLogs = _logger.ReadLog();
            if (!string.IsNullOrEmpty(existingLogs))
            {
                _logTextBox.Text = existingLogs;
                _logTextBox.SelectionStart = _logTextBox.Text.Length;
                _logTextBox.ScrollToCaret();
            }
        }

        private void UpdateStatus()
        {
            string statusText;
            Color statusColor;

            switch (_currentState)
            {
                case AppState.IDLE:
                    statusText = LocalizationHelper.Get("MainWindow.StatusLabel.Idle", LocalizationHelper.Fallback.StatusIdle);
                    statusColor = ThemeHelper.GetColor("Colors.StatusIdle", ThemeHelper.Fallback.StatusIdle);
                    _startStopButton.Text = LocalizationHelper.Get("MainWindow.ControlButtons.StartButton", LocalizationHelper.Fallback.Start);
                    _startStopButton.Enabled = _currentConfig.Workspaces != null && _currentConfig.Workspaces.Count > 0;
                    break;

                case AppState.STOPPED:
                    statusText = LocalizationHelper.Get("MainWindow.StatusLabel.Stopped", LocalizationHelper.Fallback.StatusStopped);
                    statusColor = ThemeHelper.GetColor("Colors.StatusStopped", ThemeHelper.Fallback.StatusStopped);
                    _startStopButton.Text = LocalizationHelper.Get("MainWindow.ControlButtons.StartButton", LocalizationHelper.Fallback.Start);
                    _startStopButton.Enabled = true;
                    break;

                case AppState.RUNNING:
                    statusText = LocalizationHelper.Get("MainWindow.StatusLabel.Running", LocalizationHelper.Fallback.StatusRunning);
                    statusColor = ThemeHelper.GetColor("Colors.StatusRunning", ThemeHelper.Fallback.StatusRunning);
                    _startStopButton.Text = LocalizationHelper.Get("MainWindow.ControlButtons.StopButton", LocalizationHelper.Fallback.Stop);
                    _startStopButton.Enabled = true;
                    break;

                case AppState.ERROR:
                    statusText = LocalizationHelper.Get("MainWindow.StatusLabel.Error", LocalizationHelper.Fallback.StatusError);
                    statusColor = ThemeHelper.GetColor("Colors.StatusError", ThemeHelper.Fallback.StatusError);
                    _startStopButton.Enabled = false;
                    break;

                default:
                    statusText = "Status: Unknown";
                    statusColor = ThemeHelper.Fallback.Text;
                    break;
            }

            _statusLabel.Text = statusText;
            _statusLabel.ForeColor = statusColor;
            _addButton.Enabled = (_currentState != AppState.RUNNING);
            _removeButton.Enabled = (_currentState != AppState.RUNNING);

            // Update tray menu items
            UpdateTrayMenu();
        }

        private void AppendLogToGui(string logEntry)
        {
            if (_logTextBox.InvokeRequired)
            {
                _logTextBox.BeginInvoke(new Action(() => AppendLogToGui(logEntry)));
                return;
            }

            _logTextBox.AppendText(logEntry + Environment.NewLine);
            _logTextBox.SelectionStart = _logTextBox.Text.Length;
            _logTextBox.ScrollToCaret();
        }

        // Event Handlers
        private void AutoStartupCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            _configDirty = true;
            _currentConfig.AutoStartup = _autoStartupCheckBox.Checked;

            // Update startup shortcut
            if (_autoStartupCheckBox.Checked)
            {
                if (!StartupHelper.CreateStartupShortcut())
                {
                    _logger.Write(Constants.LOG_ACTION_WARN, "Failed to create startup shortcut.");
                }
                else
                {
                    _logger.Write(Constants.LOG_ACTION_CONFIG, "Auto-startup enabled.");
                }
            }
            else
            {
                if (!StartupHelper.RemoveStartupShortcut())
                {
                    _logger.Write(Constants.LOG_ACTION_WARN, "Failed to remove startup shortcut.");
                }
                else
                {
                    _logger.Write(Constants.LOG_ACTION_CONFIG, "Auto-startup disabled.");
                }
            }
        }

        private void RefreshButton_Click(object? sender, EventArgs e)
        {
            string logs = _logger.ReadLog();
            _logTextBox.Text = logs;
            _logTextBox.SelectionStart = _logTextBox.Text.Length;
            _logTextBox.ScrollToCaret();
        }

        private void ClearButton_Click(object? sender, EventArgs e)
        {
            _logTextBox.Clear();
        }

        private void AddButton_Click(object? sender, EventArgs e)
        {
            if (_currentConfig.Workspaces.Count >= 8)
            {
                MessageBox.Show("Maximum 8 workspaces allowed.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = LocalizationHelper.Get("Messages.SelectWorkspaceTitle", "Select Workspace Folder");
                
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    if (_currentConfig.Workspaces.Contains(dialog.SelectedPath))
                        return;

                    if (_currentState == AppState.RUNNING)
                    {
                        _fileMonitor.Stop();
                        _currentState = AppState.STOPPED;
                        UpdateStatus();
                    }

                    _currentConfig.Workspaces.Add(dialog.SelectedPath);
                    _workspacesListBox.Items.Add(dialog.SelectedPath);
                    _configDirty = true;
                    UpdateStatus();
                }
            }
        }

        private void RemoveButton_Click(object? sender, EventArgs e)
        {
            if (_workspacesListBox.SelectedItem is string selectedPath)
            {
                if (_currentState == AppState.RUNNING)
                {
                    _fileMonitor.Stop();
                    _currentState = AppState.STOPPED;
                    UpdateStatus();
                }

                _currentConfig.Workspaces.Remove(selectedPath);
                _workspacesListBox.Items.Remove(selectedPath);
                _configDirty = true;
                UpdateStatus();
            }
        }

        private void StartStopButton_Click(object? sender, EventArgs e)
        {
            if (_currentState == AppState.RUNNING)
            {
                // Stop monitoring
                _fileMonitor.Stop();
                _currentState = AppState.STOPPED;
                UpdateStatus();
            }
            else
            {
                // Start monitoring
                if (_configDirty)
                {
                    if (!_configManager.ValidateWorkspaces(_currentConfig.Workspaces))
                    {
                        MessageBox.Show(
                            LocalizationHelper.Get("Messages.WorkspaceInvalid", "Invalid workspace directory."),
                            this.Text,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                        return;
                    }

                    if (!_configManager.Save(_currentConfig))
                    {
                        MessageBox.Show(
                            LocalizationHelper.Get("Messages.ConfigSaveFailed", "Failed to save configuration."),
                            this.Text,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                        return;
                    }

                    _configDirty = false;
                }

                if (_fileMonitor.Start(_currentConfig.Workspaces))
                {
                    _currentState = AppState.RUNNING;
                    UpdateStatus();
                }
                else
                {
                    _currentState = AppState.ERROR;
                    UpdateStatus();
                    MessageBox.Show(
                        LocalizationHelper.Get("Messages.MonitorStartFailed", "Failed to start monitoring."),
                        this.Text,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
        }

        private void HelpButton_Click(object? sender, EventArgs e)
        {
            using (HelpDialog helpDialog = new HelpDialog())
            {
                this.Enabled = false;
                helpDialog.ShowDialog(this);
                this.Enabled = true;
            }
        }

        private void ExitButton_Click(object? sender, EventArgs e)
        {
            if (_currentState == AppState.RUNNING)
            {
                _fileMonitor.Stop();
                _logger.Write(Constants.LOG_ACTION_STOP, "Monitoring stopped (application exiting).");
            }

            if (_configDirty)
            {
                if (_configManager.ValidateWorkspaces(_currentConfig.Workspaces))
                {
                    _configManager.Save(_currentConfig);
                }
                else
                {
                    var result = MessageBox.Show(
                        LocalizationHelper.Get("Messages.ConfirmExit", "Exit without saving?"),
                        LocalizationHelper.Get("Messages.ConfirmExitTitle", "Unsaved Changes"),
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning
                    );

                    if (result == DialogResult.No)
                        return;
                }
            }

            _logger.Write(Constants.LOG_ACTION_STOP, "PDFsManager stopped.");
            Application.Exit();
        }

        private void CancelButton_Click(object? sender, EventArgs e)
        {
            if (_configDirty)
            {
                _currentConfig = _configManager.Load();
                _workspacesListBox.Items.Clear();
                if (_currentConfig.Workspaces != null)
                {
                    foreach(var ws in _currentConfig.Workspaces)
                    {
                        _workspacesListBox.Items.Add(ws);
                    }
                }
                _autoStartupCheckBox.Checked = _currentConfig.AutoStartup;
                _configDirty = false;
                _logger.Write(Constants.LOG_ACTION_CONFIG, "Configuration changes discarded.");
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // If monitoring is running, minimize to tray instead of closing
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
                _notifyIcon.Visible = true;
                _notifyIcon.ShowBalloonTip(2000, "PDFs Manager", 
                    "Application minimized to system tray.", 
                    ToolTipIcon.Info);
                return;
            }

            if (_currentState == AppState.RUNNING)
            {
                _fileMonitor.Stop();
            }
            
            _notifyIcon?.Dispose();
            _fileMonitor.Dispose();
            base.OnFormClosing(e);
        }

        private void SetupSystemTray()
        {
            _notifyIcon = new NotifyIcon();
            
            // Set tray icon - try to load from file, fallback to application icon
            try
            {
                string iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "res", "icon", "app256.ico");
                if (System.IO.File.Exists(iconPath))
                {
                    _notifyIcon.Icon = new Icon(iconPath);
                }
                else if (this.Icon != null)
                {
                    _notifyIcon.Icon = this.Icon;
                }
                else
                {
                    _notifyIcon.Icon = SystemIcons.Application;
                }
            }
            catch
            {
                // Fallback to default system icon
                _notifyIcon.Icon = SystemIcons.Application;
            }

            _notifyIcon.Text = "PDFs Manager";
            _notifyIcon.Visible = true;  // Always visible for system tray operation

            // Double-click to restore window
            _notifyIcon.DoubleClick += (s, e) =>
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                this.BringToFront();
                _notifyIcon.Visible = false;
            };

            // Context menu for tray icon
            ContextMenuStrip trayMenu = new ContextMenuStrip();
            
            ToolStripMenuItem showItem = new ToolStripMenuItem("Show Window");
            showItem.Click += (s, e) =>
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                this.BringToFront();
                _notifyIcon.Visible = false;
            };
            trayMenu.Items.Add(showItem);

            _trayStartItem = new ToolStripMenuItem("Start Monitoring");
            _trayStartItem.Click += (s, e) =>
            {
                if (_currentState != AppState.RUNNING && _configManager.ValidateWorkspaces(_currentConfig.Workspaces))
                {
                    if (_fileMonitor.Start(_currentConfig.Workspaces))
                    {
                        _currentState = AppState.RUNNING;
                        UpdateStatus();
                        _notifyIcon.ShowBalloonTip(2000, "PDFs Manager", 
                            "Monitoring started.", ToolTipIcon.Info);
                    }
                    else
                    {
                        _currentState = AppState.ERROR;
                        UpdateStatus();
                        _notifyIcon.ShowBalloonTip(2000, "PDFs Manager", 
                            "Failed to start monitoring.", ToolTipIcon.Error);
                    }
                }
            };
            trayMenu.Items.Add(_trayStartItem);

            _trayStopItem = new ToolStripMenuItem("Stop Monitoring");
            _trayStopItem.Click += (s, e) =>
            {
                if (_currentState == AppState.RUNNING)
                {
                    _fileMonitor.Stop();
                    _currentState = AppState.STOPPED;
                    UpdateStatus();
                    _notifyIcon.ShowBalloonTip(2000, "PDFs Manager", 
                        "Monitoring stopped.", ToolTipIcon.Info);
                }
            };
            trayMenu.Items.Add(_trayStopItem);

            trayMenu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem exitItem = new ToolStripMenuItem("Exit");
            exitItem.Click += (s, e) =>
            {
                _notifyIcon.Visible = false;
                if (_currentState == AppState.RUNNING)
                {
                    _fileMonitor.Stop();
                }
                Application.Exit();
            };
            trayMenu.Items.Add(exitItem);

            _notifyIcon.ContextMenuStrip = trayMenu;
            
            // Initialize tray menu state
            UpdateTrayMenu();
        }

        private void UpdateTrayMenu()
        {
            if (_trayStartItem == null || _trayStopItem == null)
                return;

            // Show/enable appropriate menu items based on current state
            switch (_currentState)
            {
                case AppState.IDLE:
                case AppState.STOPPED:
                    _trayStartItem.Visible = true;
                    _trayStartItem.Enabled = _configManager.ValidateWorkspaces(_currentConfig.Workspaces);
                    _trayStopItem.Visible = false;
                    break;

                case AppState.RUNNING:
                    _trayStartItem.Visible = false;
                    _trayStopItem.Visible = true;
                    _trayStopItem.Enabled = true;
                    break;

                case AppState.ERROR:
                    _trayStartItem.Visible = true;
                    _trayStartItem.Enabled = false;
                    _trayStopItem.Visible = false;
                    break;
            }
        }

        /// <summary>
        /// Public method to auto-start monitoring and minimize to tray.
        /// Called when application is launched at Windows startup.
        /// </summary>
        public void AutoStartMonitoring()
        {
            if (_currentState != AppState.RUNNING && _configManager.ValidateWorkspaces(_currentConfig.Workspaces))
            {
                if (_fileMonitor.Start(_currentConfig.Workspaces))
                {
                    _currentState = AppState.RUNNING;
                    UpdateStatus();
                    _logger.Write(Constants.LOG_ACTION_START, "Auto-started monitoring at Windows startup.");
                    
                    // Minimize to tray
                    this.WindowState = FormWindowState.Minimized;
                    this.Hide();
                    _notifyIcon.Visible = true;
                    _notifyIcon.ShowBalloonTip(3000, "PDFs Manager", 
                        "Auto-started and minimized to tray. Monitoring is active.", 
                        ToolTipIcon.Info);
                }
                else
                {
                    _currentState = AppState.ERROR;
                    UpdateStatus();
                    _logger.Write(Constants.LOG_ACTION_ERROR, "Failed to auto-start monitoring.");
                }
            }
        }
    }
}
