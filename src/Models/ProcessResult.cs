namespace PDFsManager.Models
{
    /// <summary>
    /// Result object returned from file processing operations.
    /// Contains success/failure status and error details.
    /// </summary>
    public class ProcessResult
    {
        /// <summary>
        /// Indicates whether the operation was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Error message if the operation failed, otherwise empty.
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Original file path that was processed.
        /// </summary>
        public string? OriginalFilePath { get; set; }

        /// <summary>
        /// New file path after processing (renamed and moved).
        /// </summary>
        public string? NewFilePath { get; set; }

        /// <summary>
        /// Creates a successful result.
        /// </summary>
        public static ProcessResult CreateSuccess(string originalPath, string newPath)
        {
            return new ProcessResult
            {
                Success = true,
                OriginalFilePath = originalPath,
                NewFilePath = newPath
            };
        }

        /// <summary>
        /// Creates a failed result with error message.
        /// </summary>
        public static ProcessResult CreateFailure(string originalPath, string errorMessage)
        {
            return new ProcessResult
            {
                Success = false,
                OriginalFilePath = originalPath,
                ErrorMessage = errorMessage
            };
        }
    }
}
