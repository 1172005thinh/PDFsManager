using System;
using System.IO;
using System.Threading;
using PDFsManager.Utils;

namespace PDFsManager.Utils
{
    /// <summary>
    /// File system helper utilities for file lock detection and retry logic.
    /// </summary>
    public static class FileHelper
    {
        /// <summary>
        /// Checks if a file is currently locked or in use.
        /// </summary>
        /// <param name="filePath">Path to the file to check.</param>
        /// <returns>True if file is locked, false if accessible.</returns>
        public static bool IsFileLocked(string filePath)
        {
            try
            {
                using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
                return false; // File is not locked
            }
            catch (IOException)
            {
                return true; // File is locked
            }
            catch (UnauthorizedAccessException)
            {
                return true; // Access denied (treat as locked)
            }
        }

        /// <summary>
        /// Attempts to perform an action on a file with retry logic for locked files.
        /// </summary>
        /// <param name="filePath">Path to the file.</param>
        /// <param name="action">Action to perform on the file.</param>
        /// <param name="onRetry">Optional callback invoked on each retry attempt.</param>
        /// <returns>True if action succeeded, false if file remained locked after retries.</returns>
        public static bool TryWithRetry(string filePath, Action action, Action<int>? onRetry = null)
        {
            for (int attempt = 0; attempt <= Constants.FILE_LOCK_RETRY_COUNT; attempt++)
            {
                if (!IsFileLocked(filePath))
                {
                    try
                    {
                        action();
                        return true;
                    }
                    catch (IOException)
                    {
                        // File became locked during action
                        if (attempt == Constants.FILE_LOCK_RETRY_COUNT)
                            return false;
                    }
                }

                if (attempt < Constants.FILE_LOCK_RETRY_COUNT)
                {
                    onRetry?.Invoke(attempt + 1);
                    Thread.Sleep(Constants.FILE_LOCK_RETRY_DELAY_MS);
                }
            }

            return false; // File remained locked after all retries
        }

        /// <summary>
        /// Safely moves a file, ensuring the target directory exists.
        /// </summary>
        /// <param name="sourceFilePath">Source file path.</param>
        /// <param name="targetFilePath">Target file path.</param>
        /// <returns>True if move succeeded, false otherwise.</returns>
        public static bool SafeMove(string sourceFilePath, string targetFilePath)
        {
            try
            {
                // Ensure target directory exists
                string? targetDir = Path.GetDirectoryName(targetFilePath);
                if (!string.IsNullOrEmpty(targetDir) && !Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }

                File.Move(sourceFilePath, targetFilePath);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
