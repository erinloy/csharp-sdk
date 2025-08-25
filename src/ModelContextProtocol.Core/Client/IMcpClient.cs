using ModelContextProtocol.Protocol;

namespace ModelContextProtocol.Client;

/// <summary>
/// Represents an instance of a Model Context Protocol (MCP) client that connects to and communicates with an MCP server.
/// </summary>
public interface IMcpClient : IMcpEndpoint
{
    /// <summary>
    /// Gets a value indicating whether the client is currently connected to the server.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This property provides a non-throwing way to check the connection state.
    /// It returns true only when the client has successfully completed the initialization
    /// handshake with the server and is ready to send and receive messages.
    /// </para>
    /// <para>
    /// Unlike other properties on this interface, this property will not throw an exception
    /// when the client is not connected, making it safe to check at any time.
    /// </para>
    /// </remarks>
    bool IsConnected { get; }

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
    /// Occurs when the client successfully connects and completes initialization with the server.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This event is raised after the client has established a transport connection to the server,
    /// completed the MCP initialization handshake, and is ready to send and receive messages.
    /// </para>
    /// <para>
    /// Event handlers should not perform blocking operations as they may delay the connection process.
    /// For long-running operations triggered by connection events, consider using Task.Run or similar
    /// mechanisms to avoid blocking the connection thread.
    /// </para>
    /// </remarks>
    event EventHandler<ConnectionEventArgs>? Connected;

    /// <summary>
    /// Occurs when the client disconnects from the server, either gracefully or due to an error.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This event is raised when the connection to the server is lost or explicitly closed.
    /// The event args indicate whether the disconnection was graceful (planned) or due to an error.
    /// </para>
    /// <para>
    /// After this event is raised, the client will no longer be able to send or receive messages
    /// until a new connection is established.
    /// </para>
    /// </remarks>
    event EventHandler<DisconnectionEventArgs>? Disconnected;

    /// <summary>
    /// Occurs when the client encounters an error that affects the connection.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This event is raised when connection-related errors occur that may not immediately
    /// result in disconnection but could indicate connection quality issues or temporary problems.
    /// </para>
    /// <para>
    /// Depending on the error severity, the connection may recover automatically or may eventually
    /// result in a <see cref="Disconnected"/> event being raised.
    /// </para>
    /// </remarks>
    event EventHandler<ConnectionErrorEventArgs>? ConnectionError;
}