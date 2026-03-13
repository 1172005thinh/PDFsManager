using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PDFsManager.Models;
using PDFsManager.Utils;

namespace PDFsManager.Core
{
    /// <summary>
    /// Core file processing logic for PDFs Manager.
    /// Handles scanning workspace, processing individual PDF files, renaming, and moving.
    /// </summary>
    public class FileProcessor
    {
        private readonly Logger _logger;
        private List<string> _workspacePaths = new List<string>();

        /// <summary>
        /// Creates a new FileProcessor instance.
        /// </summary>
        /// <param name="logger">Logger instance for logging operations.</param>
        public FileProcessor(Logger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Sets the workspace paths for file processing.
        /// </summary>
        public void SetWorkspaces(List<string> workspacePaths)
        {
            _workspacePaths = workspacePaths ?? new List<string>();
        }

        /// <summary>
        /// Scans the workspace directories and processes all PDF files found.
        /// Non-recursive - ignores subdirectories.
        /// </summary>
        public void ScanAndProcess()
        {
            if (_workspacePaths == null || _workspacePaths.Count == 0)
            {
                _logger.Write(Constants.LOG_ACTION_ERROR, "Workspaces are not set.");
                return;
            }

            try
            {
                var allPdfFiles = new List<string>();
                foreach (var workspacePath in _workspacePaths)
                {
                    if (Directory.Exists(workspacePath))
                    {
                        allPdfFiles.AddRange(Directory.GetFiles(workspacePath, Constants.PDF_FILTER, SearchOption.TopDirectoryOnly));
                    }
                }

                if (allPdfFiles.Count == 0)
                {
                    _logger.Write(Constants.LOG_ACTION_SCAN, "Nothing to process.");
                    return;
                }

                _logger.Write(Constants.LOG_ACTION_SCAN, $"Found {allPdfFiles.Count} file(s) to process.");
                _logger.WriteSeparator(major: true);

                foreach (string filePath in allPdfFiles)
                {
                    ProcessFile(filePath);
                    _logger.WriteSeparator(major: false);
                }

                _logger.WriteSeparator(major: true);
            }
            catch (Exception ex)
            {
                _logger.Write(Constants.LOG_ACTION_ERROR, $"Scan failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Processes a single PDF file: extracts metadata, renames, and moves to organized folder.
        /// </summary>
        /// <param name="filePath">Full path to the PDF file to process.</param>
        /// <returns>ProcessResult indicating success or failure.</returns>
        public ProcessResult ProcessFile(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            _logger.Write(Constants.LOG_ACTION_FILE, $"Processing file '{fileName}'");

            try
            {
                // Step 1: Check if file is locked/in use
                bool unlocked = FileHelper.TryWithRetry(filePath, () => { }, 
                    (attempt) => _logger.Write(Constants.LOG_ACTION_WARN, $"'{fileName}' - File is locked or in use. Retry {attempt}/{Constants.FILE_LOCK_RETRY_COUNT}..."));

                if (!unlocked)
                {
                    string errorMsg = $"'{fileName}' - File remains locked after {Constants.FILE_LOCK_RETRY_COUNT} retries. Skipping file.";
                    _logger.Write(Constants.LOG_ACTION_ERROR, errorMsg);
                    _logger.Write(Constants.LOG_ACTION_DONE, $"'{fileName}' - Processing completed with errors.");
                    return ProcessResult.CreateFailure(filePath, "File locked");
                }

                // Step 2: Extract PDF creation date metadata
                _logger.Write(Constants.LOG_ACTION_READ, $"'{fileName}' - Extracting metadata.");
                
                if (!PdfHelper.TryGetCreationDate(filePath, out DateTime creationDate))
                {
                    string errorMsg = $"'{fileName}' - Missing or invalid create date metadata. Skipping file.";
                    _logger.Write(Constants.LOG_ACTION_ERROR, errorMsg);
                    _logger.Write(Constants.LOG_ACTION_DONE, $"'{fileName}' - Processing completed with errors.");
                    return ProcessResult.CreateFailure(filePath, "Missing metadata");
                }

                _logger.Write(Constants.LOG_ACTION_META, $"'{fileName}' - Create date: {creationDate:yyyy-MM-dd HH:mm:ss}");

                // Step 3: Generate new filename
                string newFileName = creationDate.ToString(Constants.FILENAME_DATE_FORMAT) + ".pdf";
                _logger.Write(Constants.LOG_ACTION_RENAME, $"'{fileName}' - Renamed to '{newFileName}'");

                // Step 4: Determine target folder (workspace/YYYY/MM/)
                string yearFolder = creationDate.ToString(Constants.FOLDER_YEAR_FORMAT);
                string monthFolder = creationDate.ToString(Constants.FOLDER_MONTH_FORMAT);
                string parentDir = Path.GetDirectoryName(filePath) ?? string.Empty;
                string targetFolder = Path.Combine(parentDir, yearFolder, monthFolder);

                // Step 5: Ensure target folder exists
                if (!Directory.Exists(targetFolder))
                {
                    _logger.Write(Constants.LOG_ACTION_DIRS, $"'{yearFolder}/{monthFolder}/' - Directory not found. Creating directory.");
                    Directory.CreateDirectory(targetFolder);
                    _logger.Write(Constants.LOG_ACTION_DIRC, $"'{yearFolder}/{monthFolder}/' - Directory created successfully.");
                }
                else
                {
                    _logger.Write(Constants.LOG_ACTION_DIRS, $"'{yearFolder}/{monthFolder}/' - Directory exists.");
                }

                // Step 6: Handle duplicate filenames
                string targetFilePath = Path.Combine(targetFolder, newFileName);
                if (File.Exists(targetFilePath))
                {
                    targetFilePath = GetUniqueFileName(targetFolder, newFileName);
                    string uniqueFileName = Path.GetFileName(targetFilePath);
                    _logger.Write(Constants.LOG_ACTION_DUPLICATE, $"Duplicate detected, renamed to '{uniqueFileName}'");
                    newFileName = uniqueFileName;
                }

                // Step 7: Move file to target folder
                _logger.Write(Constants.LOG_ACTION_MOVE, $"'{newFileName}' - Moving to '{yearFolder}/{monthFolder}/'");
                
                if (!FileHelper.SafeMove(filePath, targetFilePath))
                {
                    string errorMsg = $"Failed to move file to '{yearFolder}/{monthFolder}/'";
                    _logger.Write(Constants.LOG_ACTION_ERROR, errorMsg);
                    _logger.Write(Constants.LOG_ACTION_DONE, $"'{fileName}' - Processing completed with errors.");
                    return ProcessResult.CreateFailure(filePath, "Move failed");
                }

                _logger.Write(Constants.LOG_ACTION_DONE, $"'{fileName}' - Processing completed successfully.");
                return ProcessResult.CreateSuccess(filePath, targetFilePath);
            }
            catch (Exception ex)
            {
                _logger.Write(Constants.LOG_ACTION_ERROR, $"'{fileName}' - Unexpected error: {ex.Message}");
                _logger.Write(Constants.LOG_ACTION_DONE, $"'{fileName}' - Processing completed with errors.");
                return ProcessResult.CreateFailure(filePath, ex.Message);
            }
        }

        /// <summary>
        /// Generates a unique filename by appending sequential number suffix.
        /// </summary>
        /// <param name="folder">Target folder path.</param>
        /// <param name="baseFileName">Base filename (without suffix).</param>
        /// <returns>Full path to unique filename.</returns>
        private string GetUniqueFileName(string folder, string baseFileName)
        {
            string nameWithoutExt = Path.GetFileNameWithoutExtension(baseFileName);
            string extension = Path.GetExtension(baseFileName);

            for (int i = 1; i <= Constants.DUPLICATE_MAX_ATTEMPTS; i++)
            {
                string suffix = string.Format(Constants.DUPLICATE_SUFFIX_FORMAT, i);
                string uniqueName = nameWithoutExt + suffix + extension;
                string uniquePath = Path.Combine(folder, uniqueName);

                if (!File.Exists(uniquePath))
                {
                    return uniquePath;
                }
            }

            // Fallback: use timestamp-based unique name
            string timestampSuffix = $"_{DateTime.Now:yyyyMMddHHmmssfff}";
            return Path.Combine(folder, nameWithoutExt + timestampSuffix + extension);
        }
    }
}
