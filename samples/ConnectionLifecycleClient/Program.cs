using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using System.Diagnostics;

namespace ConnectionLifecycleClient;

/// <summary>
/// Sample demonstrating MCP client connection lifecycle events.
/// This client monitors connection state and handles reconnection scenarios.
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        // Setup logging
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        var logger = loggerFactory.CreateLogger<Program>();

        // Determine transport type from args
        var transportType = args.FirstOrDefault() ?? "stdio";
        
        try
        {
            if (transportType == "stdio")
            {
                await RunStdioClientWithReconnection(logger, loggerFactory);
            }
            else if (transportType == "http")
            {
                await RunHttpClientWithMonitoring(logger, loggerFactory);
            }
            else
            {
                logger.LogError("Unknown transport type: {TransportType}. Use 'stdio' or 'http'", transportType);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fatal error in client");
        }
    }

    /// <summary>
    /// Demonstrates stdio transport with automatic reconnection on disconnection
    /// </summary>
    static async Task RunStdioClientWithReconnection(ILogger logger, ILoggerFactory loggerFactory)
    {
        var reconnectAttempts = 0;
        const int maxReconnectAttempts = 3;
        IMcpClient? client = null;

        while (reconnectAttempts < maxReconnectAttempts)
        {
            try
            {
                logger.LogInformation("Starting QuickstartWeatherServer process (attempt {Attempt}/{Max})...",
                    reconnectAttempts + 1, maxReconnectAttempts);

                // Start the server process
                var serverProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "run --project ../QuickstartWeatherServer/QuickstartWeatherServer.csproj",
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                });

                if (serverProcess == null)
                {
                    logger.LogError("Failed to start server process");
                    break;
                }

                // Create client with stdio transport
                var transport = new StdioClientTransport(
                    new StdioClientTransportOptions { Process = serverProcess },
                    loggerFactory);

                client = await McpClientFactory.CreateAsync(
                    transport,
                    new McpClientOptions
                    {
                        ClientInfo = new Implementation
                        {
                            Name = "ConnectionLifecycleClient",
                            Version = "1.0.0"
                        }
                    },
                    loggerFactory);

                // Setup connection lifecycle event handlers
                SetupConnectionEventHandlers(client, logger);

                // Monitor connection state
                logger.LogInformation("Client connected. IsConnected: {IsConnected}", client.IsConnected);

                // Use the client
                await UseClient(client, logger);

                // Keep the client running until disconnection
                var disconnectionTcs = new TaskCompletionSource();
                client.Disconnected += (s, e) => disconnectionTcs.TrySetResult();

                logger.LogInformation("Client is running. Press Ctrl+C to exit or wait for server disconnection...");
                
                // Wait for disconnection or user cancellation
                using var cts = new CancellationTokenSource();
                Console.CancelKeyPress += (s, e) =>
                {
                    e.Cancel = true;
                    cts.Cancel();
                };

                try
                {
                    await Task.WhenAny(
                        disconnectionTcs.Task,
                        Task.Delay(Timeout.Infinite, cts.Token));
                }
                catch (OperationCanceledException)
                {
                    logger.LogInformation("User requested shutdown");
                    break;
                }

                // If we got here due to disconnection, attempt reconnect
                if (disconnectionTcs.Task.IsCompleted)
                {
                    logger.LogWarning("Server disconnected. Will attempt to reconnect...");
                    reconnectAttempts++;
                    
                    // Wait before reconnecting
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, reconnectAttempts))); // Exponential backoff
                }
                else
                {
                    break; // User requested exit
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in client operation");
                reconnectAttempts++;
                
                if (reconnectAttempts < maxReconnectAttempts)
                {
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, reconnectAttempts)));
                }
            }
            finally
            {
                if (client != null)
                {
                    await client.DisposeAsync();
                    client = null;
                }
            }
        }

        if (reconnectAttempts >= maxReconnectAttempts)
        {
            logger.LogError("Maximum reconnection attempts reached. Exiting.");
        }
    }

    /// <summary>
    /// Demonstrates HTTP transport with connection monitoring
    /// </summary>
    static async Task RunHttpClientWithMonitoring(ILogger logger, ILoggerFactory loggerFactory)
    {
        var serverUrl = "http://localhost:5000/mcp";
        logger.LogInformation("Connecting to HTTP server at {Url}...", serverUrl);

        var httpClient = new HttpClient { BaseAddress = new Uri(serverUrl) };
        var transport = new SseClientTransport(
            new SseClientTransportOptions { HttpClient = new McpHttpClient(httpClient) },
            loggerFactory);

        var client = await McpClientFactory.CreateAsync(
            transport,
            new McpClientOptions
            {
                ClientInfo = new Implementation
                {
                    Name = "ConnectionLifecycleClient",
                    Version = "1.0.0"
                }
            },
            loggerFactory);

        // Setup connection lifecycle event handlers
        SetupConnectionEventHandlers(client, logger);

        // Periodically check connection status
        using var statusTimer = new PeriodicTimer(TimeSpan.FromSeconds(10));
        var statusTask = Task.Run(async () =>
        {
            while (await statusTimer.WaitForNextTickAsync())
            {
                logger.LogInformation("Connection status: IsConnected={IsConnected}", client.IsConnected);
                
                if (!client.IsConnected)
                {
                    logger.LogWarning("Connection lost! Consider implementing reconnection logic.");
                    break;
                }
            }
        });

        // Use the client
        await UseClient(client, logger);

        // Wait for user to exit
        logger.LogInformation("Press any key to exit...");
        Console.ReadKey();

        await client.DisposeAsync();
    }

    /// <summary>
    /// Sets up event handlers for connection lifecycle events
    /// </summary>
    static void SetupConnectionEventHandlers(IMcpClient client, ILogger logger)
    {
        client.Connected += (sender, e) =>
        {
            logger.LogInformation("🟢 Connected to server: {ServerName} v{ServerVersion} at {Time}",
                e.ServerInfo.Name,
                e.ServerInfo.Version,
                e.ConnectedAt.ToLocalTime());
        };

        client.Disconnected += (sender, e) =>
        {
            if (e.IsGraceful)
            {
                logger.LogInformation("🔵 Gracefully disconnected at {Time}",
                    e.DisconnectedAt.ToLocalTime());
            }
            else
            {
                logger.LogWarning("🔴 Unexpectedly disconnected at {Time}. Error: {Error}",
                    e.DisconnectedAt.ToLocalTime(),
                    e.Error?.Message ?? "Unknown");
            }
        };

        client.ConnectionError += (sender, e) =>
        {
            logger.LogError(e.Error, "⚠️ Connection error occurred. Retryable: {IsRetryable}",
                e.IsRetryable);
        };
    }

    /// <summary>
    /// Uses the client to perform operations
    /// </summary>
    static async Task UseClient(IMcpClient client, ILogger logger)
    {
        try
        {
            // List available tools
            var tools = await client.ListToolsAsync();
            logger.LogInformation("Available tools: {Tools}",
                string.Join(", ", tools.Tools.Select(t => t.Name)));

            // Call a tool if available
            if (tools.Tools.Any(t => t.Name == "get_forecast"))
            {
                var result = await client.CallToolAsync(
                    "get_forecast",
                    new { city = "Seattle", days = 3 });
                
                logger.LogInformation("Weather forecast retrieved successfully");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error using client");
        }
    }
}