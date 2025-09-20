namespace OptimizeRK.Models;

/// <summary>
/// Represents the status of a process.
/// </summary>
public enum ProcessStatus {
    /// <summary>
    /// The process has not started yet.
    /// </summary>
    Pending,

    /// <summary>
    /// The process is currently running.
    /// </summary>
    Processing,

    /// <summary>
    /// The process completed successfully and was optimized.
    /// </summary>
    Optimized,

    /// <summary>
    /// The process failed to complete.
    /// </summary>
    Failed,
}
