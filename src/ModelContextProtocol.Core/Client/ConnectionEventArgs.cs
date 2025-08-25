using ModelContextProtocol.Protocol;

namespace ModelContextProtocol.Client;

/// <summary>
/// Provides data for connection events.
/// </summary>
/// <remarks>
/// This class contains information about a successful connection event,
/// including when the connection was established and details about the connected server.
/// </remarks>
public class ConnectionEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionEventArgs"/> class.
    /// </summary>
    /// <param name="connectedAt">The date and time when the connection was established.</param>
    /// <param name="serverInfo">Information about the connected server.</param>
    /// <param name="serverCapabilities">The capabilities supported by the connected server.</param>
    public ConnectionEventArgs(DateTime connectedAt, Implementation serverInfo, ServerCapabilities serverCapabilities)
    {
        ConnectedAt = connectedAt;
        ServerInfo = serverInfo ?? throw new ArgumentNullException(nameof(serverInfo));
        ServerCapabilities = serverCapabilities ?? throw new ArgumentNullException(nameof(serverCapabilities));
    }

    /// <summary>
    /// Gets the date and time when the connection was established.
    /// </summary>
    public DateTime ConnectedAt { get; }

    /// <summary>
    /// Gets information about the connected server.
    /// </summary>
    /// <remarks>
    /// This contains details such as the server name and version, which can be useful
    /// for logging, debugging, or compatibility checks.
    /// </remarks>
    public Implementation ServerInfo { get; }

    /// <summary>
    /// Gets the capabilities supported by the connected server.
    /// </summary>
    /// <remarks>
    /// This information can be used to determine which features are available
    /// and adapt client behavior accordingly.
    /// </remarks>
    public ServerCapabilities ServerCapabilities { get; }
}