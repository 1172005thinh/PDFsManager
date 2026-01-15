namespace PDFsManager.Models
{
    /// <summary>
    /// Application state enumeration.
    /// Defines the current operational state of the PDFs Manager.
    /// </summary>
    public enum AppState
    {
        /// <summary>
        /// Initial state, no workspace configured.
        /// </summary>
        IDLE,

        /// <summary>
        /// Valid workspace configured, but monitoring is paused.
        /// </summary>
        STOPPED,

        /// <summary>
        /// Active monitoring and file processing in progress.
        /// </summary>
        RUNNING,

        /// <summary>
        /// Critical error encountered, requires user intervention.
        /// </summary>
        ERROR
    }
}
