# Connection Lifecycle Management

The MCP C# SDK provides comprehensive connection lifecycle management through events and properties that allow you to monitor and respond to connection state changes.

## Overview

Connection lifecycle management is essential for building resilient MCP clients that can:
- Detect when connections are established or lost
- Implement automatic reconnection strategies
- Handle network failures gracefully
- Monitor connection health

## Connection States

The client provides a simple boolean property to check connection status:

```csharp
if (client.IsConnected)
{
    // Client is connected and operational
}
```

## Connection Events

The SDK provides three main events for monitoring connection lifecycle:

### Connected Event

Fired when the client successfully establishes a connection with the server:

```csharp
client.Connected += (sender, e) =>
{
    Console.WriteLine($"Connected to {e.ServerInfo.Name} at {e.ConnectedAt}");
};
```

### Disconnected Event

Fired when the connection is terminated, either gracefully or unexpectedly:

```csharp
client.Disconnected += (sender, e) =>
{
    if (e.IsGraceful)
    {
        Console.WriteLine("Gracefully disconnected");
    }
    else
    {
        Console.WriteLine($"Unexpected disconnection: {e.Error?.Message}");
        // Implement reconnection logic
    }
};
```

### ConnectionError Event

Fired when an error occurs during connection attempts:

```csharp
client.ConnectionError += (sender, e) =>
{
    Console.WriteLine($"Connection error: {e.Error.Message}");
    if (e.IsRetryable)
    {
        // Attempt reconnection
    }
};
```

## Implementing Reconnection

Here's an example of implementing automatic reconnection with exponential backoff:

```csharp
async Task RunClientWithReconnection(ILoggerFactory loggerFactory)
{
    var reconnectAttempts = 0;
    const int maxAttempts = 5;
    
    while (reconnectAttempts < maxAttempts)
    {
        IMcpClient? client = null;
        try
        {
            // Create and connect client
            client = await McpClientFactory.CreateAsync(transport, options, loggerFactory);
            
            // Setup disconnection handler
            var disconnectedTcs = new TaskCompletionSource();
            client.Disconnected += (s, e) =>
            {
                if (!e.IsGraceful)
                {
                    disconnectedTcs.TrySetResult();
                }
            };
            
            // Use the client
            await PerformOperations(client);
            
            // Wait for disconnection
            await disconnectedTcs.Task;
            
            // Disconnected unexpectedly, attempt reconnect
            reconnectAttempts++;
            var delay = TimeSpan.FromSeconds(Math.Pow(2, reconnectAttempts));
            await Task.Delay(delay);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Client error");
            reconnectAttempts++;
            await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, reconnectAttempts)));
        }
        finally
        {
            if (client != null)
            {
                await client.DisposeAsync();
            }
        }
    }
}
```

## Transport-Specific Considerations

### STDIO Transport

For stdio transports, disconnection is detected when the underlying process terminates:

```csharp
var process = Process.Start(new ProcessStartInfo { /* ... */ });
var transport = new StdioClientTransport(
    new StdioClientTransportOptions { Process = process },
    loggerFactory);

// Client will detect when process exits
```

### HTTP/SSE Transport

For HTTP-based transports, disconnection is detected when the SSE stream ends:

```csharp
var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5000") };
var transport = new SseClientTransport(
    new SseClientTransportOptions { HttpClient = new McpHttpClient(httpClient) },
    loggerFactory);

// Client will detect when HTTP connection is lost
```

## Best Practices

1. **Always register event handlers before connecting**:
   ```csharp
   var client = new McpClient(transport, options, loggerFactory);
   client.Connected += OnConnected;
   client.Disconnected += OnDisconnected;
   await client.ConnectAsync(); // Connect after setting up handlers
   ```

2. **Handle event handler exceptions gracefully**:
   Event handler exceptions are logged but don't affect the connection.

3. **Implement connection health monitoring**:
   ```csharp
   using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
   while (await timer.WaitForNextTickAsync())
   {
       if (!client.IsConnected)
       {
           // Handle disconnection
       }
   }
   ```

4. **Clean up properly on shutdown**:
   ```csharp
   await client.DisposeAsync(); // Triggers graceful disconnection event
   ```

## Sample Application

See the [ConnectionLifecycleClient sample](../../../samples/ConnectionLifecycleClient) for a complete example demonstrating:
- Connection event handling
- Automatic reconnection with exponential backoff
- Connection health monitoring
- Graceful shutdown handling

## Thread Safety

All connection events are raised on background threads and are thread-safe. Event handlers should:
- Avoid blocking operations
- Use proper synchronization for shared state
- Handle exceptions internally

## Migration Guide

If you're upgrading from a version without connection lifecycle support:

1. The changes are purely additive - no breaking changes
2. Consider adding connection monitoring for improved reliability
3. Implement reconnection logic for production scenarios
4. Update logging to capture connection state changes