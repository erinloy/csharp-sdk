using ModelContextProtocol.Protocol;

namespace ModelContextProtocol.Client;

/// <summary>
/// Event arguments for connection state changes
/// </summary>
public class ConnectedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the timestamp when the connection was established.
    /// </summary>
    public DateTime ConnectedAt { get; }
    
    /// <summary>
    /// Gets the server implementation information.
    /// </summary>
    public Implementation ServerInfo { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectedEventArgs"/> class.
    /// </summary>
    /// <param name="connectedAt">The timestamp when the connection was established.</param>
    /// <param name="serverInfo">The server implementation information.</param>
    public ConnectedEventArgs(DateTime connectedAt, Implementation serverInfo)
    {
        ConnectedAt = connectedAt;
        ServerInfo = serverInfo;
    }
}

/// <summary>
/// Event arguments for disconnection events
/// </summary>
public class DisconnectedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the timestamp when the disconnection occurred.
    /// </summary>
    public DateTime DisconnectedAt { get; }
    
    /// <summary>
    /// Gets a value indicating whether the disconnection was graceful.
    /// </summary>
    public bool IsGraceful { get; }
    
    /// <summary>
    /// Gets the error that caused the disconnection, if any.
    /// </summary>
    public Exception? Error { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DisconnectedEventArgs"/> class.
    /// </summary>
    /// <param name="disconnectedAt">The timestamp when the disconnection occurred.</param>
    /// <param name="isGraceful">Whether the disconnection was graceful.</param>
    /// <param name="error">The error that caused the disconnection, if any.</param>
    public DisconnectedEventArgs(DateTime disconnectedAt, bool isGraceful, Exception? error = null)
    {
        DisconnectedAt = disconnectedAt;
        IsGraceful = isGraceful;
        Error = error;
    }
}

/// <summary>
/// Event arguments for connection errors
/// </summary>
public class ConnectionErrorEventArgs : EventArgs
{
    /// <summary>
    /// Gets the error that occurred during connection.
    /// </summary>
    public Exception Error { get; }
    
    /// <summary>
    /// Gets a value indicating whether the error is retryable.
    /// </summary>
    public bool IsRetryable { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionErrorEventArgs"/> class.
    /// </summary>
    /// <param name="error">The error that occurred during connection.</param>
    /// <param name="isRetryable">Whether the error is retryable.</param>
    public ConnectionErrorEventArgs(Exception error, bool isRetryable)
    {
        Error = error;
        IsRetryable = isRetryable;
    }
}

/// <summary>
/// Represents an instance of a Model Context Protocol (MCP) client that connects to and communicates with an MCP server.
/// </summary>
public interface IMcpClient : IMcpEndpoint
{
    /// <summary>
    /// Gets the capabilities supported by the connected server.
    /// </summary>
    /// <exception cref="InvalidOperationException">The client is not connected.</exception>
    ServerCapabilities ServerCapabilities { get; }

    /// <summary>
    /// Gets the implementation information of the connected server.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This property provides identification details about the connected server, including its name and version.
    /// It is populated during the initialization handshake and is available after a successful connection.
    /// </para>
    /// <para>
    /// This information can be useful for logging, debugging, compatibility checks, and displaying server
    /// information to users.
    /// </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">The client is not connected.</exception>
    Implementation ServerInfo { get; }

    /// <summary>
    /// Gets any instructions describing how to use the connected server and its features.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This property contains instructions provided by the server during initialization that explain
    /// how to effectively use its capabilities. These instructions can include details about available
    /// tools, expected input formats, limitations, or any other helpful information.
    /// </para>
    /// <para>
    /// This can be used by clients to improve an LLM's understanding of available tools, prompts, and resources. 
    /// It can be thought of like a "hint" to the model and may be added to a system prompt.
    /// </para>
    /// </remarks>
    string? ServerInstructions { get; }

    /// <summary>
    /// Gets a value indicating whether the client is currently connected to the server.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Occurs when the client successfully connects to the server.
    /// </summary>
    event EventHandler<ConnectedEventArgs>? Connected;

    /// <summary>
    /// Occurs when the client disconnects from the server.
    /// </summary>
    event EventHandler<DisconnectedEventArgs>? Disconnected;

    /// <summary>
    /// Occurs when a connection error is detected.
    /// </summary>
    event EventHandler<ConnectionErrorEventArgs>? ConnectionError;
}