using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using PDFsManager.Utils;

namespace PDFsManager.UI
{
    /// <summary>
    /// Help and About dialog for PDFs Manager.
    /// Displays usage instructions, author info, and links.
    /// </summary>
    public class HelpDialog : Form
    {
        private TextBox _helpTextBox = null!;
        private Button _closeButton = null!;
        private LinkLabel _facebookLink = null!;
        private LinkLabel _githubLink = null!;
        private LinkLabel _licenseLink = null!;
        private Label _authorLabel = null!;
        private Label _versionLabel = null!;

        public HelpDialog()
        {
            InitializeComponent();
            SetupGUI();
        }

        private void SetupGUI()
        {
            // Window properties
            this.Text = LocalizationHelper.Get("HelpDialog.Title", "PDFs Manager Help");
            this.Size = new Size(600, 530);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = ThemeHelper.GetColor("Colors.Background", ThemeHelper.Fallback.Background);

            // Help content text box
            _helpTextBox = new TextBox
            {
                Location = new Point(10, 10),
                Size = new Size(560, 380),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                BackColor = ThemeHelper.GetColor("Colors.LogBackground", ThemeHelper.Fallback.LogBackground),
                ForeColor = ThemeHelper.GetColor("Colors.Text", ThemeHelper.Fallback.Text),
                Font = new Font(ThemeHelper.GetString("Fonts.FontFamily", ThemeHelper.Fallback.FontFamily), 
                               ThemeHelper.GetInt("Fonts.FontSizeNormal", ThemeHelper.Fallback.FontSizeNormal))
            };

            // Load help content
            string helpContent = LocalizationHelper.Get("HelpDialog.HelpContent", GetFallbackHelpContent());
            _helpTextBox.Text = helpContent;
            this.Controls.Add(_helpTextBox);

            // Separator line
            Panel separator = new Panel
            {
                Location = new Point(10, 400),
                Size = new Size(560, 1),
                BackColor = ThemeHelper.GetColor("Colors.Border", ThemeHelper.Fallback.Border)
            };
            this.Controls.Add(separator);

            // Author label
            _authorLabel = new Label
            {
                Text = LocalizationHelper.Get("HelpDialog.Author", "Author: 1172005thinh (QuickComp.)"),
                Location = new Point(10, 410),
                Size = new Size(300, 20),
                Font = new Font(ThemeHelper.GetString("Fonts.FontFamily", ThemeHelper.Fallback.FontFamily), 
                               ThemeHelper.GetInt("Fonts.FontSizeNormal", ThemeHelper.Fallback.FontSizeNormal))
            };
            this.Controls.Add(_authorLabel);

            // Contact links
            Label contactLabel = new Label
            {
                Text = "Contact:",
                Location = new Point(10, 433),
                Size = new Size(60, 20)
            };
            this.Controls.Add(contactLabel);

            _facebookLink = new LinkLabel
            {
                Text = LocalizationHelper.Get("HelpDialog.FacebookLink", "Facebook"),
                Location = new Point(75, 433),
                Size = new Size(70, 20),
                LinkColor = ThemeHelper.GetColor("Colors.Primary", ThemeHelper.Fallback.Primary)
            };
            _facebookLink.Click += (s, e) => OpenUrl("https://www.facebook.com/quickcomp.hungthinhnguyen/");
            this.Controls.Add(_facebookLink);

            _githubLink = new LinkLabel
            {
                Text = LocalizationHelper.Get("HelpDialog.GitHubLink", "GitHub"),
                Location = new Point(150, 433),
                Size = new Size(60, 20),
                LinkColor = ThemeHelper.GetColor("Colors.Primary", ThemeHelper.Fallback.Primary)
            };
            _githubLink.Click += (s, e) => OpenUrl("https://github.com/1172005thinh/PDFsManager");
            this.Controls.Add(_githubLink);

            _licenseLink = new LinkLabel
            {
                Text = LocalizationHelper.Get("HelpDialog.LicenseLink", "MIT License"),
                Location = new Point(215, 433),
                Size = new Size(100, 20),
                LinkColor = ThemeHelper.GetColor("Colors.Primary", ThemeHelper.Fallback.Primary)
            };
            _licenseLink.Click += (s, e) => OpenUrl("https://github.com/1172005thinh/PDFsManager/blob/dev/LICENSE");
            this.Controls.Add(_licenseLink);

            // Version label
            _versionLabel = new Label
            {
                Text = Constants.APP_VERSION,
                Location = new Point(10, 460),
                Size = new Size(100, 20),
                ForeColor = ThemeHelper.GetColor("Colors.TextSecondary", ThemeHelper.Fallback.TextSecondary)
            };
            this.Controls.Add(_versionLabel);

            // Close button
            _closeButton = new Button
            {
                Text = LocalizationHelper.Get("HelpDialog.CloseButton", LocalizationHelper.Fallback.Close),
                Location = new Point(495, 457),
                Size = new Size(75, 25)
            };
            _closeButton.Click += (s, e) => this.Close();
            this.Controls.Add(_closeButton);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(600, 500);
            this.Name = "HelpDialog";
            this.ResumeLayout(false);
        }

        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open URL: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetFallbackHelpContent()
        {
            return @"PDFs Manager - Help & Documentation

WHAT DOES THIS TOOL DO?
PDFs Manager automatically organizes your PDF files by their creation date. It monitors a workspace folder and moves files into organized year/month folders with clean, standardized names.

HOW TO USE:
1. Set Workspace: Click 'Browse' to select a folder containing your PDF files
2. Start Monitoring: Click 'Start' to begin automatic file processing
3. Auto Startup: Check the box to launch automatically with Windows

FILE PROCESSING:
• Files are renamed to: YYYY-MM-DD_HH-mm-ss.pdf
• Organized into: Workspace/YYYY/MM/
• Example: invoice.pdf → 2026-01-15_16-30-05.pdf in 2026/01/

FEATURES:
✓ Real-time monitoring with FileSystemWatcher
✓ Automatic folder creation
✓ Duplicate file handling (_001, _002, etc.)
✓ File lock retry mechanism
✓ Comprehensive error logging

TROUBLESHOOTING:
• File not processed? Check if it has valid PDF metadata
• Workspace invalid? Ensure the folder exists and you have permissions
• Files locked? Wait for downloads to complete

REQUIREMENTS:
• Windows 10/11
• .NET 8.0 Runtime
• Read/Write permissions for workspace

CONTACT & SUPPORT:
For bugs, suggestions, or questions:
• GitHub: github.com/1172005thinh/PDFsManager
• Facebook: facebook.com/quickcomp.hungthinhnguyen";
        }
    }
}
