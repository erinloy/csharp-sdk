using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;
using System.Text.Json;
using System.Threading;

namespace ModelContextProtocol.Client;

/// <inheritdoc/>
internal sealed partial class McpClient : McpEndpoint, IMcpClient
{
    private static Implementation DefaultImplementation { get; } = new()
    {
        Name = DefaultAssemblyName.Name ?? nameof(McpClient),
        Version = DefaultAssemblyName.Version?.ToString() ?? "1.0.0",
    };

    private readonly IClientTransport _clientTransport;
    private readonly McpClientOptions _options;

    private ITransport? _sessionTransport;
    private CancellationTokenSource? _connectCts;

    private ServerCapabilities? _serverCapabilities;
    private Implementation? _serverInfo;
    private string? _serverInstructions;
    private int _disconnectedEventFired;
    private bool _isDisposing;

    /// <inheritdoc/>
    public bool IsConnected
    {
        get
        {
            // Check if we have a session transport
            if (_sessionTransport is null)
                return false;
                
            // Use the transport's IsAlive property which checks process for stdio transports
            return _sessionTransport.IsAlive;
        }
    }

    /// <inheritdoc/>
    public event EventHandler<ConnectedEventArgs>? Connected;

    /// <inheritdoc/>
    public event EventHandler<DisconnectedEventArgs>? Disconnected;

    /// <inheritdoc/>
    public event EventHandler<ConnectionErrorEventArgs>? ConnectionError;

    /// <summary>
    /// Initializes a new instance of the <see cref="McpClient"/> class.
    /// </summary>
    /// <param name="clientTransport">The transport to use for communication with the server.</param>
    /// <param name="options">Options for the client, defining protocol version and capabilities.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    public McpClient(IClientTransport clientTransport, McpClientOptions? options, ILoggerFactory? loggerFactory)
        : base(loggerFactory)
    {
        options ??= new();

        _clientTransport = clientTransport;
        _options = options;

        EndpointName = clientTransport.Name;

        if (options.Capabilities is { } capabilities)
        {
            if (capabilities.NotificationHandlers is { } notificationHandlers)
            {
                NotificationHandlers.RegisterRange(notificationHandlers);
            }

            if (capabilities.Sampling is { } samplingCapability)
            {
                if (samplingCapability.SamplingHandler is not { } samplingHandler)
                {
                    throw new InvalidOperationException("Sampling capability was set but it did not provide a handler.");
                }

                RequestHandlers.Set(
                    RequestMethods.SamplingCreateMessage,
                    (request, _, cancellationToken) => samplingHandler(
                        request,
                        request?.ProgressToken is { } token ? new TokenProgress(this, token) : NullProgress.Instance,
                        cancellationToken),
                    McpJsonUtilities.JsonContext.Default.CreateMessageRequestParams,
                    McpJsonUtilities.JsonContext.Default.CreateMessageResult);
            }

            if (capabilities.Roots is { } rootsCapability)
            {
                if (rootsCapability.RootsHandler is not { } rootsHandler)
                {
                    throw new InvalidOperationException("Roots capability was set but it did not provide a handler.");
                }

                RequestHandlers.Set(
                    RequestMethods.RootsList,
                    (request, _, cancellationToken) => rootsHandler(request, cancellationToken),
                    McpJsonUtilities.JsonContext.Default.ListRootsRequestParams,
                    McpJsonUtilities.JsonContext.Default.ListRootsResult);
            }

            if (capabilities.Elicitation is { } elicitationCapability)
            {
                if (elicitationCapability.ElicitationHandler is not { } elicitationHandler)
                {
                    throw new InvalidOperationException("Elicitation capability was set but it did not provide a handler.");
                }

                RequestHandlers.Set(
                    RequestMethods.ElicitationCreate,
                    (request, _, cancellationToken) => elicitationHandler(request, cancellationToken),
                    McpJsonUtilities.JsonContext.Default.ElicitRequestParams,
                    McpJsonUtilities.JsonContext.Default.ElicitResult);
            }
        }
    }

    /// <inheritdoc/>
    public string? SessionId
    {
        get
        {
            if (_sessionTransport is null)
            {
                throw new InvalidOperationException("Must have already initialized a session when invoking this property.");
            }

            return _sessionTransport.SessionId;
        }
    }

    /// <inheritdoc/>
    public ServerCapabilities ServerCapabilities => _serverCapabilities ?? throw new InvalidOperationException("The client is not connected.");

