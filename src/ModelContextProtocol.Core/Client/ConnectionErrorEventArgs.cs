namespace ModelContextProtocol.Client;

/// <summary>
/// Provides data for connection error events.
/// </summary>
/// <remarks>
/// This class contains information about connection errors that don't necessarily
/// result in immediate disconnection but may indicate connection quality issues
/// or temporary problems.
/// </remarks>
public class ConnectionErrorEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionErrorEventArgs"/> class.
    /// </summary>
    /// <param name="error">The error that occurred.</param>
    /// <param name="isRetryable">A value indicating whether the operation that caused this error can be retried.</param>
    public ConnectionErrorEventArgs(Exception error, bool isRetryable = false)
    {
        Error = error ?? throw new ArgumentNullException(nameof(error));
        IsRetryable = isRetryable;
    }

    /// <summary>
    /// Gets the error that occurred.
    /// </summary>
    /// <remarks>
    /// This contains details about what went wrong during a connection-related operation.
    /// The error may or may not result in disconnection depending on its severity.
    /// </remarks>
    public Exception Error { get; }

    /// <summary>
    /// Gets a value indicating whether the operation that caused this error can be retried.
    /// </summary>
    /// <remarks>
    /// When this property is true, it suggests that the error was temporary and the
    /// operation might succeed if retried. When false, it indicates that retry attempts
    /// are unlikely to succeed without addressing the underlying issue.
    /// </remarks>
    public bool IsRetryable { get; }
}