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
        private TextBox _workspaceTextBox = null!;
        private Button _browseButton = null!;
        private CheckBox _autoStartupCheckBox = null!;
        private Button _startStopButton = null!;
        private Button _helpButton = null!;
        private Button _exitButton = null!;
        private Button _cancelButton = null!;
        private Label _statusLabel = null!;

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

            _workspaceTextBox = new TextBox
            {
                Location = new Point(85, 233),
                Size = new Size(400, 23),
                ReadOnly = true
            };
            _workspaceTextBox.TextChanged += (s, e) => _configDirty = true;
            this.Controls.Add(_workspaceTextBox);

            _browseButton = new Button
            {
                Text = LocalizationHelper.Get("MainWindow.ConfigSection.BrowseButton", LocalizationHelper.Fallback.Browse),
                Location = new Point(490, 232),
                Size = new Size(80, 25)
            };
            _browseButton.Click += BrowseButton_Click;
            this.Controls.Add(_browseButton);

            _autoStartupCheckBox = new CheckBox
            {
                Text = LocalizationHelper.Get("MainWindow.ConfigSection.AutoStartupCheckbox", LocalizationHelper.Fallback.AutoStartup),
                Location = new Point(10, 265),
                Size = new Size(300, 20)
            };
            _autoStartupCheckBox.CheckedChanged += (s, e) => _configDirty = true;
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
            _workspaceTextBox.Text = _currentConfig.Workspace;
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
                    _startStopButton.Enabled = !string.IsNullOrEmpty(_currentConfig.Workspace);
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

        private void BrowseButton_Click(object? sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = LocalizationHelper.Get("Messages.SelectWorkspaceTitle", "Select Workspace Folder");
                dialog.SelectedPath = string.IsNullOrEmpty(_currentConfig.Workspace) 
                    ? Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                    : _currentConfig.Workspace;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    if (_currentState == AppState.RUNNING)
                    {
                        _fileMonitor.Stop();
                        _currentState = AppState.STOPPED;
                        UpdateStatus();
                    }

                    _workspaceTextBox.Text = dialog.SelectedPath;
                    _currentConfig.Workspace = dialog.SelectedPath;
                    _configDirty = true;
                }
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
                    if (!_configManager.ValidateWorkspace(_currentConfig.Workspace))
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

                if (_fileMonitor.Start(_currentConfig.Workspace))
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
                if (_configManager.ValidateWorkspace(_currentConfig.Workspace))
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
                _workspaceTextBox.Text = _currentConfig.Workspace;
                _autoStartupCheckBox.Checked = _currentConfig.AutoStartup;
                _configDirty = false;
                _logger.Write(Constants.LOG_ACTION_CONFIG, "Configuration changes discarded.");
            }

            Application.Exit();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_currentState == AppState.RUNNING)
            {
                _fileMonitor.Stop();
            }
            _fileMonitor.Dispose();
            base.OnFormClosing(e);
        }
    }
}
