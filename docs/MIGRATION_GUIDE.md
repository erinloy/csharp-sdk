# Migration Guide: Connection Lifecycle Management

## Overview
The connection lifecycle management features are **completely opt-in**. Your existing code will continue to work without any changes.

## For Existing Applications

### No Changes Required
If you're happy with your current implementation, **no action is needed**. The SDK remains fully backward compatible.

### Adding Connection Monitoring
To start using connection lifecycle events, simply subscribe to the events you're interested in:

```csharp
// Existing code - still works
var transport = new StdioClientTransport(options);
var client = await McpClientFactory.CreateAsync(transport);

// Add connection monitoring (optional)
client.Connected += (sender, e) => 
{
    _logger.LogInformation("Connected to {ServerName}", e.ServerInfo.Name);
};

client.Disconnected += (sender, e) =>
{
    if (!e.IsGraceful)
    {
        _logger.LogWarning("Connection lost: {Error}", e.Error?.Message);
    }
};
```

## Common Patterns

### Pattern 1: Simple Connection Status Check
```csharp
// Check before sending requests
if (client.IsConnected)
{
    var response = await client.SendRequestAsync(...);
}
else
{
    _logger.LogWarning("Client not connected, skipping request");
}
```

### Pattern 2: Automatic Reconnection
```csharp
class ResilientMcpClient
{
    private IMcpClient? _client;
    private readonly IClientTransport _transport;
    private int _reconnectAttempts = 0;
    
    public async Task EnsureConnectedAsync()
    {
        if (_client?.IsConnected == true)
            return;
            
        await ConnectWithRetryAsync();
    }
    
    private async Task ConnectWithRetryAsync()
    {
        var backoff = TimeSpan.FromSeconds(Math.Pow(2, Math.Min(_reconnectAttempts, 5)));
        
        try
        {
            _client = await McpClientFactory.CreateAsync(_transport);
            _client.Disconnected += OnDisconnected;
            _reconnectAttempts = 0;
        }
        catch
        {
            _reconnectAttempts++;
            await Task.Delay(backoff);
            await ConnectWithRetryAsync();
        }
    }
    
    private async void OnDisconnected(object? sender, DisconnectedEventArgs e)
    {
        if (!e.IsGraceful)
        {
            await ConnectWithRetryAsync();
        }
    }
}
```

### Pattern 3: Connection Health Monitoring
```csharp
class HealthMonitor
{
    private readonly IMcpClient _client;
    private DateTime? _lastConnected;
    private int _disconnectionCount;
    
    public HealthMonitor(IMcpClient client)
    {
        _client = client;
        _client.Connected += OnConnected;
        _client.Disconnected += OnDisconnected;
    }
    
    private void OnConnected(object? sender, ConnectedEventArgs e)
    {
        _lastConnected = e.ConnectedAt;
        _logger.LogInformation("Connection established with {Server}", e.ServerInfo.Name);
    }
    
    private void OnDisconnected(object? sender, DisconnectedEventArgs e)
    {
        _disconnectionCount++;
        var uptime = _lastConnected.HasValue ? 
            DateTime.UtcNow - _lastConnected.Value : TimeSpan.Zero;
            
        _logger.LogWarning("Connection lost after {Uptime}. Total disconnections: {Count}", 
            uptime, _disconnectionCount);
    }
    
    public bool IsHealthy => _client.IsConnected && _disconnectionCount < 5;
}
```

## Testing Your Migration

### Unit Tests
```csharp
[Fact]
public async Task Should_Handle_Disconnection()
{
    // Arrange
    var transport = new TestTransport();
    var client = await McpClientFactory.CreateAsync(transport);
    var disconnectedFired = false;
    
    client.Disconnected += (s, e) => disconnectedFired = true;
    
    // Act
    transport.SimulateDisconnection();
    
    // Assert
    Assert.True(disconnectedFired);
    Assert.False(client.IsConnected);
}
```

### Integration Tests
```csharp
[Fact]
public async Task Should_Reconnect_After_Server_Restart()
{
    // Start server
    var server = StartTestServer();
    var client = CreateResilientClient();
    
    await client.ConnectAsync();
    Assert.True(client.IsConnected);
    
    // Restart server
    await server.StopAsync();
    await Task.Delay(100);
    Assert.False(client.IsConnected);
    
    await server.StartAsync();
    await Task.Delay(1000); // Wait for reconnection
    
    Assert.True(client.IsConnected);
}
```

## Gradual Adoption Strategy

### Phase 1: Monitoring Only
Start by adding logging to understand your connection patterns:

```csharp
client.Connected += (s, e) => _logger.LogInformation("Connected");
client.Disconnected += (s, e) => _logger.LogWarning("Disconnected: {Reason}", e.Error?.Message);
```

### Phase 2: Alert on Issues
Add metrics and alerting:

```csharp
client.Disconnected += (s, e) =>
{
    if (!e.IsGraceful)
    {
        _metrics.IncrementCounter("mcp.connection.lost");
        _alerting.SendAlert("MCP connection lost", e.Error?.Message);
    }
};
```

### Phase 3: Automatic Recovery
Implement reconnection logic once you understand failure patterns:

```csharp
client.Disconnected += async (s, e) =>
{
    if (!e.IsGraceful && ShouldReconnect(e.Error))
    {
        await ReconnectWithBackoffAsync();
    }
};
```

## Troubleshooting

### Events Not Firing
Ensure you're subscribing to events **before** connection issues occur:

```csharp
// ✅ Correct
var client = await McpClientFactory.CreateAsync(transport);
client.Disconnected += OnDisconnected; // Subscribe immediately

// ❌ Wrong  
var client = await McpClientFactory.CreateAsync(transport);
await DoSomeWork();
client.Disconnected += OnDisconnected; // Too late, might miss events
```

### Memory Leaks
Remember to unsubscribe from events when disposing:

```csharp
public void Dispose()
{
    if (_client != null)
    {
        _client.Connected -= OnConnected;
        _client.Disconnected -= OnDisconnected;
        _client.ConnectionError -= OnConnectionError;
        _client.Dispose();
    }
}
```

## Support
For questions or issues with the migration, please open an issue on GitHub.