    /// <inheritdoc/>
    public Implementation ServerInfo => _serverInfo ?? throw new InvalidOperationException("The client is not connected.");

    /// <inheritdoc/>
    public string? ServerInstructions => _serverInstructions;

    /// <inheritdoc/>
    public override string EndpointName { get; }

    /// <summary>
    /// Asynchronously connects to an MCP server, establishes the transport connection, and completes the initialization handshake.
    /// </summary>
    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        // Reset disconnected flag for reconnection scenarios
        _disconnectedEventFired = 0;
        
        _connectCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cancellationToken = _connectCts.Token;

        try
        {
            // Connect transport
            _sessionTransport = await _clientTransport.ConnectAsync(cancellationToken).ConfigureAwait(false);
            InitializeSession(_sessionTransport);
            // We don't want the ConnectAsync token to cancel the session after we've successfully connected.
            // The base class handles cleaning up the session in DisposeAsync without our help.
            StartSession(_sessionTransport, fullSessionCancellationToken: CancellationToken.None);

            // Perform initialization sequence
            using var initializationCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            initializationCts.CancelAfter(_options.InitializationTimeout);

            try
            {
                // Send initialize request
                string requestProtocol = _options.ProtocolVersion ?? McpSession.LatestProtocolVersion;
                var initializeResponse = await this.SendRequestAsync(
                    RequestMethods.Initialize,
                    new InitializeRequestParams
                    {
                        ProtocolVersion = requestProtocol,
                        Capabilities = _options.Capabilities ?? new ClientCapabilities(),
                        ClientInfo = _options.ClientInfo ?? DefaultImplementation,
                    },
                    McpJsonUtilities.JsonContext.Default.InitializeRequestParams,
                    McpJsonUtilities.JsonContext.Default.InitializeResult,
                    cancellationToken: initializationCts.Token).ConfigureAwait(false);

                // Store server information
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    LogServerCapabilitiesReceived(EndpointName,
                        capabilities: JsonSerializer.Serialize(initializeResponse.Capabilities, McpJsonUtilities.JsonContext.Default.ServerCapabilities),
                        serverInfo: JsonSerializer.Serialize(initializeResponse.ServerInfo, McpJsonUtilities.JsonContext.Default.Implementation));
                }

                _serverCapabilities = initializeResponse.Capabilities;
                _serverInfo = initializeResponse.ServerInfo;
                _serverInstructions = initializeResponse.Instructions;

                // Validate protocol version
                bool isResponseProtocolValid =
                    _options.ProtocolVersion is { } optionsProtocol ? optionsProtocol == initializeResponse.ProtocolVersion :
                    McpSession.SupportedProtocolVersions.Contains(initializeResponse.ProtocolVersion);
                if (!isResponseProtocolValid)
                {
                    LogServerProtocolVersionMismatch(EndpointName, requestProtocol, initializeResponse.ProtocolVersion);
                    throw new McpException($"Server protocol version mismatch. Expected {requestProtocol}, got {initializeResponse.ProtocolVersion}");
                }

                // Send initialized notification
                await this.SendNotificationAsync(
                    NotificationMethods.InitializedNotification,
                    new InitializedNotificationParams(),
                    McpJsonUtilities.JsonContext.Default.InitializedNotificationParams,
                    cancellationToken: initializationCts.Token).ConfigureAwait(false);

                // Fire the Connected event
                OnConnected(new ConnectedEventArgs(DateTime.UtcNow, _serverInfo));
                
                // Hook into MessageProcessingTask to detect disconnection
                RegisterDisconnectionDetection();
            }
            catch (OperationCanceledException oce) when (initializationCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
            {
                LogClientInitializationTimeout(EndpointName);
                throw new TimeoutException("Initialization timed out", oce);
            }
        }
        catch (Exception e)
        {
            LogClientInitializationError(EndpointName, e);
            
            // Fire connection error event
            OnConnectionError(new ConnectionErrorEventArgs(e, isRetryable: true));
            
            // Fire disconnected event if we had a transport
            if (_sessionTransport is not null)
            {
                OnDisconnected(new DisconnectedEventArgs(DateTime.UtcNow, isGraceful: false, e));
            }
            
            await DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    /// <inheritdoc/>
    public override async ValueTask DisposeUnsynchronizedAsync()
    {
        _isDisposing = true;
        
        try
        {
            if (_connectCts is not null)
            {
                await _connectCts.CancelAsync().ConfigureAwait(false);
                _connectCts.Dispose();
            }

            // Base class disposal will cancel the session and wait for MessageProcessingTask
            await base.DisposeUnsynchronizedAsync().ConfigureAwait(false);
        }
        finally
        {
            if (_sessionTransport is not null)
            {
                // Fire the Disconnected event before disposing the transport (if not already fired)
                if (_disconnectedEventFired == 0)
                {
                    OnDisconnected(new DisconnectedEventArgs(
                        DateTime.UtcNow, 
                        isGraceful: true,
                        error: null));
                }
                
                await _sessionTransport.DisposeAsync().ConfigureAwait(false);
                _sessionTransport = null;
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "{EndpointName} client received server '{ServerInfo}' capabilities: '{Capabilities}'.")]
    private partial void LogServerCapabilitiesReceived(string endpointName, string capabilities, string serverInfo);

    [LoggerMessage(Level = LogLevel.Error, Message = "{EndpointName} client initialization error.")]
    private partial void LogClientInitializationError(string endpointName, Exception exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "{EndpointName} client initialization timed out.")]
    private partial void LogClientInitializationTimeout(string endpointName);

    [LoggerMessage(Level = LogLevel.Error, Message = "{EndpointName} client protocol version mismatch with server. Expected '{Expected}', received '{Received}'.")]
    private partial void LogServerProtocolVersionMismatch(string endpointName, string expected, string received);
    
    [LoggerMessage(Level = LogLevel.Warning, Message = "Error in {EventName} event handler for {EndpointName}.")]
    private partial void LogEventHandlerError(string eventName, string endpointName, Exception exception);
    
    [LoggerMessage(Level = LogLevel.Warning, Message = "{EndpointName} transport disconnected unexpectedly.")]
    private partial void LogTransportDisconnectedUnexpectedly(string endpointName, Exception? exception);

    /// <summary>
    /// Registers a continuation on MessageProcessingTask to detect disconnection.
    /// </summary>
    private void RegisterDisconnectionDetection()
    {
        if (MessageProcessingTask == null)
        {
            return;
        }
        
        // Register a continuation to detect when message processing ends
        _ = MessageProcessingTask.ContinueWith(task =>
        {
            // Only fire disconnected event if we haven't already and this wasn't a graceful shutdown
            if (Interlocked.CompareExchange(ref _disconnectedEventFired, 1, 0) == 0)
            {
                // Check if this was a graceful shutdown (disposal initiated)
                bool isGraceful = _isDisposing;
                Exception? error = null;
                
                if (!isGraceful)
                {
                    // Ungraceful disconnection - extract error if available
                    if (task.IsFaulted)
                    {
                        error = task.Exception?.GetBaseException();
                    }
                    else if (!task.IsCanceled)
                    {
                        // Task completed normally but we didn't initiate disposal
                        error = new IOException("Transport connection terminated unexpectedly");
                    }
                    
                    if (error != null)
                    {
                        LogTransportDisconnectedUnexpectedly(EndpointName, error);
                    }
                }
                
                OnDisconnected(new DisconnectedEventArgs(
                    DateTime.UtcNow,
                    isGraceful: isGraceful,
                    error: error));
            }
        }, TaskScheduler.Default);
    }
    
    /// <summary>
    /// Raises the Connected event.
    /// </summary>
    private void OnConnected(ConnectedEventArgs e)
    {
        try
        {
            Connected?.Invoke(this, e);
        }
        catch (Exception ex)
        {
            // Log but don't throw - event handlers shouldn't break the connection
            LogEventHandlerError("Connected", EndpointName, ex);
        }
    }

    /// <summary>
    /// Raises the Disconnected event.
    /// </summary>
    private void OnDisconnected(DisconnectedEventArgs e)
    {
        if (Interlocked.CompareExchange(ref _disconnectedEventFired, 1, 0) != 0)
        {
            return; // Already fired
        }
        
        try
        {
            Disconnected?.Invoke(this, e);
        }
        catch (Exception ex)
        {
            // Log but don't throw - event handlers shouldn't break the disconnection
            LogEventHandlerError("Disconnected", EndpointName, ex);
        }
    }

    /// <summary>
    /// Raises the ConnectionError event.
    /// </summary>
    private void OnConnectionError(ConnectionErrorEventArgs e)
    {
        try
        {
            ConnectionError?.Invoke(this, e);
        }
        catch (Exception ex)
        {
            // Log but don't throw - event handlers shouldn't break the error handling
            LogEventHandlerError("ConnectionError", EndpointName, ex);
        }
    }
}