namespace ModelContextProtocol.Client;

/// <summary>
/// Provides data for disconnection events.
/// </summary>
/// <remarks>
/// This class contains information about a disconnection event, including when the
/// disconnection occurred and whether it was caused by an error or was graceful.
/// </remarks>
public class DisconnectionEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DisconnectionEventArgs"/> class.
    /// </summary>
    /// <param name="disconnectedAt">The date and time when the disconnection occurred.</param>
    /// <param name="error">The error that caused the disconnection, or null if the disconnection was graceful.</param>
    public DisconnectionEventArgs(DateTime disconnectedAt, Exception? error = null)
    {
        DisconnectedAt = disconnectedAt;
        Error = error;
    }

    /// <summary>
    /// Gets the date and time when the disconnection occurred.
    /// </summary>
    public DateTime DisconnectedAt { get; }

    /// <summary>
    /// Gets the error that caused the disconnection, or null if the disconnection was graceful.
    /// </summary>
    /// <remarks>
    /// When this property is null, it indicates that the disconnection was initiated by the
    /// client (e.g., through disposal) and was expected. When this property contains an
    /// exception, it indicates that the disconnection was caused by an unexpected error.
    /// </remarks>
    public Exception? Error { get; }

    /// <summary>
    /// Gets a value indicating whether the disconnection was graceful (not caused by an error).
    /// </summary>
    /// <remarks>
    /// This is a convenience property that returns true when <see cref="Error"/> is null,
    /// indicating the disconnection was intentional and expected.
    /// </remarks>
    public bool IsGraceful => Error == null;
}